import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { forkJoin, map, Observable } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import { CicloEscolarResponseDto } from '../dto/ciclos/ciclo-escolar-response.dto';
import { PeriodoRequestDto } from '../dto/periodos/periodo-request.dto';
import { PeriodoResponseDto } from '../dto/periodos/periodo-response.dto';
import { Periodo } from '../models/admin-catalogos.model';

interface CatalogOption {
  label: string;
  value: string;
}

type UiFormData = Record<string, unknown>;
type UiIdentity = { publicId?: unknown };

@Injectable({
  providedIn: 'root'
})
export class PeriodosService {
  readonly cicloOptions: CatalogOption[] = [];

  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiUrl}/api/Periodos`;
  private readonly ciclosEndpoint = `${environment.apiUrl}/api/CiclosEscolares`;
  private readonly cicloNames = new Map<string, string>();

  load(): Observable<Periodo[]> {
    return forkJoin({
      periodos: this.http
        .get<ApiResponseDto<PeriodoResponseDto[]>>(this.endpoint)
        .pipe(map(response => this.unwrap(response))),
      ciclos: this.http
        .get<ApiResponseDto<CicloEscolarResponseDto[]>>(this.ciclosEndpoint)
        .pipe(map(response => this.unwrap(response)))
    }).pipe(
      map(({ periodos, ciclos }) => {
        this.setCiclos(ciclos);
        return periodos.map(periodo => this.toUiModel(periodo));
      })
    );
  }

  create(item: UiFormData): Observable<Periodo> {
    return this.http
      .post<ApiResponseDto<PeriodoResponseDto>>(
        this.endpoint,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        map(periodo => this.toUiModel(periodo))
      );
  }

  update(item: UiFormData): Observable<Periodo> {
    const publicId = this.getPublicId(item);

    return this.http
      .put<ApiResponseDto<PeriodoResponseDto>>(
        `${this.endpoint}/${publicId}`,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        map(periodo => this.toUiModel(periodo))
      );
  }

  delete(item: UiIdentity): Observable<boolean> {
    const publicId = this.getPublicId(item);

    return this.http
      .delete<ApiResponseDto<boolean>>(`${this.endpoint}/${publicId}`)
      .pipe(map(response => this.unwrap(response)));
  }

  private setCiclos(ciclos: CicloEscolarResponseDto[]): void {
    this.cicloNames.clear();

    for (const ciclo of ciclos) {
      this.cicloNames.set(ciclo.publicId, ciclo.nombre);
    }

    this.cicloOptions.splice(
      0,
      this.cicloOptions.length,
      ...ciclos.map(ciclo => ({
        label: ciclo.nombre,
        value: ciclo.publicId
      }))
    );
  }

  private toRequestDto(item: UiFormData): PeriodoRequestDto {
    const fechaInicio = this.getRequiredDate(item, 'fechaInicio');
    const fechaFin = this.getRequiredDate(item, 'fechaFin');

    if (fechaFin <= fechaInicio) {
      throw new Error('La fecha de fin debe ser posterior a la fecha de inicio.');
    }

    return {
      cicloEscolarPublicId: this.getRequiredString(
        item,
        'cicloEscolarPublicId'
      ),
      nombre: this.getRequiredString(item, 'nombre'),
      fechaInicio,
      fechaFin
    };
  }

  private toUiModel(periodo: PeriodoResponseDto): Periodo {
    return {
      id: periodo.publicId,
      publicId: periodo.publicId,
      cicloEscolarPublicId: periodo.cicloEscolarPublicId,
      cicloEscolarNombre:
        this.cicloNames.get(periodo.cicloEscolarPublicId) ?? '',
      nombre: periodo.nombre,
      fechaInicio: this.toDateInputValue(periodo.fechaInicio),
      fechaFin: this.toDateInputValue(periodo.fechaFin),
      estado: periodo.activo ? 'activo' : 'inactivo'
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
      throw new Error('La API devolvió una fecha de periodo inválida.');
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
