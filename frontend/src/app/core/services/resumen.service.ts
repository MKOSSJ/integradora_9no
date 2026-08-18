import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import { UserRole } from './auth.service';

export interface ResumenDashboard {
  usuariosRegistrados: number;
  academias: number;
  gruposActivos: number;
  importaciones: number;
  avancePlaneaciones: number;
}

export interface ResumenDashboardDocente {
  planeaciones: number;
  aprobadas: number;
  pendientes: number;
}

export interface ResumenDashboardRevisor {
  planeaciones: number;
  validadas: number;
  correcciones: number;
  planeacionesAValidar: number;
}

export type ResumenDashboardResponse =
  | ResumenDashboard
  | ResumenDashboardDocente
  | ResumenDashboardRevisor;

@Injectable({ providedIn: 'root' })
export class ResumenService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiUrl}/api/resumen`;

  dashboard(role: UserRole): Observable<ResumenDashboardResponse> {
    const resource = role === 'DIRECTIVO'
      ? 'resumen-dashboard'
      : role === 'REVISOR'
        ? 'resumen-dashboard-revisor'
        : 'resumen-dashboard-docente';

    return this.http
      .get<ApiResponseDto<ResumenDashboardResponse>>(`${this.endpoint}/${resource}`)
      .pipe(map(response => this.unwrap(response)));
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
