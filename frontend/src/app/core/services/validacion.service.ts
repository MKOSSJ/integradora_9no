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
import { PlaneacionesService } from './planeaciones.service';

@Injectable({ providedIn: 'root' })
export class ValidacionService {
  private readonly http = inject(HttpClient);
  private readonly planeacionesService = inject(PlaneacionesService);
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
    const detail = this.planeacionesService.toDetail(dto);
    const status = detail.status;
    const updatedAt = summary?.ultimaModificacion ?? '';

    return {
      ...detail,
      actualizacion: updatedAt,
      ultimaModificacion: updatedAt,
      progreso: status === 'aprobado' ? 100 : 0,
      reviewStatus: status,
      fechaEnvio: updatedAt,
      fechaEnvioRevision: updatedAt,
      enviadoPor: detail.autor,
      carrera: detail.programa.programaEducativo,
      grupo: detail.formulario.caratula.grupos,
      comentariosRevision: comments.comentarios.map(comment => `${comment.usuario}: ${comment.mensaje}`)
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
