import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { defer, forkJoin, map, Observable } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import { CargaAcademicaRequestDto } from '../dto/cargas-academicas/carga-academica-request.dto';
import { CargaAcademicaResponseDto } from '../dto/cargas-academicas/carga-academica-response.dto';
import {
  Academia,
  Asignatura,
  CargaAcademica,
  EntityStatus,
  Grupo,
  Periodo,
  SystemRole,
  UsuarioBackendListItem
} from '../models/admin-catalogos.model';
import { AcademiasService } from './academias.service';
import { AsignaturasService } from './asignaturas.service';
import { GruposService } from './grupos.service';
import { PeriodosService } from './periodos.service';
import { UsuariosService } from './usuarios.service';

type UiFormData = Record<string, unknown>;
type CatalogOption = { label: string; value: string };

interface Catalogs {
  periodos: Periodo[];
  grupos: Grupo[];
  asignaturas: Asignatura[];
  academias: Academia[];
  usuarios: UsuarioBackendListItem[];
}

/** CRUD real de CargasAcademicas. No lee ni migra asignaciones locales. */
@Injectable({ providedIn: 'root' })
export class AsignacionesAcademicasService {
  private readonly http = inject(HttpClient);
  private readonly periodosService = inject(PeriodosService);
  private readonly gruposService = inject(GruposService);
  private readonly asignaturasService = inject(AsignaturasService);
  private readonly academiasService = inject(AcademiasService);
  private readonly usuariosService = inject(UsuariosService);
  private readonly endpoint = `${environment.apiUrl}/api/CargasAcademicas`;

  private catalogs: Catalogs = {
    periodos: [],
    grupos: [],
    asignaturas: [],
    academias: [],
    usuarios: []
  };

  readonly periodoOptions: CatalogOption[] = [];
  readonly grupoOptions: CatalogOption[] = [];
  readonly asignaturaOptions: CatalogOption[] = [];
  readonly docenteOptions: CatalogOption[] = [];
  readonly revisorOptions: CatalogOption[] = [];
  readonly academiaOptions: CatalogOption[] = [];

  load(): Observable<CargaAcademica[]> {
    return defer(() => forkJoin({
      periodos: this.periodosService.load(),
      grupos: this.gruposService.load(),
      asignaturas: this.asignaturasService.load(),
      academias: this.academiasService.load(),
      usuarios: this.usuariosService.load(),
      cargas: this.http
        .get<ApiResponseDto<CargaAcademicaResponseDto[]>>(this.endpoint)
        .pipe(map(response => this.unwrap(response)))
    }).pipe(
      map(({ cargas, ...catalogs }) => {
        this.catalogs = catalogs;
        this.updateOptions();
        return cargas.map(carga => this.toUiModel(carga));
      })
    ));
  }

  create(item: UiFormData): Observable<CargaAcademica> {
    return defer(() => this.http
      .post<ApiResponseDto<CargaAcademicaResponseDto>>(
        this.endpoint,
        this.toRequestDto(item)
      )
      .pipe(
        map(response => this.unwrap(response)),
        map(carga => this.toUiModel(carga))
      ));
  }

  update(item: UiFormData): Observable<CargaAcademica> {
    return defer(() => {
      const publicId = this.requiredString(item, 'publicId');

      if (item['source'] !== 'backend') {
        throw new Error('Una carga local no puede enviarse al backend.');
      }

      return this.http
        .put<ApiResponseDto<CargaAcademicaResponseDto>>(
          `${this.endpoint}/${publicId}`,
          this.toRequestDto(item)
        )
        .pipe(
          map(response => this.unwrap(response)),
          map(carga => this.toUiModel(carga))
        );
    });
  }

  delete(item: UiFormData): Observable<boolean> {
    return defer(() => {
      const publicId = this.requiredString(item, 'publicId');

      if (item['source'] !== 'backend') {
        throw new Error('Una carga local no puede enviarse al backend.');
      }

      return this.http
        .delete<ApiResponseDto<boolean>>(`${this.endpoint}/${publicId}`)
        .pipe(map(response => this.unwrap(response)));
    });
  }

  private updateOptions(): void {
    this.replaceCatalogOptions(this.periodoOptions, this.catalogs.periodos);
    this.replaceCatalogOptions(this.grupoOptions, this.catalogs.grupos);
    this.replaceCatalogOptions(
      this.asignaturaOptions,
      this.catalogs.asignaturas
    );
    this.replaceCatalogOptions(this.academiaOptions, this.catalogs.academias);
    this.replaceUserOptions(this.docenteOptions, 'DOCENTE');
    this.replaceUserOptions(this.revisorOptions, 'REVISOR');
  }

  private replaceCatalogOptions(
    target: CatalogOption[],
    items: Array<{ publicId: string; nombre: string; estado: EntityStatus }>
  ): void {
    target.splice(
      0,
      target.length,
      ...items
        .filter(item => item.estado === 'activo')
        .map(item => ({ label: item.nombre, value: item.publicId }))
    );
  }

  private replaceUserOptions(
    target: CatalogOption[],
    role: SystemRole
  ): void {
    target.splice(
      0,
      target.length,
      ...this.catalogs.usuarios
        .filter(user => user.roles.includes(role))
        .map(user => ({
          label: this.fullName(user),
          value: user.publicId
        }))
    );
  }

