import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { forkJoin, map, Observable } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import { CarreraResponseDto } from '../dto/carreras/carrera-response.dto';
import { CargaAcademicaResponseDto } from '../dto/cargas-academicas/carga-academica-response.dto';
import { AsignaturasService } from './asignaturas.service';
import { GruposService } from './grupos.service';
import { PeriodosService } from './periodos.service';
import { UsuariosService } from './usuarios.service';

export interface AcademiaPeriodoDetalle {
  id: string;
  asignatura: string;
  cuatrimestre: string;
  programaEducativo: string;
  docente: string;
}

export interface AcademiaPeriodo {
  id: string;
  publicId: string;
  periodo: string;
  registros: number;
  estado: 'activo';
  detalles: AcademiaPeriodoDetalle[];
}

@Injectable({ providedIn: 'root' })
export class AcademiasDetalleService {
  private readonly http = inject(HttpClient);
  private readonly asignaturasService = inject(AsignaturasService);
  private readonly gruposService = inject(GruposService);
  private readonly periodosService = inject(PeriodosService);
  private readonly usuariosService = inject(UsuariosService);
  private readonly cargasEndpoint = `${environment.apiUrl}/api/CargasAcademicas`;
  private readonly carrerasEndpoint = `${environment.apiUrl}/api/Carreras`;

  loadByPeriodo(): Observable<AcademiaPeriodo[]> {
    return forkJoin({
      cargas: this.http
        .get<ApiResponseDto<CargaAcademicaResponseDto[]>>(this.cargasEndpoint)
        .pipe(map(response => this.unwrap(response))),
      asignaturas: this.asignaturasService.load(),
      grupos: this.gruposService.load(),
      periodos: this.periodosService.load(),
      carreras: this.http
        .get<ApiResponseDto<CarreraResponseDto[]>>(this.carrerasEndpoint)
        .pipe(map(response => this.unwrap(response))),
      usuarios: this.usuariosService.load()
    }).pipe(
      map(({ cargas, asignaturas, grupos, periodos, carreras, usuarios }) => {
        const asignaturasById = new Map(asignaturas.map(item => [item.publicId, item]));
        const gruposById = new Map(grupos.map(item => [item.publicId, item]));
        const periodosById = new Map(periodos.map(item => [item.publicId, item]));
        const carrerasById = new Map(carreras.map(item => [item.publicId, item]));
        const usuariosById = new Map(usuarios.map(item => [item.publicId, item]));
        const result = new Map<string, AcademiaPeriodo>();

        for (const carga of cargas) {
          const asignatura = asignaturasById.get(carga.asignaturaPublicId);
          const grupo = gruposById.get(carga.grupoPublicId);
          const periodo = periodosById.get(carga.periodoPublicId);
          const carrera = grupo ? carrerasById.get(grupo.carreraPublicId) : undefined;
          const docente = usuariosById.get(carga.docentePublicId);
          if (!asignatura || !grupo || !periodo || !carrera || !docente) continue;

          const item = result.get(carga.periodoPublicId) ?? {
            id: carga.periodoPublicId,
            publicId: carga.periodoPublicId,
            periodo: periodo.nombre,
            registros: 0,
            estado: 'activo' as const,
            detalles: []
          };
          item.registros++;
          item.detalles.push({
            id: carga.publicId,
            asignatura: asignatura.nombre,
            cuatrimestre: grupo.nombre,
            programaEducativo: carrera.clave,
            docente: [docente.nombre, docente.apellidoPaterno, docente.apellidoMaterno]
              .filter(Boolean)
              .join(' ')
          });
          result.set(carga.periodoPublicId, item);
        }

        return [...result.values()]
          .map(item => ({
            ...item,
            detalles: item.detalles.sort((left, right) => left.asignatura.localeCompare(right.asignatura))
          }))
          .sort((left, right) => left.periodo.localeCompare(right.periodo));
      })
    );
  }

  private unwrap<T>(response: ApiResponseDto<T>): T {
    if (!response.success || response.data === null) {
      throw new Error(
        response.errors?.join(' ') || response.message || environment.defaultErrorMessage
      );
    }

    return response.data;
  }
}
