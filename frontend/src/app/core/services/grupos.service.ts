import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { forkJoin, map, Observable, of, switchMap } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import { CarreraResponseDto } from '../dto/carreras/carrera-response.dto';
import { GrupoRequestDto } from '../dto/grupos/grupo-request.dto';
import { GrupoResponseDto } from '../dto/grupos/grupo-response.dto';
import { PeriodoResponseDto } from '../dto/periodos/periodo-response.dto';
import { EntityStatus, Grupo } from '../models/admin-catalogos.model';

interface CatalogOption {
  label: string;
  value: string;
}

type UiFormData = Record<string, unknown>;
type UiIdentity = { publicId?: unknown };

@Injectable({
  providedIn: 'root'
})
export class GruposService {
  readonly carreraOptions: CatalogOption[] = [];
  readonly periodoOptions: CatalogOption[] = [];

  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiUrl}/api/Grupos`;
  private readonly carrerasEndpoint = `${environment.apiUrl}/api/Carreras`;
  private readonly periodosEndpoint = `${environment.apiUrl}/api/Periodos`;
  private readonly carreraNames = new Map<string, string>();
  private readonly periodoNames = new Map<string, string>();

  load(): Observable<Grupo[]> {
    return forkJoin({
      grupos: this.http
        .get<ApiResponseDto<GrupoResponseDto[]>>(this.endpoint)
        .pipe(map(response => this.unwrap(response))),
      carreras: this.http
        .get<ApiResponseDto<CarreraResponseDto[]>>(this.carrerasEndpoint)
        .pipe(map(response => this.unwrap(response))),
      periodos: this.http
        .get<ApiResponseDto<PeriodoResponseDto[]>>(this.periodosEndpoint)
        .pipe(map(response => this.unwrap(response)))
    }).pipe(
      map(({ grupos, carreras, periodos }) => {
        this.setCarreras(carreras);
        this.setPeriodos(periodos);
        return grupos.map(grupo => this.toUiModel(grupo));
      })
    );
  }

  create(item: UiFormData): Observable<Grupo> {
    const requestedStatus = item['estado'];

    return this.http
      .post<ApiResponseDto<GrupoResponseDto>>(
        this.endpoint,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        switchMap(grupo => {
          const createdItem = this.toUiModel(grupo);

          if (requestedStatus !== 'inactivo') {
            return of(createdItem);
          }

          return this.delete(createdItem).pipe(
            map((): Grupo => ({ ...createdItem, estado: 'inactivo' }))
          );
        })
      );
  }

  update(item: UiFormData): Observable<Grupo> {
    if (item['estado'] === 'inactivo') {
      return this.delete(item).pipe(
        map(() => this.toUiModelFromForm(item, 'inactivo'))
      );
    }

    const publicId = this.getRequiredString(item, 'publicId');

    return this.http
      .put<ApiResponseDto<GrupoResponseDto>>(
        `${this.endpoint}/${publicId}`,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        map(grupo => this.toUiModel(grupo))
      );
  }

  delete(item: UiIdentity): Observable<boolean> {
    const publicId = this.getPublicId(item);

    return this.http
      .delete<ApiResponseDto<boolean>>(`${this.endpoint}/${publicId}`)
      .pipe(map(response => this.unwrap(response)));
  }

  private setCarreras(carreras: CarreraResponseDto[]): void {
    this.carreraNames.clear();

    for (const carrera of carreras) {
      this.carreraNames.set(carrera.publicId, carrera.nombre);
    }

    this.carreraOptions.splice(
      0,
      this.carreraOptions.length,
      ...carreras.map(carrera => ({
        label: carrera.nombre,
        value: carrera.publicId
      }))
    );
  }

  private setPeriodos(periodos: PeriodoResponseDto[]): void {
    this.periodoNames.clear();

    for (const periodo of periodos) {
      this.periodoNames.set(periodo.publicId, periodo.nombre);
    }

    this.periodoOptions.splice(
      0,
      this.periodoOptions.length,
      ...periodos.map(periodo => ({
        label: periodo.nombre,
        value: periodo.publicId
      }))
    );
  }

  private toRequestDto(item: UiFormData): GrupoRequestDto {
    return {
      carreraPublicId: this.getRequiredString(item, 'carreraPublicId'),
      periodoPublicId: this.getRequiredString(item, 'periodoPublicId'),
      nombre: this.getRequiredString(item, 'nombre'),
      cuatrimestre: this.getCuatrimestre(item)
    };
  }

  private toUiModel(grupo: GrupoResponseDto): Grupo {
    return {
      id: grupo.publicId,
      publicId: grupo.publicId,
      carreraPublicId: grupo.carreraPublicId,
      carreraNombre: this.carreraNames.get(grupo.carreraPublicId) ?? '',
      periodoPublicId: grupo.periodoPublicId,
      periodoNombre: this.periodoNames.get(grupo.periodoPublicId) ?? '',
      nombre: grupo.nombre,
      cuatrimestre: grupo.cuatrimestre,
      estado: grupo.activo ? 'activo' : 'inactivo'
    };
  }

  private toUiModelFromForm(
    item: UiFormData,
    estado: EntityStatus
  ): Grupo {
    const publicId = this.getRequiredString(item, 'publicId');
    const carreraPublicId = this.getRequiredString(item, 'carreraPublicId');
    const periodoPublicId = this.getRequiredString(item, 'periodoPublicId');

    return {
      id: publicId,
      publicId,
      carreraPublicId,
      carreraNombre: this.carreraNames.get(carreraPublicId) ?? '',
      periodoPublicId,
      periodoNombre: this.periodoNames.get(periodoPublicId) ?? '',
      nombre: this.getRequiredString(item, 'nombre'),
      cuatrimestre: this.getCuatrimestre(item),
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

  private getCuatrimestre(item: UiFormData): number {
    const value = item['cuatrimestre'];
    const numberValue =
      typeof value === 'number' || typeof value === 'string'
        ? Number(value)
        : Number.NaN;

    if (
      !Number.isInteger(numberValue) ||
      numberValue < 1 ||
      numberValue > 2147483647
    ) {
      throw new Error(
        'El campo cuatrimestre debe ser un entero entre 1 y 2147483647.'
      );
    }

    return numberValue;
  }

  private getPublicId(item: UiIdentity): string {
    const value = item.publicId;

    if (typeof value !== 'string' || value.trim() === '') {
      throw new Error('El campo publicId es obligatorio.');
    }

    return value.trim();
  }
}
