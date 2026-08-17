import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import {
  catchError,
  forkJoin,
  map,
  Observable,
  of,
  switchMap,
  tap,
  throwError
} from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import {
  AsignarRevisorPlaneacionRequestDto,
  GeneracionPlaneacionDetalleDto,
  GeneracionPlaneacionesResultadoDto,
  PlaneacionDetalleConArchivosResponseDto,
  PlaneacionEdicionResponseDto,
  PlaneacionResumenResponseDto
} from '../dto/planeaciones/planeaciones-directivo.dto';
import { UsuarioBackendListItem } from '../models/admin-catalogos.model';
import {
  GeneracionPlaneacionVisual,
  GeneracionPlaneacionesVisualResultado,
  PlaneacionAsignacionRevisor
} from '../models/planeacion-directivo.model';
import { UsuariosService } from './usuarios.service';

type ReviewerOption = { label: string; value: string };

@Injectable({ providedIn: 'root' })
export class PlaneacionesDirectivoService {
  private readonly http = inject(HttpClient);
  private readonly usuariosService = inject(UsuariosService);
  private readonly generationEndpoint =
    `${environment.apiUrl}/api/Planeaciones/generar`;
  private readonly flowEndpoint =
    `${environment.apiUrl}/api/planeaciones-flujo`;
  private readonly planeacionesEndpoint =
    `${environment.apiUrl}/api/planeaciones`;
  private readonly sessionPlaneaciones =
    signal<PlaneacionAsignacionRevisor[]>([]);
  private readonly userNames = new Map<string, string>();

  readonly reviewerOptions: ReviewerOption[] = [];

  generate(): Observable<GeneracionPlaneacionesVisualResultado> {
    return this.http.post<
      ApiResponseDto<GeneracionPlaneacionesResultadoDto>
    >(this.generationEndpoint, {}).pipe(
      map(response => this.unwrap(response)),
      switchMap(result => this.enrichGeneration(result)),
      tap(result => this.rememberGeneration(result.planeaciones))
    );
  }

  load(): Observable<PlaneacionAsignacionRevisor[]> {
    return forkJoin({
      users: this.usuariosService.load(),
      planeaciones: of(this.sessionPlaneaciones())
    }).pipe(
      map(({ users, planeaciones }) => {
        this.setUserCatalog(users);
        return planeaciones.map(item => ({ ...item }));
      })
    );
  }

  create(): Observable<never> {
    return throwError(() => new Error(
      'Las planeaciones solo pueden crearse mediante Generar planeaciones.'
    ));
  }

  update(
    item: Record<string, unknown>
  ): Observable<PlaneacionAsignacionRevisor> {
    const publicId = this.requiredString(item, 'publicId');
    const revisorPublicId = this.requiredString(item, 'revisorPublicId');
    const current = this.sessionPlaneaciones().find(
      planeacion => planeacion.publicId === publicId
    );

    if (!current) {
      return throwError(() => new Error(
        'La planeación ya no está disponible en esta sesión.'
      ));
    }

    if (!this.reviewerOptions.some(option => option.value === revisorPublicId)) {
      return throwError(() => new Error(
        'Selecciona un usuario backend con rol global Revisor.'
      ));
    }

    const request: AsignarRevisorPlaneacionRequestDto = {
      revisorPublicId
    };

    return this.http.post<ApiResponseDto<PlaneacionResumenResponseDto>>(
      `${this.flowEndpoint}/${publicId}/asignar-revisor`,
      request
    ).pipe(
      map(response => this.unwrap(response)),
      map(response => this.toUiModel(response, current.resultadoGeneracion)),
      tap(updated => this.sessionPlaneaciones.update(items =>
        items.map(candidate =>
          candidate.publicId === updated.publicId ? updated : candidate
        )
      ))
    );
  }

  delete(): Observable<never> {
    return throwError(() => new Error(
      'El backend no expone una baja de planeación dentro de este flujo.'
    ));
  }

  private rememberGeneration(
    details: GeneracionPlaneacionVisual[]
  ): void {
    const byPublicId = new Map(
      this.sessionPlaneaciones().map(item => [item.publicId, item])
    );

    for (const detail of details) {
      if (!detail.planeacionPublicId) continue;

      const current = byPublicId.get(detail.planeacionPublicId);
      byPublicId.set(detail.planeacionPublicId, {
        id: detail.planeacionPublicId,
        publicId: detail.planeacionPublicId,
        asignatura: detail.asignatura,
        docente: detail.docentes || current?.docente || '',
        periodo: detail.periodo || current?.periodo || '',
        grupos: detail.grupos || current?.grupos || '',
        estado: detail.estado || current?.estado || '',
        revisorPublicId: current?.revisorPublicId ?? '',
        revisorNombre: current?.revisorNombre ?? 'Sin asignar',
        resultadoGeneracion: detail.resultadoGeneracion
      });
    }

    this.sessionPlaneaciones.set([...byPublicId.values()]);
  }

