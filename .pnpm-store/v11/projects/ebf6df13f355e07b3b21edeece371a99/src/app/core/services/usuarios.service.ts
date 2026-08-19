import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import {
  defer,
  concat,
  EMPTY,
  expand,
  forkJoin,
  from,
  last,
  map,
  mergeMap,
  Observable,
  of,
  reduce,
  shareReplay,
  switchMap,
  throwError,
  toArray
} from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import { AsignarRolUsuarioRequestDto } from '../dto/usuarios/asignar-rol-usuario-request.dto';
import { UsuarioListResponseDto } from '../dto/usuarios/usuario-list-response.dto';
import {
  RolUsuarioResponseDto,
  UsuarioRolesResponseDto
} from '../dto/usuarios/usuario-roles-response.dto';
import {
  SystemRole,
  UsuarioBackendListItem
} from '../models/admin-catalogos.model';

type CatalogOption = { label: string; value: string };

interface PagedResultDto<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class UsuariosService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiUrl}/api/Usuario/GetAll`;
  private readonly rolesEndpoint = `${environment.apiUrl}/api/usuarios-roles`;
  private readonly roleRequests =
    new Map<string, Observable<UsuarioRolesResponseDto>>();
  private readonly roleCatalog =
    new Map<SystemRole, RolUsuarioResponseDto>();
  private readonly backendUsers =
    new Map<string, UsuarioBackendListItem>();
  private readonly unsupportedMutationMessage =
    'El backend no expone operaciones compatibles para modificar perfiles de usuario.';

  readonly roleOptions: CatalogOption[] = [];

  load(): Observable<UsuarioBackendListItem[]> {
    return defer(() => {
      this.roleRequests.clear();
      this.roleCatalog.clear();
      this.backendUsers.clear();

      return forkJoin({
        users: this.loadAllUsers(),
        roleCatalog: this.http
          .get<ApiResponseDto<RolUsuarioResponseDto[]>>(
            `${this.rolesEndpoint}/catalogo`
          )
          .pipe(map(response => this.unwrap(response)))
      }).pipe(
        mergeMap(({ users, roleCatalog }) => {
          this.setRoleOptions(roleCatalog);

          return from(users.map((user, index) => ({ user, index }))).pipe(
            mergeMap(({ user, index }) =>
              this.getUserRoles(user.publicId).pipe(
                map(roles => ({ user, roles, index }))
              ),
              6
            ),
            toArray(),
            map(resolved => resolved
              .sort((left, right) => left.index - right.index)
              .map(({ user, roles }) => this.toUiModel(user, roles))),
            map(users => {
              for (const user of users) {
                this.backendUsers.set(user.publicId, user);
              }
              return users;
            })
          );
        })
      );
    });
  }

  create(): Observable<never> {
    return throwError(() => new Error(this.unsupportedMutationMessage));
  }

  update(item: Record<string, unknown>): Observable<UsuarioBackendListItem> {
    return defer(() => {
      const publicId = this.requiredString(item, 'publicId');
      const current = this.backendUsers.get(publicId);

      if (item['source'] !== 'backend' || !current) {
        throw new Error('El usuario backend ya no está disponible en esta sesión.');
      }

      return this.fetchUserRoles(publicId).pipe(
        switchMap(currentRolesResponse => {
          const currentWithRealRoles = this.updateUserRoles(
            current,
            currentRolesResponse
          );
          const desiredRoles = this.rolesFrom(item['roles']);
          const additions = this.sortRoles(
            desiredRoles.filter(role =>
              !currentWithRealRoles.roles.includes(role)
            ),
            ['DOCENTE', 'REVISOR', 'DIRECTIVO']
          );
          const removals = this.sortRoles(
            currentWithRealRoles.roles.filter(role =>
              !desiredRoles.includes(role)
            ),
            ['REVISOR', 'DOCENTE', 'DIRECTIVO']
          );
          const requests = [
            ...additions.map(role => this.assignRole(publicId, role)),
            ...removals.map(role => this.removeRole(publicId, role))
          ];

          if (requests.length === 0) {
            throw new Error('No se detectaron cambios en los roles del usuario.');
          }

          return concat(...requests).pipe(
            last(),
            switchMap(() => this.fetchUserRoles(publicId)),
            map(response => this.updateUserRoles(
              currentWithRealRoles,
              response
            ))
          );
        })
      );
    });
  }

  delete(): Observable<boolean> {
    return throwError(() => new Error(this.unsupportedMutationMessage));
  }

  private getUserRoles(
    publicId: string
  ): Observable<UsuarioRolesResponseDto> {
    const cached = this.roleRequests.get(publicId);

    if (cached) return cached;

    const request = this.fetchUserRoles(publicId)
      .pipe(
        shareReplay({ bufferSize: 1, refCount: false })
      );

    this.roleRequests.set(publicId, request);
    return request;
  }

  ensureReviewerRole(usuarioPublicId: string): Observable<{ added: boolean }> {
    return this.fetchUserRoles(usuarioPublicId).pipe(
      switchMap(currentRoles => {
        if (currentRoles.roles.some(role => role.nombre === 'Revisor')) {
          this.syncUserRoles(usuarioPublicId, currentRoles);
          return of({ added: false });
        }

        return this.assignRole(usuarioPublicId, 'REVISOR').pipe(
          map(updatedRoles => {
            this.syncUserRoles(usuarioPublicId, updatedRoles);
            return { added: true };
          })
        );
      })
    );
  }

  private loadAllUsers(): Observable<UsuarioListResponseDto[]> {
    const pageSize = 100;
    const requestPage = (page: number) => this.http
      .get<ApiResponseDto<PagedResultDto<UsuarioListResponseDto>>>(
        `${this.endpoint}?page=${page}&pageSize=${pageSize}`
      )
      .pipe(map(response => this.unwrap(response)));

    return requestPage(1).pipe(
      expand(result =>
        result.page < result.totalPages
          ? requestPage(result.page + 1)
          : EMPTY
      ),
      reduce<PagedResultDto<UsuarioListResponseDto>, UsuarioListResponseDto[]>(
        (users, result) => [...users, ...result.items],
        []
      )
    );
  }

  private setRoleOptions(catalog: RolUsuarioResponseDto[]): void {
    this.roleCatalog.clear();

    for (const role of catalog) {
      const mapped = this.toSystemRole(role.nombre);
      if (mapped) this.roleCatalog.set(mapped, role);
    }

    this.roleOptions.splice(
      0,
      this.roleOptions.length,
      ...catalog.flatMap(role => {
        const mapped = this.toSystemRole(role.nombre);
        return mapped ? [{ label: role.nombre, value: mapped }] : [];
      })
    );
  }

  private toUiModel(
    user: UsuarioListResponseDto,
    userRoles: UsuarioRolesResponseDto
  ): UsuarioBackendListItem {
    return {
      source: 'backend',
      id: user.publicId,
      publicId: user.publicId,
      nombre: this.normalizeBackendText(user.nombre),
      apellidoPaterno: this.normalizeBackendText(user.apellidoPaterno),
      apellidoMaterno: this.normalizeBackendText(user.apellidoMaterno),
      email: this.normalizeBackendText(user.email),
      telefono: this.normalizeBackendText(user.telefono),
      ultimoAcceso: this.normalizeBackendText(user.ultimoAcceso),
      roles: userRoles.roles.flatMap(role => {
        const mapped = this.toSystemRole(role.nombre);
        return mapped ? [mapped] : [];
      }),
      academiaNombre: 'Sin información',
      rolEnAcademia: 'Sin información',
      estado: user.activo ? 'activo' : 'inactivo'
    };
  }

  private toSystemRole(name: string): SystemRole | null {
    if (name === 'Director') return 'DIRECTIVO';
    if (name === 'Revisor') return 'REVISOR';
    if (name === 'Docente') return 'DOCENTE';
    return null;
  }

  private fetchUserRoles(
    publicId: string
  ): Observable<UsuarioRolesResponseDto> {
    return this.http
      .get<ApiResponseDto<UsuarioRolesResponseDto>>(
        `${this.rolesEndpoint}/${publicId}`
      )
      .pipe(map(response => this.unwrap(response)));
  }

  private normalizeBackendText(value: string | null | undefined): string {
    const normalized = value?.trim() ?? '';
    return normalized.toLowerCase() === 'string' ? '' : normalized;
  }

  private assignRole(
    usuarioPublicId: string,
    role: SystemRole
  ): Observable<UsuarioRolesResponseDto> {
    const catalogRole = this.requireCatalogRole(role);
    const request: AsignarRolUsuarioRequestDto = {
      rolPublicId: catalogRole.publicId
    };

    return this.http
      .post<ApiResponseDto<UsuarioRolesResponseDto>>(
        `${this.rolesEndpoint}/${usuarioPublicId}`,
        request
      )
      .pipe(map(response => this.unwrap(response)));
  }

  private removeRole(
    usuarioPublicId: string,
    role: SystemRole
  ): Observable<UsuarioRolesResponseDto> {
    const catalogRole = this.requireCatalogRole(role);

    return this.http
      .delete<ApiResponseDto<UsuarioRolesResponseDto>>(
        `${this.rolesEndpoint}/${usuarioPublicId}/${catalogRole.publicId}`
      )
      .pipe(map(response => this.unwrap(response)));
  }

  private updateUserRoles(
    current: UsuarioBackendListItem,
    response: UsuarioRolesResponseDto
  ): UsuarioBackendListItem {
    const updated: UsuarioBackendListItem = {
      ...current,
      roles: response.roles.flatMap(role => {
        const mapped = this.toSystemRole(role.nombre);
        return mapped ? [mapped] : [];
      })
    };

    this.backendUsers.set(updated.publicId, updated);
    this.roleRequests.set(updated.publicId, of(response).pipe(
      shareReplay({ bufferSize: 1, refCount: false })
    ));
    return updated;
  }

  private syncUserRoles(
    publicId: string,
    response: UsuarioRolesResponseDto
  ): void {
    const current = this.backendUsers.get(publicId);
    if (current) this.updateUserRoles(current, response);
  }

  private requireCatalogRole(role: SystemRole): RolUsuarioResponseDto {
    const catalogRole = this.roleCatalog.get(role);

    if (!catalogRole) {
      throw new Error(`El rol ${role} no existe o está inactivo.`);
    }

    return catalogRole;
  }

  private rolesFrom(value: unknown): SystemRole[] {
    if (!Array.isArray(value)) return [];

    const valid: SystemRole[] = ['DIRECTIVO', 'REVISOR', 'DOCENTE'];
    return [...new Set(value.filter(
      (role): role is SystemRole => valid.includes(role as SystemRole)
    ))];
  }

  private sortRoles(
    roles: SystemRole[],
    order: SystemRole[]
  ): SystemRole[] {
    return [...roles].sort((left, right) =>
      order.indexOf(left) - order.indexOf(right)
    );
  }

  private requiredString(item: Record<string, unknown>, key: string): string {
    const value = item[key];

    if (typeof value !== 'string' || value.trim() === '') {
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
