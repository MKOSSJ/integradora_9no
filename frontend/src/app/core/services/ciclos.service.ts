import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import { CicloEscolarRequestDto } from '../dto/ciclos/ciclo-escolar-request.dto';
import { CicloEscolarResponseDto } from '../dto/ciclos/ciclo-escolar-response.dto';
import { CicloEscolar } from '../models/admin-catalogos.model';

type UiFormData = Record<string, unknown>;
type UiIdentity = { publicId?: unknown };

@Injectable({
  providedIn: 'root'
})
export class CiclosService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiUrl}/api/CiclosEscolares`;

  load(): Observable<CicloEscolar[]> {
    return this.http
      .get<ApiResponseDto<CicloEscolarResponseDto[]>>(this.endpoint)
      .pipe(
        map(response => this.unwrap(response)),
        map(ciclos => ciclos.map(ciclo => this.toUiModel(ciclo)))
      );
  }

  create(item: UiFormData): Observable<CicloEscolar> {
    return this.http
      .post<ApiResponseDto<CicloEscolarResponseDto>>(
        this.endpoint,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        map(ciclo => this.toUiModel(ciclo))
      );
  }

  update(item: UiFormData): Observable<CicloEscolar> {
    const publicId = this.getPublicId(item);

    return this.http
      .put<ApiResponseDto<CicloEscolarResponseDto>>(
        `${this.endpoint}/${publicId}`,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        map(ciclo => this.toUiModel(ciclo))
      );
  }

  delete(item: UiIdentity): Observable<boolean> {
    const publicId = this.getPublicId(item);

    return this.http
      .delete<ApiResponseDto<boolean>>(`${this.endpoint}/${publicId}`)
      .pipe(map(response => this.unwrap(response)));
  }

  private toRequestDto(item: UiFormData): CicloEscolarRequestDto {
    const fechaInicio = this.getRequiredDate(item, 'fechaInicio');
    const fechaFin = this.getRequiredDate(item, 'fechaFin');

    if (fechaFin <= fechaInicio) {
      throw new Error('La fecha de fin debe ser posterior a la fecha de inicio.');
    }

    return {
      nombre: this.getRequiredString(item, 'nombre'),
      fechaInicio,
      fechaFin
    };
  }

  private toUiModel(ciclo: CicloEscolarResponseDto): CicloEscolar {
    return {
      id: ciclo.publicId,
      publicId: ciclo.publicId,
      nombre: ciclo.nombre,
      fechaInicio: this.toDateInputValue(ciclo.fechaInicio),
      fechaFin: this.toDateInputValue(ciclo.fechaFin),
      estado: ciclo.activo ? 'activo' : 'inactivo'
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

  private getRequiredDate(item: UiFormData, key: string): string {
    const value = this.getRequiredString(item, key);

    if (!this.isValidDateInput(value)) {
      throw new Error(`El campo ${key} debe contener una fecha válida.`);
    }

    return value;
  }

  private getPublicId(item: UiIdentity): string {
    const value = item.publicId;

    if (typeof value !== 'string' || value.trim() === '') {
      throw new Error('El campo publicId es obligatorio.');
    }

    return value.trim();
  }

  private toDateInputValue(value: string): string {
    const datePart = value.slice(0, 10);

    if (!this.isValidDateInput(datePart)) {
      throw new Error('La API devolvió una fecha de ciclo escolar inválida.');
    }

    return datePart;
  }

  private isValidDateInput(value: string): boolean {
    const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);

    if (!match) return false;

    const year = Number(match[1]);
    const month = Number(match[2]);
    const day = Number(match[3]);
    const date = new Date(Date.UTC(year, month - 1, day));

    return date.getUTCFullYear() === year &&
      date.getUTCMonth() === month - 1 &&
      date.getUTCDate() === day;
  }
}
