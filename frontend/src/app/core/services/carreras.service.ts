import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable, of, switchMap } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import { CarreraRequestDto } from '../dto/carreras/carrera-request.dto';
import { CarreraResponseDto } from '../dto/carreras/carrera-response.dto';
import { Carrera, EntityStatus } from '../models/admin-catalogos.model';

type UiFormData = Record<string, unknown>;
type UiIdentity = { publicId?: unknown };

@Injectable({
  providedIn: 'root'
})
export class CarrerasService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiUrl}/api/Carreras`;

  load(): Observable<Carrera[]> {
    return this.http
      .get<ApiResponseDto<CarreraResponseDto[]>>(this.endpoint)
      .pipe(
        map(response => this.unwrap(response)),
        map(carreras => carreras.map(carrera => this.toUiModel(carrera)))
      );
  }

  create(item: UiFormData): Observable<Carrera> {
    const requestedStatus = item['estado'];

    return this.http
      .post<ApiResponseDto<CarreraResponseDto>>(
        this.endpoint,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        switchMap(carrera => {
          const createdItem = this.toUiModel(carrera);

          if (requestedStatus !== 'inactivo') {
            return of(createdItem);
          }

          return this.delete(createdItem).pipe(
            map((): Carrera => ({ ...createdItem, estado: 'inactivo' }))
          );
        })
      );
  }

  update(item: UiFormData): Observable<Carrera> {
    if (item['estado'] === 'inactivo') {
      return this.delete(item).pipe(
        map(() => this.toUiModelFromForm(item, 'inactivo'))
      );
    }

    const publicId = this.getRequiredString(item, 'publicId');

    return this.http
      .put<ApiResponseDto<CarreraResponseDto>>(
        `${this.endpoint}/${publicId}`,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        map(carrera => this.toUiModel(carrera))
      );
  }

  delete(item: UiIdentity): Observable<boolean> {
    const publicId = this.getPublicId(item);

    return this.http
      .delete<ApiResponseDto<boolean>>(`${this.endpoint}/${publicId}`)
      .pipe(map(response => this.unwrap(response)));
  }

  private toRequestDto(item: UiFormData): CarreraRequestDto {
    return {
      nombre: this.getRequiredString(item, 'nombre'),
      clave: this.getRequiredString(item, 'clave'),
      nivel: this.getOptionalString(item, 'nivel')
    };
  }

  private toUiModel(carrera: CarreraResponseDto): Carrera {
    return {
      id: carrera.publicId,
      publicId: carrera.publicId,
      nombre: carrera.nombre,
      clave: carrera.clave,
      nivel: carrera.nivel ?? '',
      estado: carrera.activo ? 'activo' : 'inactivo'
    };
  }

  private toUiModelFromForm(
    item: UiFormData,
    estado: EntityStatus
  ): Carrera {
    const publicId = this.getRequiredString(item, 'publicId');

    return {
      id: publicId,
      publicId,
      nombre: this.getRequiredString(item, 'nombre'),
      clave: this.getRequiredString(item, 'clave'),
      nivel: this.getOptionalString(item, 'nivel') ?? '',
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
}
