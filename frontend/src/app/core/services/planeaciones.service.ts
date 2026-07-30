import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

import {
  PlaneacionDetail,
  PlaneacionListItem,
  SeguimientoEstado,
  SeguimientoPlaneacion
} from '../models/planeacion.model';

@Injectable({
  providedIn: 'root'
})
export class PlaneacionesService {
  private planeaciones: PlaneacionDetail[] = [
    {
      id: 1,
      titulo: 'Comunicación y habilidades digitales',
      descripcion: 'Planeación didáctica del programa de asignatura.',
      actualizacion: '2026-07-15T10:00:00.000Z',
      progreso: 85,
      status: 'borrador',
      autor: 'Docente de Comunicación',
      fechaCreacion: '2026-07-01T10:00:00.000Z',
      ultimaModificacion: '2026-07-15T10:00:00.000Z',
      fechaEnvioRevision: '2026-07-15T10:00:00.000Z',
      fechaLimiteCaptura: '2026-07-22T10:00:00.000Z',
      pdfPages: 21,
      programa: {
        id: 1,
        nombre: 'Comunicación y habilidades digitales',
        clave: 'CHD-101',
        programaEducativo: 'Técnico Superior Universitario',
        cuatrimestre: 'Primer cuatrimestre',
        creditos: 5,
        horasTotales: 60,
        horasSaber: 20,
        horasSaberHacer: 40,
        horasSemana: 4,
        proposito:
          'Desarrollar habilidades de comunicación oral, escrita y digital para el desempeño académico y profesional.',
        competencia:
          'Comunicar ideas de forma efectiva mediante herramientas digitales y estrategias de comunicación.',
        tipoCompetencia: 'Base',
        modalidad: 'Escolarizada',
        referenciasBase: [
          {
            id: 1,
            tipo: 'Libro',
            autor: 'Cassany, D.',
            anio: '2021',
            titulo: 'La cocina de la escritura',
            fuente: 'Editorial Anagrama',
            precargada: true
          }
        ]
      },
      formulario: {
        titulo: 'Comunicación y habilidades digitales',
        periodoId: 1,
        asignaturaId: 1,
        academiaId: 1,
        programaAsignaturaId: 1,
        revisorId: 2,
        docenteIds: [3],
        grupoIds: [1],
        caratula: {
          programaEducativo: 'Técnico Superior Universitario',
          docentes: 'Docente de Comunicación',
          cuatrimestre: 'Primer cuatrimestre',
          periodoEscolar: 'Septiembre - Diciembre 2026',
          asignatura: 'Comunicación y habilidades digitales',
          grupos: '1A',
          propositoAsignatura:
            'Desarrollar habilidades de comunicación oral, escrita y digital.',
          competenciaContribuye:
            'Comunicar ideas de forma efectiva mediante herramientas digitales.',
          tipoCompetencia: 'Base',
          creditos: 5,
          modalidad: 'Escolarizada',
          horasSaber: 20,
          horasSaberHacer: 40,
          horasTotales: 60,
          horasSemana: 4
        },
        unidades: [
          {
            id: 1,
            numero: 1,
            nombre: 'Fundamentos de la información',
            propositoEsperado:
              'Identificar fuentes confiables de información para su uso académico.',
            horasSaber: 5,
            horasSaberHacer: 10,
            horasTotales: 15,
            porcentajeUnidad: 20,
            periodoSemanas: 3,
            resultadoAprendizaje:
              'Determina el uso de información confiable para resolver necesidades académicas.',
            temas: [
              {
                id: 1,
                tema: 'Fuentes de información',
                saber: 'Reconoce fuentes primarias y secundarias.',
                saberHacer: 'Selecciona fuentes confiables.',
                saberSerConvivir: 'Actúa con responsabilidad en el uso de información.'
              }
            ],
            evaluaciones: [
              {
                id: 1,
                evidenciaAprendizaje: 'Reporte de investigación documental',
                ponderacion: 100,
                tiposEvaluacion: [
                  {
                    id: 1,
                    fase: 'apertura',
                    tipoEvaluacion: 'Conceptual',
                    agenteEvaluacion: 'Heteroevaluación'
                  },
                  {
                    id: 2,
                    fase: 'desarrollo',
                    tipoEvaluacion: 'Reporte académico',
                    agenteEvaluacion: 'Heteroevaluación'
                  },
                  {
                    id: 3,
                    fase: 'cierre',
                    tipoEvaluacion: 'Análisis de desempeño',
                    agenteEvaluacion: 'Autoevaluación'
                  }
                ],
                instrumentos: [
                  {
                    id: 1,
                    categoria: 'Conocimiento',
                    nombre: 'Prueba de conocimientos',
                    instrumento: 'Prueba de opción múltiple',
                    ponderacion: 30
                  },
                  {
                    id: 2,
                    categoria: 'Producto',
                    nombre: 'Reporte documental',
                    instrumento: 'Lista de cotejo',
                    ponderacion: 50
                  },
                  {
                    id: 3,
                    categoria: 'Desempeño',
                    nombre: 'Autoanálisis',
                    instrumento: 'Guía de observación',
                    ponderacion: 20
                  }
                ]
              }
            ],
            apertura: [
              {
                id: 1,
                consecutivo: 1,
                metodoTecnica: 'Lluvia de ideas',
                actividadesDocentes:
                  'Presenta el propósito de la unidad y plantea preguntas detonadoras.',
                actividadesEstudiantes:
                  'Participan compartiendo conocimientos previos sobre fuentes de información.',
                evidenciaAprendizaje: 'Participación diagnóstica',
                recursos: 'Pizarrón, presentación digital'
              }
            ],
            desarrollo: [
              {
                id: 2,
                consecutivo: 2,
                metodoTecnica: 'Investigación',
                actividadesDocentes:
                  'Guía la búsqueda y selección de fuentes confiables.',
                actividadesEstudiantes:
                  'Buscan, comparan y seleccionan fuentes de información.',
                evidenciaAprendizaje: 'Listado de fuentes confiables',
                recursos: 'Internet, biblioteca digital'
              }
            ],
            cierre: [
              {
                id: 3,
                consecutivo: 3,
                metodoTecnica: 'Cuestionario reflexivo',
                actividadesDocentes:
                  'Retroalimenta los resultados y orienta conclusiones.',
                actividadesEstudiantes:
                  'Reflexionan sobre la importancia de validar información.',
                evidenciaAprendizaje: 'Reflexión escrita',
                recursos: 'Formulario digital'
              }
            ],
            referencias: [
              {
                id: 1,
                tipo: 'Libro',
                autor: 'Cassany, D.',
                anio: '2021',
                titulo: 'La cocina de la escritura',
                fuente: 'Editorial Anagrama',
                precargada: true
              }
            ]
          }
        ]
      }
    },
    {
      id: 2,
      titulo: 'Matemáticas Básicas — Álgebra',
      descripcion: 'Planeación didáctica para álgebra básica.',
      actualizacion: '2026-07-10T09:00:00.000Z',
      progreso: 45,
      status: 'borrador',
      autor: 'Docente de Matemáticas',
      fechaCreacion: '2026-07-05T09:00:00.000Z',
      ultimaModificacion: '2026-07-10T09:00:00.000Z',
      fechaLimiteCaptura: '2026-07-26T09:00:00.000Z',
      pdfPages: 12,
      programa: {
        id: 2,
        nombre: 'Matemáticas Básicas',
        clave: 'MAT-101',
        programaEducativo: 'Técnico Superior Universitario',
        cuatrimestre: 'Primer cuatrimestre',
        creditos: 5,
        horasTotales: 60,
        horasSaber: 25,
        horasSaberHacer: 35,
        horasSemana: 4,
        proposito:
          'Fortalecer el razonamiento lógico-matemático mediante el uso de operaciones algebraicas.',
        competencia:
          'Resolver problemas matemáticos básicos mediante procedimientos algebraicos.',
        tipoCompetencia: 'Base',
        modalidad: 'Escolarizada',
        referenciasBase: []
      },
      formulario: {
        titulo: 'Matemáticas Básicas — Álgebra',
        periodoId: 1,
        asignaturaId: 2,
        academiaId: 2,
        programaAsignaturaId: 2,
        revisorId: 2,
        docenteIds: [4],
        grupoIds: [2],
        caratula: {
          programaEducativo: 'Técnico Superior Universitario',
          docentes: 'Docente de Matemáticas',
          cuatrimestre: 'Primer cuatrimestre',
          periodoEscolar: 'Septiembre - Diciembre 2026',
          asignatura: 'Matemáticas Básicas',
          grupos: '1B',
          propositoAsignatura:
            'Fortalecer el razonamiento lógico-matemático.',
          competenciaContribuye:
            'Resolver problemas matemáticos mediante procedimientos algebraicos.',
          tipoCompetencia: 'Base',
          creditos: 5,
          modalidad: 'Escolarizada',
          horasSaber: 25,
          horasSaberHacer: 35,
          horasTotales: 60,
          horasSemana: 4
        },
        unidades: [
          {
            id: 1,
            numero: 1,
            nombre: 'Álgebra básica',
            propositoEsperado:
              'Aplicar operaciones algebraicas en la resolución de problemas.',
            horasSaber: 8,
            horasSaberHacer: 12,
            horasTotales: 20,
            porcentajeUnidad: 30,
            periodoSemanas: 3,
            resultadoAprendizaje:
              'Resuelve problemas algebraicos utilizando operaciones básicas.',
            temas: [],
            evaluaciones: [],
            apertura: [],
            desarrollo: [],
            cierre: [],
            referencias: []
          }
        ]
      }
    }
  ];