  private setUserCatalog(users: UsuarioBackendListItem[]): void {
    this.userNames.clear();

    for (const user of users) {
      if (user.source !== 'backend') continue;
      this.userNames.set(user.publicId, this.fullName(user));
    }

    this.reviewerOptions.splice(
      0,
      this.reviewerOptions.length,
      ...users
        .filter(user =>
          user.source === 'backend' && user.roles.includes('REVISOR')
        )
        .map(user => {
          const name = this.userNames.get(user.publicId) ?? '';

          return {
            value: user.publicId,
            label: name || user.email
          };
        })
        .sort((left, right) => left.label.localeCompare(right.label))
    );
  }

  private toUiModel(
    response: PlaneacionResumenResponseDto,
    resultadoGeneracion: string
  ): PlaneacionAsignacionRevisor {
    return {
      id: response.publicId,
      publicId: response.publicId,
      asignatura: response.asignatura,
      docente: response.docentes,
      periodo: response.periodo,
      grupos: response.grupos,
      estado: this.estadoLabel(response.estado),
      revisorPublicId: response.revisorPublicId ?? '',
      revisorNombre: response.revisor?.trim() ||
        (response.revisorPublicId
          ? this.userNames.get(response.revisorPublicId)
          : null) ||
        'Sin asignar',
      resultadoGeneracion
    };
  }

  private enrichGeneration(
    result: GeneracionPlaneacionesResultadoDto
  ): Observable<GeneracionPlaneacionesVisualResultado> {
    const detailRequests = result.planeaciones.map(detail => {
      if (!detail.planeacionPublicId) {
        return of(this.toGenerationModel(detail));
      }

      return this.http.get<
        ApiResponseDto<PlaneacionDetalleConArchivosResponseDto>
      >(`${this.planeacionesEndpoint}/${detail.planeacionPublicId}`).pipe(
        map(response => this.unwrap(response).planeacion),
        map(planeacion => this.toGenerationModel(detail, planeacion)),
        catchError(() => of(this.toGenerationModel(detail)))
      );
    });

    const enriched = detailRequests.length > 0
      ? forkJoin(detailRequests)
      : of([] as GeneracionPlaneacionVisual[]);

    return enriched.pipe(map(planeaciones => ({
      totalProgramas: result.totalProgramas,
      planeacionesCreadas: result.planeacionesCreadas,
      yaExistentes: result.yaExistentes,
      omitidas: result.omitidas,
      planeaciones
    })));
  }

  private toGenerationModel(
    detail: GeneracionPlaneacionDetalleDto,
    planeacion?: PlaneacionEdicionResponseDto
  ): GeneracionPlaneacionVisual {
    const caratula = planeacion?.caratula;

    return {
      programaAsignaturaPublicId: detail.programaAsignaturaPublicId,
      planeacionPublicId: detail.planeacionPublicId,
      asignatura: caratula?.nombreAsignatura?.trim() || detail.asignatura,
      docentes: caratula?.docentes?.trim() ?? '',
      periodo: caratula?.periodoEscolar?.trim() ?? '',
      grupos: caratula?.grupos?.trim() ?? '',
      estado: planeacion
        ? this.estadoLabel(planeacion.estado)
        : this.resultadoGeneracionLabel(detail.estado),
      resultado: detail.mensaje?.trim() ||
        this.resultadoGeneracionMensaje(detail.estado),
      resultadoGeneracion: detail.estado
    };
  }

  private resultadoGeneracionLabel(value: string): string {
    return ({
      creada: 'Creada',
      existente: 'Existente',
      omitida: 'Omitida',
      error: 'Error'
    } as Record<string, string>)[value.toLowerCase()] ?? value;
  }

  private resultadoGeneracionMensaje(value: string): string {
    return ({
      creada: 'Planeación creada correctamente.',
      existente: 'La planeación ya existía.',
      omitida: 'La planeación fue omitida.',
      error: 'No fue posible generar la planeación.'
    } as Record<string, string>)[value.toLowerCase()] ?? value;
  }

  private estadoLabel(value: number): string {
    return ({
      1: 'Borrador',
      2: 'En proceso',
      3: 'En revisión',
      4: 'Correcciones',
      5: 'Aprobada',
      6: 'Rechazada',
      7: 'Finalizada',
      8: 'Reabierta'
    } as Record<number, string>)[value] ?? '';
  }

  private fullName(user: UsuarioBackendListItem): string {
    return [user.nombre, user.apellidoPaterno, user.apellidoMaterno]
      .filter(Boolean)
      .join(' ')
      .trim();
  }

  private requiredString(
    item: Record<string, unknown>,
    key: string
  ): string {
    const value = item[key];

    if (typeof value !== 'string' || !value.trim()) {
      throw new Error(`El campo ${key} es obligatorio.`);
    }

    return value.trim();
  }

  private unwrap<T>(response: ApiResponseDto<T>): T {
    if (!response.success || response.data === null) {
      throw new Error(
        response.errors?.join(' ') ||
          response.message ||
          environment.defaultErrorMessage
      );
    }

    return response.data;
  }
}
