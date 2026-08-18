import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { EMPTY, expand, map, Observable, reduce } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';

interface PagedResultDto<T> {
  items: T[];
  page: number;
  totalPages: number;
}

interface EntidadResumenDto {
  publicId: string;
  nombre: string;
}

interface RepositorioPlaneacionDto {
  publicId: string;
  asignatura: EntidadResumenDto;
  docentes: EntidadResumenDto[];
  grupos: EntidadResumenDto[];
  carreras: EntidadResumenDto[];
  estadoPlaneacion: number;
  fecha: string;
}

export type ReportStatus = 'validada' | 'enviada' | 'elaboracion' | 'observada' | 'revision';

export interface ReportePlaneacion {
  id: string;
  titulo: string;
  autor: string;
  estado: ReportStatus;
  progreso: number;
  fecha: string;
  carrera: string;
  grupo: string;
}

@Injectable({ providedIn: 'root' })
export class RepositorioService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiUrl}/api/repositorio/planeaciones`;

  loadAll(): Observable<ReportePlaneacion[]> {
    const pageSize = 100;
    const page = (number: number) => this.http
      .get<ApiResponseDto<PagedResultDto<RepositorioPlaneacionDto>>>(
        `${this.endpoint}?page=${number}&pageSize=${pageSize}`
      )
      .pipe(map(response => this.unwrap(response)));

    return page(1).pipe(
      expand(result => result.page < result.totalPages ? page(result.page + 1) : EMPTY),
      reduce<PagedResultDto<RepositorioPlaneacionDto>, RepositorioPlaneacionDto[]>(
        (items, result) => [...items, ...result.items],
        []
      ),
      map(items => items.map(item => this.toReport(item)))
    );
  }

  private toReport(item: RepositorioPlaneacionDto): ReportePlaneacion {
    const estado = this.toStatus(item.estadoPlaneacion);
    return {
      id: item.publicId,
      titulo: item.asignatura.nombre,
      autor: item.docentes.map(docente => docente.nombre).join(', '),
      estado,
      progreso: estado === 'validada' ? 100 : estado === 'revision' ? 70 : 0,
      fecha: item.fecha,
      carrera: item.carreras.map(carrera => carrera.nombre).join(', '),
      grupo: item.grupos.map(grupo => grupo.nombre).join(', ')
    };
  }

  private toStatus(estado: number): ReportStatus {
    if (estado === 5 || estado === 7) return 'validada';
    if (estado === 3) return 'revision';
    if (estado === 4 || estado === 6) return 'observada';
    if (estado === 2) return 'enviada';
    return 'elaboracion';
  }

  private unwrap<T>(response: ApiResponseDto<T>): T {
    if (!response.success || response.data === null) {
      throw new Error(response.errors?.join(' ') || response.message || environment.defaultErrorMessage);
    }
    return response.data;
  }
}
