import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';

export interface ProgramaAsignaturaResumen {
  publicId: string;
  asignatura: string;
  claveAsignatura: string | null;
  carrera: string | null;
  cuatrimestre: number | null;
  nombreArchivo: string;
  fechaSubida: string;
  estado: string;
  subidoPor: string | null;
}

@Injectable({ providedIn: 'root' })
export class ProgramasAsignaturaService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiUrl}/api/programas-asignatura`;

  load(): Observable<ProgramaAsignaturaResumen[]> {
    return this.http.get<ApiResponseDto<ProgramaAsignaturaResumen[]>>(this.endpoint).pipe(
      map(response => this.unwrap(response))
    );
  }

  view(publicId: string): Observable<Blob> {
    return this.http.get(`${this.endpoint}/${publicId}/archivo`, { responseType: 'blob' });
  }

  download(publicId: string): Observable<Blob> {
    return this.http.get(`${this.endpoint}/${publicId}/archivo/descarga`, { responseType: 'blob' });
  }

  private unwrap<T>(response: ApiResponseDto<T>): T {
    if (!response.success || response.data === null) {
      throw new Error(response.errors?.join(' ') || response.message || environment.defaultErrorMessage);
    }
    return response.data;
  }
}
