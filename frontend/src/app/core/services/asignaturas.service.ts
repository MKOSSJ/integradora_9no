import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { forkJoin, map, Observable, of, switchMap } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import { AsignaturaRequestDto } from '../dto/asignaturas/asignatura-request.dto';
import {
  AcademiaResponseDto,
  AsignaturaResponseDto
} from '../dto/asignaturas/asignatura-response.dto';
import { Asignatura, EntityStatus } from '../models/admin-catalogos.model';

interface CatalogOption {
  label: string;
  value: string;
}

type UiFormData = Record<string, unknown>;
type UiIdentity = { publicId?: unknown };

@Injectable({
  providedIn: 'root'
})
export class AsignaturasService {
  readonly academiaOptions: CatalogOption[] = [];

  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiUrl}/api/Asignaturas`;
  private readonly academiasEndpoint = `${environment.apiUrl}/api/Academias`;
  private readonly academiaNames = new Map<string, string>();

  load(): Observable<Asignatura[]> {
    return forkJoin({
      asignaturas: this.http
        .get<ApiResponseDto<AsignaturaResponseDto[]>>(this.endpoint)
        .pipe(map(response => this.unwrap(response))),
      academias: this.http
        .get<ApiResponseDto<AcademiaResponseDto[]>>(this.academiasEndpoint)
        .pipe(map(response => this.unwrap(response)))
    }).pipe(
      map(({ asignaturas, academias }) => {
        this.setAcademias(academias);
        return asignaturas.map(asignatura => this.toUiModel(asignatura));
      })
    );
  }

  create(item: UiFormData): Observable<Asignatura> {
    const requestedStatus = item['estado'];

    return this.http
      .post<ApiResponseDto<AsignaturaResponseDto>>(
        this.endpoint,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        switchMap(asignatura => {
          const createdItem = this.toUiModel(asignatura);

          if (requestedStatus !== 'inactivo') {
            return of(createdItem);
          }

          return this.delete(createdItem).pipe(
            map((): Asignatura => ({ ...createdItem, estado: 'inactivo' }))
          );
        })
      );
  }

  update(item: UiFormData): Observable<Asignatura> {
    if (item['estado'] === 'inactivo') {
      return this.delete(item).pipe(
        map(() => this.toUiModelFromForm(item, 'inactivo'))
      );
    }

    const publicId = this.getRequiredString(item, 'publicId');

    return this.http
      .put<ApiResponseDto<AsignaturaResponseDto>>(
        `${this.endpoint}/${publicId}`,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        map(asignatura => this.toUiModel(asignatura))
      );
  }

  delete(item: UiIdentity): Observable<boolean> {
    const publicId = this.getPublicId(item);

    return this.http
      .delete<ApiResponseDto<boolean>>(`${this.endpoint}/${publicId}`)
      .pipe(map(response => this.unwrap(response)));
  }

  private setAcademias(academias: AcademiaResponseDto[]): void {
    this.academiaNames.clear();

    for (const academia of academias) {
      this.academiaNames.set(academia.publicId, academia.nombre);
    }

    this.academiaOptions.splice(
      0,
      this.academiaOptions.length,
      ...academias.map(academia => ({
        label: academia.nombre,
        value: academia.publicId
      }))
    );
  }

  private toRequestDto(item: UiFormData): AsignaturaRequestDto {
    return {
      nombre: this.getRequiredString(item, 'nombre'),
      clave: this.getRequiredString(item, 'clave'),
      cuatrimestre: this.getPositiveInteger(item, 'cuatrimestre'),
      horasTotales: this.getPositiveInteger(item, 'horasTotales'),
      horasSemana: this.getPositiveInteger(item, 'horasSemana'),
      creditos: this.getCredits(item),
      academiaPublicId: this.getOptionalString(item, 'academiaPublicId')
    };
  }

  private toUiModel(asignatura: AsignaturaResponseDto): Asignatura {
    return {
      id: asignatura.publicId,
      publicId: asignatura.publicId,
      academiaPublicId: asignatura.academiaPublicId ?? '',
      academiaNombre: asignatura.academiaPublicId
        ? this.academiaNames.get(asignatura.academiaPublicId) ?? ''
        : '',
      nombre: asignatura.nombre,
      clave: asignatura.clave,
      cuatrimestre: asignatura.cuatrimestre,
      horasTotales: asignatura.horasTotales,
      horasSemana: asignatura.horasSemana,
      creditos: asignatura.creditos,
      estado: asignatura.activo ? 'activo' : 'inactivo'
    };
  }

  private toUiModelFromForm(
    item: UiFormData,
    estado: EntityStatus
  ): Asignatura {
    const publicId = this.getRequiredString(item, 'publicId');
    const academiaPublicId = this.getOptionalString(item, 'academiaPublicId');

    return {
      id: publicId,
      publicId,
      academiaPublicId: academiaPublicId ?? '',
      academiaNombre: academiaPublicId
        ? this.academiaNames.get(academiaPublicId) ?? ''
        : '',
      nombre: this.getRequiredString(item, 'nombre'),
      clave: this.getRequiredString(item, 'clave'),
      cuatrimestre: this.getPositiveInteger(item, 'cuatrimestre'),
      horasTotales: this.getPositiveInteger(item, 'horasTotales'),
      horasSemana: this.getPositiveInteger(item, 'horasSemana'),
      creditos: this.getCredits(item),
      estado
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

  private getPublicId(item: UiIdentity): string {
    const value = item.publicId;

    if (typeof value !== 'string' || value.trim() === '') {
      throw new Error('El campo publicId es obligatorio.');
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

  private getPositiveInteger(
    item: UiFormData,
    key: string
  ): number {
    const value = item[key];
    const numberValue =
      typeof value === 'number' || typeof value === 'string'
        ? Number(value)
        : Number.NaN;

    if (!Number.isInteger(numberValue) || numberValue < 1) {
      throw new Error(`El campo ${key} debe ser un entero mayor que cero.`);
    }

    return numberValue;
  }

  private getCredits(item: UiFormData): number {
    const value = item['creditos'];
    const credits =
      typeof value === 'number' || typeof value === 'string'
        ? Number(value)
        : Number.NaN;

    if (!Number.isFinite(credits) || credits < 0 || credits > 999.99) {
      throw new Error('El campo creditos debe estar entre 0 y 999.99.');
    }

    return credits;
  }
}
