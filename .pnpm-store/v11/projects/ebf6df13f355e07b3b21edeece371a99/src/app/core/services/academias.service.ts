import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { forkJoin, map, Observable, of, switchMap } from 'rxjs';

import { environment } from '../../environments/environments';
import { AcademiaRequestDto } from '../dto/academias/academia-request.dto';
import { AcademiaUsuarioResponseDto } from '../dto/academias/academia-usuario-response.dto';
import { ApiResponseDto } from '../dto/api-response.dto';
import {
  AcademiaResponseDto,
  AsignaturaResponseDto
} from '../dto/asignaturas/asignatura-response.dto';
import { Academia, EntityStatus } from '../models/admin-catalogos.model';

type UiFormData = Record<string, unknown>;
type UiIdentity = { publicId?: unknown };

@Injectable({
  providedIn: 'root'
})
export class AcademiasService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiUrl}/api/Academias`;
  private readonly asignaturasEndpoint = `${environment.apiUrl}/api/Asignaturas`;

  load(): Observable<Academia[]> {
    return forkJoin({
      academias: this.http
        .get<ApiResponseDto<AcademiaResponseDto[]>>(this.endpoint)
        .pipe(map(response => this.unwrap(response))),
      asignaturas: this.http
        .get<ApiResponseDto<AsignaturaResponseDto[]>>(this.asignaturasEndpoint)
        .pipe(map(response => this.unwrap(response)))
    }).pipe(
      switchMap(({ academias, asignaturas }) => {
        if (academias.length === 0) {
          return of([]);
        }

        const totalAsignaturas = this.countAsignaturasByAcademia(asignaturas);

        return forkJoin(
          academias.map(academia =>
            this.http
              .get<ApiResponseDto<AcademiaUsuarioResponseDto[]>>(
                `${this.endpoint}/${academia.publicId}/usuarios`
              )
              .pipe(
                map(response => this.unwrap(response)),
                map(usuarios =>
                  this.toUiModel(
                    academia,
                    usuarios.length,
                    totalAsignaturas.get(academia.publicId) ?? 0
                  )
                )
              )
          )
        );
      })
    );
  }

  create(item: UiFormData): Observable<Academia> {
    const requestedStatus = item['estado'];

    return this.http
      .post<ApiResponseDto<AcademiaResponseDto>>(
        this.endpoint,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        map(academia => this.toUiModel(academia, 0, 0)),
        switchMap(academia => {
          if (requestedStatus !== 'inactivo') {
            return of(academia);
          }

          return this.delete(academia).pipe(
            map((): Academia => ({ ...academia, estado: 'inactivo' }))
          );
        })
      );
  }

  update(item: UiFormData): Observable<Academia> {
    if (item['estado'] === 'inactivo') {
      return this.delete(item).pipe(
        map(() => this.toUiModelFromForm(item, 'inactivo'))
      );
    }

    const publicId = this.getRequiredString(item, 'publicId');
    const totalUsuarios = this.getCount(item, 'totalUsuarios');
    const totalAsignaturas = this.getCount(item, 'totalAsignaturas');

    return this.http
      .put<ApiResponseDto<AcademiaResponseDto>>(
        `${this.endpoint}/${publicId}`,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        map(academia =>
          this.toUiModel(academia, totalUsuarios, totalAsignaturas)
        )
      );
  }

  delete(item: UiIdentity): Observable<boolean> {
    const publicId = this.getPublicId(item);

    return this.http
      .delete<ApiResponseDto<boolean>>(`${this.endpoint}/${publicId}`)
      .pipe(map(response => this.unwrap(response)));
  }

  private countAsignaturasByAcademia(
    asignaturas: AsignaturaResponseDto[]
  ): Map<string, number> {
    const counts = new Map<string, number>();

    for (const asignatura of asignaturas) {
      if (!asignatura.academiaPublicId) continue;

      counts.set(
        asignatura.academiaPublicId,
        (counts.get(asignatura.academiaPublicId) ?? 0) + 1
      );
    }

    return counts;
  }

  private toRequestDto(item: UiFormData): AcademiaRequestDto {
    return {
      nombre: this.getRequiredString(item, 'nombre'),
      descripcion: this.getOptionalString(item, 'descripcion')
    };
  }

  private toUiModel(
    academia: AcademiaResponseDto,
    totalUsuarios: number,
    totalAsignaturas: number
  ): Academia {
    return {
      id: academia.publicId,
      publicId: academia.publicId,
      nombre: academia.nombre,
      descripcion: academia.descripcion ?? '',
      estado: academia.activo ? 'activo' : 'inactivo',
      totalUsuarios,
      totalAsignaturas
    };
  }

  private toUiModelFromForm(
    item: UiFormData,
    estado: EntityStatus
  ): Academia {
    const publicId = this.getRequiredString(item, 'publicId');

    return {
      id: publicId,
      publicId,
      nombre: this.getRequiredString(item, 'nombre'),
      descripcion: this.getOptionalString(item, 'descripcion') ?? '',
      estado,
      totalUsuarios: this.getCount(item, 'totalUsuarios'),
      totalAsignaturas: this.getCount(item, 'totalAsignaturas')
    };
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

  private getRequiredString(item: UiFormData, key: string): string {
    const value = item[key];

    if (typeof value !== 'string' || value.trim() === '') {
      throw new Error(`El campo ${key} es obligatorio.`);
    }

    return value.trim();
  }

  private getOptionalString(item: UiFormData, key: string): string | null {
    const value = item[key];

    if (typeof value !== 'string' || value.trim() === '') {
      return null;
    }

    return value.trim();
  }

  private getPublicId(item: UiIdentity): string {
    const value = item.publicId;

    if (typeof value !== 'string' || value.trim() === '') {
      throw new Error('El campo publicId es obligatorio.');
    }

    return value.trim();
  }

  private getCount(item: UiFormData, key: string): number {
    const value = item[key];

    return typeof value === 'number' && Number.isInteger(value) && value >= 0
      ? value
      : 0;
  }
}
