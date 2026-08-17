import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, catchError, map, Observable, of, tap } from 'rxjs';

import {
  Academia,
  EntityStatus,
  RolAcademia,
  SystemRole,
  UsuarioAdmin
} from '../models/admin-catalogos.model';
import { USUARIOS } from '../../features/admin/shared/admin-data';
import { AcademiasService } from './academias.service';

type UiFormData = Record<string, unknown>;

export interface UsuarioLocalImportInput {
  nombre: string;
  apellidoPaterno: string;
  apellidoMaterno?: string;
  email: string;
  telefono?: string;
  roles: SystemRole[];
  academiaPublicId?: string;
  academiaNombre?: string;
  rolEnAcademia?: RolAcademia | '';
}

/**
 * Persistencia frontend temporal. No envía usuarios al backend hasta que exista
 * un contrato administrativo con publicId y roles suficiente para este CRUD.
 */
@Injectable({ providedIn: 'root' })
export class UsuariosLocalService {
  static readonly storageKey = 'plandi_local_usuarios';

  private readonly academiasService = inject(AcademiasService);
  private readonly changesSubject = new BehaviorSubject<UsuarioAdmin[]>([]);
  private academias: Academia[] = [];

  readonly changes$ = this.changesSubject.asObservable();
  readonly academiaOptions: Array<{ label: string; value: string }> = [];

  load(): Observable<UsuarioAdmin[]> {
    return this.academiasService.load().pipe(
      catchError(() => of([] as Academia[])),
      map(academias => {
        this.setAcademias(academias);
        const users = this.readUsers().map(user => this.resolveAcademia(user));
        this.writeUsers(users);
        return this.cloneUsers(users);
      }),
      tap(users => this.changesSubject.next(this.cloneUsers(users)))
    );
  }

  create(item: UiFormData): Observable<UsuarioAdmin> {
    const users = this.readUsers();
    const user = this.toUser(item, this.newPublicId());
    this.ensureUniqueEmail(user.email, users);
    const updated = [user, ...users];
    this.persistAndNotify(updated);
    return of(this.cloneUser(user));
  }

  update(item: UiFormData): Observable<UsuarioAdmin> {
    const publicId = this.requiredString(item, 'publicId');
    const users = this.readUsers();
    const current = users.find(user => user.publicId === publicId);

    if (!current) throw new Error('El usuario que intentas editar ya no existe.');

    const user = {
      ...this.toUser(item, publicId),
      ultimoAcceso: current.ultimoAcceso
    };
    this.ensureUniqueEmail(user.email, users, publicId);
    const updated = users.map(existing =>
      existing.publicId === publicId ? user : existing
    );
    this.persistAndNotify(updated);
    return of(this.cloneUser(user));
  }

  delete(item: UiFormData): Observable<boolean> {
    const publicId = this.requiredString(item, 'publicId');
    const users = this.readUsers();

    if (!users.some(user => user.publicId === publicId)) {
      throw new Error('El usuario que intentas dar de baja ya no existe.');
    }

    this.persistAndNotify(users.map(user =>
      user.publicId === publicId ? { ...user, estado: 'inactivo' } : user
    ));
    return of(true);
  }

  getSnapshot(): UsuarioAdmin[] {
    return this.cloneUsers(this.readUsers());
  }

  importUsers(inputs: UsuarioLocalImportInput[]): UsuarioAdmin[] {
    const users = this.readUsers();
    const emails = new Set(users.map(user => user.email.toLocaleLowerCase()));
    const imported: UsuarioAdmin[] = [];

    for (const input of inputs) {
      const emailKey = input.email.trim().toLocaleLowerCase();
      if (emails.has(emailKey)) {
        throw new Error(`Ya existe un usuario con el correo ${input.email}.`);
      }

      const publicId = this.newPublicId();
      const user = this.validateUser({
        source: 'local',
        id: publicId,
        publicId,
        nombre: input.nombre.trim(),
        apellidoPaterno: input.apellidoPaterno.trim(),
        apellidoMaterno: input.apellidoMaterno?.trim() ?? '',
        email: input.email.trim(),
        telefono: input.telefono?.trim() ?? '',
        roles: [...input.roles],
        academiaPublicId: input.academiaPublicId ?? '',
        academiaNombre: input.academiaNombre ?? '',
        rolEnAcademia: input.rolEnAcademia ?? '',
        estado: 'activo'
      });

      emails.add(emailKey);
      imported.push(user);
    }

    this.persistAndNotify([...imported, ...users]);
    return this.cloneUsers(imported);
  }

  private readUsers(): UsuarioAdmin[] {
    const stored = localStorage.getItem(UsuariosLocalService.storageKey);

    if (stored === null) {
      const seed = USUARIOS.map(item => this.seedToUser(item));
      localStorage.setItem(
        UsuariosLocalService.storageKey,
        JSON.stringify(seed)
      );
      return seed;
    }

    try {
      const parsed: unknown = JSON.parse(stored);
      return Array.isArray(parsed)
        ? parsed.map(item => this.normalizeStoredUser(item))
        : [];
    } catch {
      throw new Error('No fue posible leer los usuarios guardados localmente.');
    }
  }

  private writeUsers(users: UsuarioAdmin[]): void {
    localStorage.setItem(
      UsuariosLocalService.storageKey,
      JSON.stringify(users)
    );
  }

  private persistAndNotify(users: UsuarioAdmin[]): void {
    this.writeUsers(users);
    this.changesSubject.next(this.cloneUsers(users));
  }