  private toRequestDto(item: UiFormData): CargaAcademicaRequestDto {
    const periodo = this.requireActiveCatalog(
      this.catalogs.periodos,
      this.requiredString(item, 'periodoPublicId'),
      'periodo'
    );
    const grupo = this.requireActiveCatalog(
      this.catalogs.grupos,
      this.requiredString(item, 'grupoPublicId'),
      'grupo'
    );
    const asignatura = this.requireActiveCatalog(
      this.catalogs.asignaturas,
      this.requiredString(item, 'asignaturaPublicId'),
      'asignatura'
    );
    const docente = this.requireUserWithRole(
      this.requiredString(item, 'docentePublicId'),
      'DOCENTE',
      'docente'
    );
    const revisorPublicId = this.optionalString(item, 'revisorPublicId');
    const academiaPublicId = this.optionalString(item, 'academiaPublicId');
    const revisor = revisorPublicId
      ? this.requireUserWithRole(revisorPublicId, 'REVISOR', 'revisor')
      : undefined;
    const academia = academiaPublicId
      ? this.requireActiveCatalog(
          this.catalogs.academias,
          academiaPublicId,
          'academia'
        )
      : undefined;

    return {
      periodoPublicId: periodo.publicId,
      grupoPublicId: grupo.publicId,
      asignaturaPublicId: asignatura.publicId,
      docentePublicId: docente.publicId,
      revisorPublicId: revisor?.publicId ?? null,
      academiaPublicId: academia?.publicId ?? null
    };
  }

  private toUiModel(item: CargaAcademicaResponseDto): CargaAcademica {
    const periodo = this.requireCatalog(
      this.catalogs.periodos,
      item.periodoPublicId,
      'periodo'
    );
    const grupo = this.requireCatalog(
      this.catalogs.grupos,
      item.grupoPublicId,
      'grupo'
    );
    const asignatura = this.requireCatalog(
      this.catalogs.asignaturas,
      item.asignaturaPublicId,
      'asignatura'
    );
    const docente = this.requireUser(item.docentePublicId, 'docente');
    const revisor = item.revisorPublicId
      ? this.requireUser(item.revisorPublicId, 'revisor')
      : undefined;
    const academia = item.academiaPublicId
      ? this.requireCatalog(
          this.catalogs.academias,
          item.academiaPublicId,
          'academia'
        )
      : undefined;

    return {
      source: 'backend',
      id: item.publicId,
      publicId: item.publicId,
      periodoPublicId: item.periodoPublicId,
      periodoNombre: periodo.nombre,
      grupoPublicId: item.grupoPublicId,
      grupoNombre: grupo.nombre,
      asignaturaPublicId: item.asignaturaPublicId,
      asignaturaNombre: asignatura.nombre,
      docentePublicId: item.docentePublicId,
      docenteNombre: this.fullName(docente),
      docenteRoles: [...docente.roles],
      revisorPublicId: item.revisorPublicId ?? '',
      revisorNombre: revisor ? this.fullName(revisor) : '',
      revisorRoles: revisor ? [...revisor.roles] : [],
      academiaPublicId: item.academiaPublicId ?? '',
      academiaNombre: academia?.nombre ?? '',
      estado: item.activo ? 'activo' : 'inactivo'
    };
  }

  private requireActiveCatalog<T extends {
    publicId: string;
    nombre: string;
    estado: EntityStatus;
  }>(items: T[], publicId: string, label: string): T {
    const item = this.requireCatalog(items, publicId, label);

    if (item.estado !== 'activo') {
      throw new Error(`El ${label} seleccionado está inactivo.`);
    }

    return item;
  }

  private requireCatalog<T extends { publicId: string; nombre: string }>(
    items: T[],
    publicId: string,
    label: string
  ): T {
    const item = items.find(candidate => candidate.publicId === publicId);

    if (!item) {
      throw new Error(`No fue posible resolver el ${label} de la carga.`);
    }

    return item;
  }

  private requireUserWithRole(
    publicId: string,
    role: SystemRole,
    label: string
  ): UsuarioBackendListItem {
    const user = this.requireUser(publicId, label);

    if (!user.roles.includes(role)) {
      throw new Error(`El ${label} seleccionado no tiene el rol requerido.`);
    }

    return user;
  }

  private requireUser(
    publicId: string,
    label: string
  ): UsuarioBackendListItem {
    const user = this.catalogs.usuarios.find(candidate =>
      candidate.publicId === publicId && candidate.source === 'backend'
    );

    if (!user) {
      throw new Error(`No fue posible resolver el ${label} de la carga.`);
    }

    return user;
  }

  private fullName(user: UsuarioBackendListItem): string {
    return [user.nombre, user.apellidoPaterno, user.apellidoMaterno]
      .filter(Boolean)
      .join(' ');
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

  private requiredString(item: UiFormData, key: string): string {
    const value = item[key];

    if (typeof value !== 'string' || value.trim() === '') {
      throw new Error(`El campo ${key} es obligatorio.`);
    }

    return value.trim();
  }

  private optionalString(item: UiFormData, key: string): string {
    const value = item[key];
    return typeof value === 'string' ? value.trim() : '';
  }
}
