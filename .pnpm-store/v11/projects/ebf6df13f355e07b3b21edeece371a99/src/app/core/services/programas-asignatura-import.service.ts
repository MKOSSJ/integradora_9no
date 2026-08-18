import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom, map, switchMap } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import { ProgramaAsignaturaImportacionResultadoDto } from '../dto/programas-asignatura/programa-asignatura-importacion.dto';
import { UsuarioListResponseDto } from '../dto/usuarios/usuario-list-response.dto';
import { AuthService } from './auth.service';

type PreviewRow = Record<string, any>;

@Injectable({ providedIn: 'root' })
export class ProgramasAsignaturaImportService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly endpoint =
    `${environment.apiUrl}/api/programas-asignatura/importar`;
  private readonly usersEndpoint =
    `${environment.apiUrl}/api/Usuario/GetAll`;
  private selectedFile: File | null = null;

  readonly dataSource = {
    validate: (file: File) => this.validate(file),
    import: (_items: PreviewRow[]) => this.importSelectedFile(),
    downloadTemplate: () => undefined
  };

  private async validate(file: File): Promise<PreviewRow[]> {
    this.selectedFile = null;

    if (!file.name.toLowerCase().endsWith('.pdf')) {
      throw new Error('Solo se admiten programas de asignatura en formato PDF.');
    }

    const signature = await file.slice(0, 5).text();

    if (file.size === 0 || !signature.startsWith('%PDF-')) {
      throw new Error('El archivo no es un PDF válido.');
    }

    this.selectedFile = file;
    return [{
      id: file.name,
      archivo: file.name,
      asignatura: '',
      clave: '',
      unidadesExtraidas: '',
      datosGuardados: '',
      estado: 'validado',
      observacion: 'PDF listo para importar.'
    }];
  }

  private async importSelectedFile(): Promise<{
    message: string;
    type: 'success' | 'error';
    items: PreviewRow[];
  }> {
    const file = this.selectedFile;

    if (!file) {
      throw new Error('Selecciona y valida un PDF antes de importarlo.');
    }

    const response = await firstValueFrom(
      this.resolveAuthenticatedPublicId().pipe(
        switchMap(publicId => {
          const form = new FormData();
          form.append('Files', file, file.name);
          form.append('SubidoPorPublicId', publicId);

          return this.http.post<
            ApiResponseDto<ProgramaAsignaturaImportacionResultadoDto[]>
          >(this.endpoint, form);
        }),
        map(apiResponse => this.unwrap(apiResponse))
      )
    );

    const items = response.map((result, index) => ({
      id: result.programaAsignaturaPublicId ?? `${result.archivo}-${index}`,
      archivo: result.archivo,
      asignatura: result.asignatura ?? '',
      clave: result.clave ?? '',
      unidadesExtraidas: result.unidadesExtraidas,
      datosGuardados: result.datosGuardados ? 'Sí' : 'No',
      estado: result.errores.length > 0 ? 'error' : 'validado',
      observacion: result.errores.length > 0
        ? result.errores.join(' ')
        : result.datosGuardados
          ? 'Programa guardado correctamente.'
          : 'El programa ya estaba registrado.'
    }));
    const errors = response.flatMap(result => result.errores);

    return {
      type: errors.length > 0 ? 'error' : 'success',
      message: errors.length > 0
        ? errors.join(' ')
        : 'Importación de programa de asignatura finalizada.',
      items
    };
  }

  private resolveAuthenticatedPublicId() {
    const email = this.authService.currentUser()?.email.trim().toLowerCase();

    if (!email) {
      throw new Error(
        'La sesión actual no contiene un correo que permita identificar al usuario que realiza la carga.'
      );
    }

    return this.http.get<UsuarioListResponseDto[]>(this.usersEndpoint).pipe(
      map(users => users.filter(user =>
        user.email?.trim().toLowerCase() === email
      )),
      map(matches => {
        if (matches.length !== 1 || !matches[0].publicId) {
          throw new Error(
            'No fue posible resolver de forma inequívoca el publicId del usuario autenticado.'
          );
        }

        return matches[0].publicId;
      })
    );
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

