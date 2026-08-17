import { inject, Injectable } from '@angular/core';
import { forkJoin, map, Observable, of } from 'rxjs';

import {
  Academia,
  Asignatura,
  CargaAcademica,
  EntityStatus,
  Grupo,
  Periodo,
  UsuarioAdmin
} from '../models/admin-catalogos.model';
import { CARGA_ACADEMICA } from '../../features/admin/shared/admin-data';
import { AcademiasService } from './academias.service';
import { AsignaturasService } from './asignaturas.service';
import { GruposService } from './grupos.service';
import { PeriodosService } from './periodos.service';
import { UsuariosLocalService } from './usuarios-local.service';

type UiFormData = Record<string, unknown>;
type CatalogOption = { label: string; value: string };

interface Catalogs {
  periodos: Periodo[];
  grupos: Grupo[];
  asignaturas: Asignatura[];
  academias: Academia[];
  usuarios: UsuarioAdmin[];
}

/**
 * Persistencia frontend temporal de cargas académicas. Las referencias de
 * docente y revisor son publicId locales y nunca se envían a CargasAcademicas.
 */
@Injectable({ providedIn: 'root' })
export class AsignacionesAcademicasLocalService {
  static readonly storageKey = 'plandi_local_cargas_academicas';

  private readonly periodosService = inject(PeriodosService);
  private readonly gruposService = inject(GruposService);
  private readonly asignaturasService = inject(AsignaturasService);
  private readonly academiasService = inject(AcademiasService);
  private readonly usuariosLocalService = inject(UsuariosLocalService);

  private catalogs: Catalogs = {
    periodos: [], grupos: [], asignaturas: [], academias: [], usuarios: []
  };

  readonly periodoOptions: CatalogOption[] = [];
  readonly grupoOptions: CatalogOption[] = [];
  readonly asignaturaOptions: CatalogOption[] = [];
  readonly docenteOptions: CatalogOption[] = [];
  readonly revisorOptions: CatalogOption[] = [];
  readonly academiaOptions: CatalogOption[] = [];

  load(): Observable<CargaAcademica[]> {
    return forkJoin({
      periodos: this.periodosService.load(),
      grupos: this.gruposService.load(),
      asignaturas: this.asignaturasService.load(),
      academias: this.academiasService.load(),
      usuarios: this.usuariosLocalService.load()
    }).pipe(
      map(catalogs => {
        this.catalogs = catalogs;
        this.updateOptions();

        const stored = localStorage.getItem(
          AsignacionesAcademicasLocalService.storageKey
        );
        const assignments = stored === null
          ? this.createSeed(catalogs)
          : this.parseStored(stored);
        const resolved = assignments.map(item => this.resolveNames(item));
        this.write(resolved);
        return this.clone(resolved);
      })
    );
  }

  create(item: UiFormData): Observable<CargaAcademica> {
    const assignments = this.read();
    const publicId = globalThis.crypto.randomUUID();
    const assignment = this.toAssignment(item, publicId);
    this.ensureNotDuplicate(assignment, assignments);
    this.write([assignment, ...assignments]);
    return of({ ...assignment });
  }

  update(item: UiFormData): Observable<CargaAcademica> {
    const publicId = this.requiredString(item, 'publicId');
    const assignments = this.read();

    if (!assignments.some(assignment => assignment.publicId === publicId)) {
      throw new Error('La asignación académica que intentas editar ya no existe.');
    }

    const assignment = this.toAssignment(item, publicId);
    this.ensureNotDuplicate(assignment, assignments, publicId);
    this.write(assignments.map(current =>
      current.publicId === publicId ? assignment : current
    ));
    return of({ ...assignment });
  }

  delete(item: UiFormData): Observable<boolean> {
    const publicId = this.requiredString(item, 'publicId');
    const assignments = this.read();

    if (!assignments.some(assignment => assignment.publicId === publicId)) {
      throw new Error('La asignación académica que intentas dar de baja ya no existe.');
    }

    this.write(assignments.map(assignment =>
      assignment.publicId === publicId
        ? { ...assignment, estado: 'inactivo' }
        : assignment
    ));
    return of(true);
  }

  private updateOptions(): void {
    this.replaceOptions(this.periodoOptions, this.catalogs.periodos);
    this.replaceOptions(this.grupoOptions, this.catalogs.grupos);
    this.replaceOptions(this.asignaturaOptions, this.catalogs.asignaturas);
    this.replaceOptions(this.academiaOptions, this.catalogs.academias);

    this.replaceUserOptions(
      this.docenteOptions,
      this.catalogs.usuarios.filter(user => user.roles.includes('DOCENTE'))
    );
    this.replaceUserOptions(
      this.revisorOptions,
      this.catalogs.usuarios.filter(user => user.roles.includes('REVISOR'))
    );
  }