  private setAcademias(academias: Academia[]): void {
    this.academias = academias;
    this.academiaOptions.splice(
      0,
      this.academiaOptions.length,
      ...academias
        .filter(academia => academia.estado === 'activo')
        .map(academia => ({ label: academia.nombre, value: academia.publicId }))
    );
  }

  private resolveAcademia(user: UsuarioAdmin): UsuarioAdmin {
    const academia = user.academiaPublicId
      ? this.academias.find(item => item.publicId === user.academiaPublicId)
      : this.academias.find(item => item.nombre === user.academiaNombre);

    return academia
      ? {
          ...user,
          academiaPublicId: academia.publicId,
          academiaNombre: academia.nombre
        }
      : user;
  }

  private toUser(item: UiFormData, publicId: string): UsuarioAdmin {
    const academiaPublicId = this.optionalString(item, 'academiaPublicId');
    const academia = academiaPublicId
      ? this.academias.find(candidate => candidate.publicId === academiaPublicId)
      : undefined;

    if (academiaPublicId && (!academia || academia.estado !== 'activo')) {
      throw new Error('La academia seleccionada no existe o está inactiva.');
    }

    return this.validateUser({
      source: 'local',
      id: publicId,
      publicId,
      nombre: this.requiredString(item, 'nombre'),
      apellidoPaterno: this.requiredString(item, 'apellidoPaterno'),
      apellidoMaterno: this.optionalString(item, 'apellidoMaterno'),
      email: this.requiredString(item, 'email'),
      telefono: this.optionalString(item, 'telefono'),
      roles: this.rolesFrom(item['roles']),
      academiaPublicId: academia?.publicId ?? '',
      academiaNombre: academia?.nombre ?? '',
      rolEnAcademia: this.roleAcademiaFrom(item['rolEnAcademia']),
      estado: this.statusFrom(item['estado'])
    });
  }

  private validateUser(user: UsuarioAdmin): UsuarioAdmin {
    this.ensureMaxLength(user.nombre, 64, 'Nombre');
    this.ensureMaxLength(user.apellidoPaterno, 64, 'Apellido paterno');
    this.ensureMaxLength(user.apellidoMaterno ?? '', 64, 'Apellido materno');
    this.ensureMaxLength(user.email, 128, 'Correo electrónico');
    this.ensureMaxLength(user.telefono ?? '', 20, 'Teléfono');

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(user.email)) {
      throw new Error('El correo electrónico no tiene un formato válido.');
    }

    if (user.roles.length === 0) {
      throw new Error('Debes seleccionar al menos un rol del sistema.');
    }

    return user;
  }

  private ensureUniqueEmail(
    email: string,
    users: UsuarioAdmin[],
    ignoredPublicId?: string
  ): void {
    const normalized = email.trim().toLocaleLowerCase();
    const duplicate = users.some(user =>
      user.publicId !== ignoredPublicId &&
      user.email.trim().toLocaleLowerCase() === normalized
    );

    if (duplicate) {
      throw new Error(`Ya existe un usuario con el correo ${email}.`);
    }
  }

  private rolesFrom(value: unknown): SystemRole[] {
    if (!Array.isArray(value)) return [];

    const validRoles: SystemRole[] = ['DIRECTIVO', 'REVISOR', 'DOCENTE'];
    return [...new Set(value.filter(
      (role): role is SystemRole => validRoles.includes(role as SystemRole)
    ))];
  }

  private roleAcademiaFrom(value: unknown): RolAcademia | '' {
    return value === 'Docente' || value === 'Revisor' ? value : '';
  }

  private statusFrom(value: unknown): EntityStatus {
    return value === 'inactivo' ? 'inactivo' : 'activo';
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

  private ensureMaxLength(value: string, max: number, label: string): void {
    if (value.length > max) {
      throw new Error(`${label} no puede exceder ${max} caracteres.`);
    }
  }

  private seedToUser(item: Record<string, any>): UsuarioAdmin {
    const publicId = String(item['publicId']);
    return {
      source: 'local',
      id: publicId,
      publicId,
      nombre: String(item['nombre'] ?? ''),
      apellidoPaterno: String(item['apellidoPaterno'] ?? ''),
      apellidoMaterno: String(item['apellidoMaterno'] ?? ''),
      email: String(item['email'] ?? ''),
      telefono: String(item['telefono'] ?? ''),
      roles: this.normalizeRoles(item['roles']),
      academiaPublicId: String(item['academiaPublicId'] ?? ''),
      academiaNombre: String(item['academiaNombre'] ?? ''),
      rolEnAcademia: this.roleAcademiaFrom(item['rolEnAcademia']),
      estado: this.statusFrom(item['estado']),
      ultimoAcceso: typeof item['ultimoAcceso'] === 'string'
        ? item['ultimoAcceso']
        : undefined
    };
  }

  private normalizeStoredUser(value: unknown): UsuarioAdmin {
    const item = value && typeof value === 'object'
      ? value as Record<string, any>
      : {};
    const publicId = String(item['publicId'] ?? item['id'] ?? this.newPublicId());

    return {
      ...this.seedToUser({ ...item, publicId }),
      id: publicId,
      publicId
    };
  }

  private normalizeRoles(value: unknown): SystemRole[] {
    if (!Array.isArray(value)) return [];
    return this.rolesFrom(value.map(role => role === 'ADMIN' ? 'DIRECTIVO' : role));
  }

  private cloneUser(user: UsuarioAdmin): UsuarioAdmin {
    return { ...user, roles: [...user.roles] };
  }

  private cloneUsers(users: UsuarioAdmin[]): UsuarioAdmin[] {
    return users.map(user => this.cloneUser(user));
  }

  private newPublicId(): string {
    return globalThis.crypto.randomUUID();
  }
}