  getPlaneaciones(): Observable<PlaneacionListItem[]> {
    return of(
      this.planeaciones.map(item => ({
        id: item.id,
        titulo: item.titulo,
        descripcion: item.descripcion,
        actualizacion: item.actualizacion,
        progreso: item.progreso,
        status: item.status
      }))
    );
  }

  getPlaneacionById(id: number): Observable<PlaneacionDetail | undefined> {
    return of(this.planeaciones.find(item => item.id === id));
  }

  saveDraft(id: number): Observable<boolean> {
    const planeacion = this.planeaciones.find(item => item.id === id);

    if (planeacion) {
      planeacion.ultimaModificacion = new Date().toISOString();
      planeacion.actualizacion = new Date().toISOString();
    }

    return of(true);
  }

  submitForApproval(id: number): Observable<boolean> {
    const planeacion = this.planeaciones.find(item => item.id === id);

    if (planeacion) {
      planeacion.status = 'pendiente';
      planeacion.fechaEnvioRevision = new Date().toISOString();
      planeacion.ultimaModificacion = new Date().toISOString();
      planeacion.actualizacion = new Date().toISOString();
    }

    return of(true);
  }

  getSeguimientoDirectivo(): Observable<SeguimientoPlaneacion[]> {
    return of(
      this.planeaciones.map(item => {
        const fechaLimite =
          item.fechaLimiteCaptura ?? this.calculateLimitDate(item.fechaCreacion);

        const diasRestantes = this.getDaysRemaining(fechaLimite);

        const estadoSeguimiento = this.getSeguimientoEstado(
          item,
          diasRestantes
        );

        return {
          id: item.id,
          titulo: item.titulo,
          docente: item.autor,
          asignatura: item.formulario.caratula.asignatura,
          grupos: item.formulario.caratula.grupos,
          status: item.status,

          fechaCreacion: item.fechaCreacion,
          fechaLimiteCaptura: fechaLimite,
          fechaEnvioRevision: item.fechaEnvioRevision,
          fechaValidacion: item.fechaValidacion,
          fechaAutorizacion: item.fechaAutorizacion,

          diasRestantes,
          estadoSeguimiento
        };
      })
    );
  }

  private calculateLimitDate(fechaCreacion: string): string {
    const date = new Date(fechaCreacion);

    date.setDate(date.getDate() + 21);

    return date.toISOString();
  }

  private getDaysRemaining(fechaLimite: string): number {
    const today = new Date();
    const limit = new Date(fechaLimite);

    today.setHours(0, 0, 0, 0);
    limit.setHours(0, 0, 0, 0);

    const diff = limit.getTime() - today.getTime();

    return Math.ceil(diff / (1000 * 60 * 60 * 24));
  }

  private getSeguimientoEstado(
    planeacion: PlaneacionDetail,
    diasRestantes: number
  ): SeguimientoEstado {
    if (
      planeacion.status === 'aprobado' ||
      planeacion.fechaValidacion ||
      planeacion.fechaAutorizacion
    ) {
      return 'completada';
    }

    if (diasRestantes < 0) {
      return 'vencida';
    }

    if (diasRestantes <= 5) {
      return 'por-vencer';
    }

    return 'en-tiempo';
  }
}