  private replaceOptions(
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
    users: UsuarioAdmin[]
  ): void {
    target.splice(
      0,
      target.length,
      ...users
        .filter(user => user.estado === 'activo')
        .map(user => ({ label: this.fullName(user), value: user.publicId }))
    );
  }

  private toAssignment(
    item: UiFormData,
    publicId: string
  ): CargaAcademica {
    const periodoPublicId = this.requiredString(item, 'periodoPublicId');
    const grupoPublicId = this.requiredString(item, 'grupoPublicId');
    const asignaturaPublicId = this.requiredString(item, 'asignaturaPublicId');
    const docentePublicId = this.requiredString(item, 'docentePublicId');
    const revisorPublicId = this.optionalString(item, 'revisorPublicId');
    const academiaPublicId = this.optionalString(item, 'academiaPublicId');

    const periodo = this.requireActiveCatalog(
      this.catalogs.periodos, periodoPublicId, 'periodo'
    );
    const grupo = this.requireActiveCatalog(
      this.catalogs.grupos, grupoPublicId, 'grupo'
    );
    const asignatura = this.requireActiveCatalog(
      this.catalogs.asignaturas, asignaturaPublicId, 'asignatura'
    );
    const docente = this.requireActiveUser(docentePublicId, 'DOCENTE', 'docente');
    const revisor = revisorPublicId
      ? this.requireActiveUser(revisorPublicId, 'REVISOR', 'revisor')
      : undefined;
    const academia = academiaPublicId
      ? this.requireActiveCatalog(
          this.catalogs.academias, academiaPublicId, 'academia'
        )
      : undefined;

    return {
      source: 'local',
      id: publicId,
      publicId,
      periodoPublicId,
      periodoNombre: periodo.nombre,
      grupoPublicId,
      grupoNombre: grupo.nombre,
      asignaturaPublicId,
      asignaturaNombre: asignatura.nombre,
      docentePublicId,
      docenteNombre: this.fullName(docente),
      revisorPublicId,
      revisorNombre: revisor ? this.fullName(revisor) : '',
      academiaPublicId,
      academiaNombre: academia?.nombre ?? '',
      estado: item['estado'] === 'inactivo' ? 'inactivo' : 'activo'
    };
  }

  private ensureNotDuplicate(
    assignment: CargaAcademica,
    assignments: CargaAcademica[],
    ignoredPublicId?: string
  ): void {
    const duplicate = assignments.some(current =>
      current.publicId !== ignoredPublicId &&
      current.estado === 'activo' &&
      current.periodoPublicId === assignment.periodoPublicId &&
      current.grupoPublicId === assignment.grupoPublicId &&
      current.asignaturaPublicId === assignment.asignaturaPublicId &&
      current.docentePublicId === assignment.docentePublicId
    );

    if (duplicate) {
      throw new Error(
        'Ya existe una asignación activa con el mismo periodo, grupo, asignatura y docente.'
      );
    }
  }

  private requireActiveCatalog<T extends {
    publicId: string;
    nombre: string;
    estado: EntityStatus;
  }>(items: T[], publicId: string, label: string): T {
    const item = items.find(candidate => candidate.publicId === publicId);

    if (!item || item.estado !== 'activo') {
      throw new Error(`El ${label} seleccionado no existe o está inactivo.`);
    }

    return item;
  }

  private requireActiveUser(
    publicId: string,
    role: 'DOCENTE' | 'REVISOR',
    label: string
  ): UsuarioAdmin {
    const user = this.catalogs.usuarios.find(candidate =>
      candidate.publicId === publicId &&
      candidate.estado === 'activo' &&
      candidate.roles.includes(role)
    );

    if (!user) {
      throw new Error(`El ${label} seleccionado no existe, está inactivo o no tiene el rol requerido.`);
    }

    return user;
  }

