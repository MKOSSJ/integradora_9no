import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { forkJoin, map, Observable, tap } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import {
  ComentarioCorreccionDto,
  ComentariosCorreccionDto,
  PlaneacionEdicionDto,
  PlaneacionResumenDto
} from '../dto/planeaciones/planeacion-flujo.dto';
import { PlaneacionStatus } from '../models/planeacion.model';
import { RevisionDetail, RevisionItem } from '../models/validacion.model';

@Injectable({ providedIn: 'root' })
export class ValidacionService {
  private readonly http = inject(HttpClient);
  private readonly flowEndpoint = `${environment.apiUrl}/api/planeaciones-flujo`;
  private readonly commentsEndpoint = `${environment.apiUrl}/api/planeaciones`;
  private readonly summaries = new Map<string, PlaneacionResumenDto>();

  getRevisions(): Observable<RevisionItem[]> {
    return this.http
      .get<ApiResponseDto<PlaneacionResumenDto[]>>(`${this.flowEndpoint}/revisiones`)
      .pipe(
        map(response => this.unwrap(response)),
        tap(revisions => {
          this.summaries.clear();
          revisions.forEach(revision => this.summaries.set(revision.publicId, revision));
        }),
        map(revisions => revisions.map(revision => this.toListItem(revision)))
      );
  }

  getRevisionById(publicId: string): Observable<RevisionDetail> {
    return forkJoin({
      detail: this.http
        .get<ApiResponseDto<PlaneacionEdicionDto>>(
          `${this.flowEndpoint}/revisiones/${publicId}`
        )
        .pipe(map(response => this.unwrap(response))),
      comments: this.http
        .get<ApiResponseDto<ComentariosCorreccionDto>>(
          `${this.commentsEndpoint}/${publicId}/comentarios-correccion`
        )
        .pipe(map(response => this.unwrap(response)))
    }).pipe(map(({ detail, comments }) => this.toDetail(detail, comments)));
  }

  resolveRevision(publicId: string, estado: 4 | 5): Observable<PlaneacionStatus> {
    return this.http
      .post<ApiResponseDto<PlaneacionResumenDto>>(
        `${this.flowEndpoint}/revisiones/${publicId}/estado`,
        { estado }
      )
      .pipe(
        map(response => this.unwrap(response)),
        tap(summary => this.summaries.set(summary.publicId, summary)),
        map(summary => this.toStatus(summary.estado))
      );
  }

  addComment(publicId: string, mensaje: string): Observable<ComentarioCorreccionDto> {
    return this.http
      .post<ApiResponseDto<ComentarioCorreccionDto>>(
        `${this.commentsEndpoint}/${publicId}/comentarios-correccion`,
        { mensaje: mensaje.trim() }
      )
      .pipe(map(response => this.unwrap(response)));
  }

  private toListItem(summary: PlaneacionResumenDto): RevisionItem {
    return {
      id: summary.publicId,
      titulo: summary.asignatura,
      autor: summary.docentes,
      estado: this.toStatus(summary.estado),
      fechaEnvio: summary.ultimaModificacion ?? '',
      carrera: '',
      grupo: summary.grupos
    };
  }

  private toDetail(dto: PlaneacionEdicionDto, comments: ComentariosCorreccionDto): RevisionDetail {
    const summary = this.summaries.get(dto.publicId);
    const caratula = dto.caratula;
    const title = caratula.nombreAsignatura?.trim() || summary?.asignatura || '';
    const status = this.toStatus(dto.estado);
    const updatedAt = summary?.ultimaModificacion ?? '';

    return {
      id: dto.publicId,
      titulo: title,
      descripcion: [caratula.periodoEscolar, caratula.grupos, caratula.docentes]
        .filter((value): value is string => !!value?.trim())
        .join(' · '),
      actualizacion: updatedAt,
      progreso: status === 'aprobado' ? 100 : 0,
      status,
      reviewStatus: status,
      autor: caratula.docentes ?? '',
      enviadoPor: caratula.docentes ?? '',
      carrera: caratula.programaEducativo ?? '',
      grupo: caratula.grupos ?? '',
      fechaCreacion: '',
      ultimaModificacion: updatedAt,
      fechaEnvio: updatedAt,
      fechaEnvioRevision: updatedAt,
      pdfPages: 0,
      comentariosRevision: comments.comentarios.map(comment => `${comment.usuario}: ${comment.mensaje}`),
      programa: {
        nombre: title,
        clave: '',
        programaEducativo: caratula.programaEducativo ?? '',
        cuatrimestre: caratula.cuatrimestre?.toString() ?? '',
        creditos: caratula.creditos ?? 0,
        horasTotales: caratula.horasTotales ?? 0,
        horasSaber: caratula.horasSaber ?? 0,
        horasSaberHacer: caratula.horasSaberHacer ?? 0,
        horasSemana: caratula.horasSemana ?? 0,
        proposito: caratula.propositoAsignatura ?? '',
        competencia: caratula.competenciaAsignatura ?? '',
        tipoCompetencia: this.tipoCompetencia(caratula.tipoCompetencia),
        modalidad: this.modalidad(caratula.modalidad),
        referenciasBase: []
      },
      formulario: {
        titulo: title,
        periodoId: null,
        asignaturaId: null,
        academiaId: null,
        programaAsignaturaId: null,
        revisorId: null,
        docenteIds: [],
        grupoIds: [],
        caratula: {
          programaEducativo: caratula.programaEducativo ?? '',
          docentes: caratula.docentes ?? '',
          cuatrimestre: caratula.cuatrimestre?.toString() ?? '',
          periodoEscolar: caratula.periodoEscolar ?? '',
          asignatura: title,
          grupos: caratula.grupos ?? '',
          propositoAsignatura: caratula.propositoAsignatura ?? '',
          competenciaContribuye: caratula.competenciaAsignatura ?? '',
          tipoCompetencia: this.tipoCompetencia(caratula.tipoCompetencia),
          creditos: caratula.creditos ?? 0,
          modalidad: this.modalidad(caratula.modalidad),
          horasSaber: caratula.horasSaber ?? 0,
          horasSaberHacer: caratula.horasSaberHacer ?? 0,
          horasTotales: caratula.horasTotales ?? 0,
          horasSemana: caratula.horasSemana ?? 0
        },
        unidades: []
      }
    };
  }

  private toStatus(value: number): PlaneacionStatus {
    const status = ({
      1: 'borrador', 2: 'en-proceso', 3: 'revision', 4: 'correcciones',
      5: 'aprobado', 6: 'rechazada', 7: 'finalizada', 8: 'reabierta'
    } as Record<number, PlaneacionStatus>)[value];

    if (!status) throw new Error(`El estado de planeación ${value} no es válido.`);
    return status;
  }

  private tipoCompetencia(value: string | null) {
    return value === 'Base' || value === 'Transversal' || value === 'Específica' ? value : '';
  }

  private modalidad(value: string | null) {
    return value === 'Escolarizada' || value === 'Mixta' || value === 'Dual' || value === 'No escolarizada' ? value : '';
  }

  private unwrap<T>(response: ApiResponseDto<T>): T {
    if (!response.success || response.data === null) {
      throw new Error(response.errors?.join(' ') || response.message || environment.defaultErrorMessage);
    }
    return response.data;
  }
}