  private resolveNames(item: CargaAcademica): CargaAcademica {
    const periodo = this.catalogs.periodos.find(
      candidate => candidate.publicId === item.periodoPublicId
    );
    const grupo = this.catalogs.grupos.find(
      candidate => candidate.publicId === item.grupoPublicId
    );
    const asignatura = this.catalogs.asignaturas.find(
      candidate => candidate.publicId === item.asignaturaPublicId
    );
    const docente = this.catalogs.usuarios.find(
      candidate => candidate.publicId === item.docentePublicId
    );
    const revisor = this.catalogs.usuarios.find(
      candidate => candidate.publicId === item.revisorPublicId
    );
    const academia = this.catalogs.academias.find(
      candidate => candidate.publicId === item.academiaPublicId
    );

    return {
      ...item,
      periodoNombre: periodo?.nombre ?? item.periodoNombre,
      grupoNombre: grupo?.nombre ?? item.grupoNombre,
      asignaturaNombre: asignatura?.nombre ?? item.asignaturaNombre,
      docenteNombre: docente ? this.fullName(docente) : item.docenteNombre,
      revisorNombre: revisor ? this.fullName(revisor) : item.revisorNombre,
      academiaNombre: academia?.nombre ?? item.academiaNombre
    };
  }

  private createSeed(catalogs: Catalogs): CargaAcademica[] {
    const seed: CargaAcademica[] = [];

    for (const item of CARGA_ACADEMICA) {
      const periodo = catalogs.periodos.find(value => value.nombre === item.periodoNombre);
      const grupo = catalogs.grupos.find(value => value.nombre === item.grupoNombre);
      const asignatura = catalogs.asignaturas.find(value => value.nombre === item.asignaturaNombre);
      const docente = catalogs.usuarios.find(value => this.fullName(value) === item.docenteNombre);

      if (!periodo || !grupo || !asignatura || !docente) continue;

      const revisor = catalogs.usuarios.find(value => this.fullName(value) === item.revisorNombre);
      const academia = catalogs.academias.find(value => value.nombre === item.academiaNombre);
      const publicId = String(item.publicId);

      seed.push({
        source: 'local',
        id: publicId,
        publicId,
        periodoPublicId: periodo.publicId,
        periodoNombre: periodo.nombre,
        grupoPublicId: grupo.publicId,
        grupoNombre: grupo.nombre,
        asignaturaPublicId: asignatura.publicId,
        asignaturaNombre: asignatura.nombre,
        docentePublicId: docente.publicId,
        docenteNombre: this.fullName(docente),
        revisorPublicId: revisor?.publicId ?? '',
        revisorNombre: revisor ? this.fullName(revisor) : '',
        academiaPublicId: academia?.publicId ?? '',
        academiaNombre: academia?.nombre ?? '',
        estado: item.estado === 'inactivo' ? 'inactivo' : 'activo'
      });
    }

    this.write(seed);
    return seed;
  }

  private parseStored(value: string): CargaAcademica[] {
    try {
      const parsed: unknown = JSON.parse(value);
      if (!Array.isArray(parsed)) return [];

      return parsed.flatMap(raw => {
        if (!raw || typeof raw !== 'object') return [];
        const item = raw as Record<string, unknown>;
        const publicId = String(item['publicId'] ?? item['id'] ?? '');
        if (!publicId) return [];

        return [{
          source: 'local',
          id: publicId,
          publicId,
          periodoPublicId: String(item['periodoPublicId'] ?? ''),
          periodoNombre: String(item['periodoNombre'] ?? ''),
          grupoPublicId: String(item['grupoPublicId'] ?? ''),
          grupoNombre: String(item['grupoNombre'] ?? ''),
          asignaturaPublicId: String(item['asignaturaPublicId'] ?? ''),
          asignaturaNombre: String(item['asignaturaNombre'] ?? ''),
          docentePublicId: String(item['docentePublicId'] ?? ''),
          docenteNombre: String(item['docenteNombre'] ?? ''),
          revisorPublicId: String(item['revisorPublicId'] ?? ''),
          revisorNombre: String(item['revisorNombre'] ?? ''),
          academiaPublicId: String(item['academiaPublicId'] ?? ''),
          academiaNombre: String(item['academiaNombre'] ?? ''),
          estado: item['estado'] === 'inactivo' ? 'inactivo' : 'activo'
        } satisfies CargaAcademica];
      });
    } catch {
      throw new Error('No fue posible leer las asignaciones guardadas localmente.');
    }
  }

  private read(): CargaAcademica[] {
    const stored = localStorage.getItem(
      AsignacionesAcademicasLocalService.storageKey
    );
    return stored === null ? [] : this.parseStored(stored);
  }

  private write(items: CargaAcademica[]): void {
    localStorage.setItem(
      AsignacionesAcademicasLocalService.storageKey,
      JSON.stringify(items)
    );
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

  private fullName(user: UsuarioAdmin): string {
    return [user.nombre, user.apellidoPaterno, user.apellidoMaterno]
      .filter(Boolean)
      .join(' ');
  }

  private clone(items: CargaAcademica[]): CargaAcademica[] {
    return items.map(item => ({ ...item }));
  }
}
