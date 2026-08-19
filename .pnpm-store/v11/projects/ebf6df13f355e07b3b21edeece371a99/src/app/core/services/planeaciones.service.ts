import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { defer, map, Observable, switchMap, tap, throwError } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import {
  ComentarioCorreccionDto,
  ComentariosCorreccionDto,
  EvaluacionPlaneacionEdicionDto,
  PlaneacionEdicionDto,
  PlaneacionResumenDto,
  ReferenciaPlaneacionEdicionDto,
  SecuenciaPlaneacionEdicionDto
} from '../dto/planeaciones/planeacion-flujo.dto';

import {
  ActividadSecuencia,
  AgenteEvaluacion,
  EvaluacionUnidad,
  FaseSecuencia,
  PlaneacionDetail,
  PlaneacionListItem,
  PlaneacionStatus,
  ReferenciaBibliografica,
  SeguimientoPlaneacion,
  UnidadPlaneacion
} from '../models/planeacion.model';
import { PlaneacionDetalleConArchivosResponseDto } from '../dto/planeaciones/planeaciones-directivo.dto';

@Injectable({
  providedIn: 'root'
})
export class PlaneacionesService {
  private readonly http = inject(HttpClient);
  private readonly flowEndpoint =
    `${environment.apiUrl}/api/planeaciones-flujo/mis-planeaciones`;
  private readonly commentsEndpoint =
    `${environment.apiUrl}/api/planeaciones`;
  private readonly summaries = new Map<string, PlaneacionResumenDto>();
  private readonly detailDtos = new Map<string, PlaneacionEdicionDto>();

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

  getPlaneaciones(): Observable<PlaneacionListItem<string>[]> {
    return this.http
      .get<ApiResponseDto<PlaneacionResumenDto[]>>(this.flowEndpoint)
      .pipe(
        map(response => this.unwrap(response)),
        tap(items => {
          this.summaries.clear();
          for (const item of items) this.summaries.set(item.publicId, item);
        }),
        map(items => items.map(item => this.toListItem(item)))
      );
  }

  getPlaneacionById(publicId: string): Observable<PlaneacionDetail<string>> {
    return this.http
      .get<ApiResponseDto<PlaneacionEdicionDto>>(
        `${this.flowEndpoint}/${publicId}`
      )
      .pipe(
        map(response => this.unwrap(response)),
        tap(detail => this.detailDtos.set(detail.publicId, detail)),
        map(detail => this.toDetail(detail))
      );
  }

  getPlaneacionAdministrativaById(publicId: string): Observable<PlaneacionDetail<string>> {
    return this.http
      .get<ApiResponseDto<{ planeacion: PlaneacionEdicionDto }>>(
        `${environment.apiUrl}/api/planeaciones/${publicId}`
      )
      .pipe(
        map(response => this.unwrap(response).planeacion),
        tap(detail => this.detailDtos.set(detail.publicId, detail)),
        map(detail => this.toDetail(detail))
      );
  }

  saveDraft(
    planeacion: PlaneacionDetail<string>
  ): Observable<PlaneacionDetail<string>> {
    return defer(() => {
      const publicId = String(planeacion.id);
      const request = this.toEditDto(planeacion);

      return this.http
        .put<ApiResponseDto<PlaneacionEdicionDto>>(
          `${this.flowEndpoint}/${publicId}`,
          request
        )
        .pipe(
          map(response => this.unwrap(response)),
          tap(detail => this.detailDtos.set(detail.publicId, detail)),
          map(detail => this.toDetail(detail))
        );
    });
  }

  submitForApproval(publicId: string): Observable<PlaneacionStatus> {
    return this.http
      .post<ApiResponseDto<PlaneacionResumenDto>>(
        `${this.flowEndpoint}/${publicId}/enviar-revision`,
        {}
      )
      .pipe(
        map(response => this.unwrap(response)),
        tap(summary => this.summaries.set(summary.publicId, summary)),
        map(summary => this.toStatus(summary.estado))
      );
  }

  getComments(publicId: string): Observable<ComentariosCorreccionDto> {
    return this.http
      .get<ApiResponseDto<ComentariosCorreccionDto>>(
        `${this.commentsEndpoint}/${publicId}/comentarios-correccion`
      )
      .pipe(map(response => this.unwrap(response)));
  }

  addComment(
    publicId: string,
    mensaje: string
  ): Observable<ComentarioCorreccionDto> {
    return this.http
      .post<ApiResponseDto<ComentarioCorreccionDto>>(
        `${this.commentsEndpoint}/${publicId}/comentarios-correccion`,
        { mensaje }
      )
      .pipe(map(response => this.unwrap(response)));
  }

  getPlaneacionPdf(publicId: string): Observable<Blob> {
    return this.http.get(`${environment.apiUrl}/api/planeaciones/${publicId}/pdf`, {
      responseType: 'blob'
    });
  }

  getProgramaPdf(publicId: string): Observable<Blob> {
    return this.http
      .get<ApiResponseDto<PlaneacionDetalleConArchivosResponseDto>>(
        `${environment.apiUrl}/api/planeaciones/${publicId}`
      )
      .pipe(
        map(response => this.unwrap(response).archivos.programaAsignatura),
        switchMap(file => {
          if (!file.disponible || !file.urlVisualizacion) {
            throw new Error('La API no tiene un programa de asignatura disponible para esta planeación.');
          }

          return this.http.get(this.apiUrl(file.urlVisualizacion), {
            responseType: 'blob'
          });
        })
      );
  }

  private toListItem(item: PlaneacionResumenDto): PlaneacionListItem<string> {
    return {
      id: item.publicId,
      titulo: item.asignatura,
      descripcion: [item.periodo, item.grupos, item.docentes]
        .filter(value => !!value?.trim())
        .join(' · '),
      actualizacion: item.ultimaModificacion ?? '',
      progreso: 0,
      status: this.toStatus(item.estado)
    };
  }

  toDetail(dto: PlaneacionEdicionDto): PlaneacionDetail<string> {
    const summary = this.summaries.get(dto.publicId);
    const caratula = dto.caratula;
    const referencias = dto.referencias.map((item, index) =>
      this.toReference(item, index)
    );
    const unidades = dto.unidades.map((item, index) =>
      this.toUnit(item, index, index === 0 ? referencias : [])
    );
    const titulo = caratula.nombreAsignatura?.trim() ||
      summary?.asignatura?.trim() || '';
    const actualizacion = summary?.ultimaModificacion ?? '';

    return {
      id: dto.publicId,
      titulo,
      descripcion: [
        caratula.periodoEscolar,
        caratula.grupos,
        caratula.docentes
      ].filter(value => !!value?.trim()).join(' · '),
      actualizacion,
      progreso: 0,
      status: this.toStatus(dto.estado),
      autor: caratula.docentes?.trim() ?? '',
      fechaCreacion: '',
      ultimaModificacion: actualizacion,
      pdfPages: 0,
      programa: {
        nombre: titulo,
        clave: '',
        programaEducativo: caratula.programaEducativo ?? '',
        cuatrimestre: caratula.cuatrimestre?.toString() ?? '',
        creditos: caratula.creditos ?? 0,
        horasTotales: caratula.horasTotales ?? 0,
        horasSaber: caratula.horasSaber ?? 0,
        horasSaberHacer: caratula.horasSaberHacer ?? 0,
        horasSemana: caratula.horasSemana ?? 0,
        proposito: caratula.propositoAsignatura ?? '',
        competencia: caratula.competenciaAsignatura ?? '',
        tipoCompetencia: this.tipoCompetencia(caratula.tipoCompetencia),
        modalidad: this.modalidad(caratula.modalidad),
        referenciasBase: referencias
      },
      formulario: {
        titulo,
        periodoId: null,
        asignaturaId: null,
        academiaId: null,
        programaAsignaturaId: null,
        revisorId: null,
        docenteIds: [],
        grupoIds: [],
        caratula: {
          programaEducativo: caratula.programaEducativo ?? '',
          docentes: caratula.docentes ?? '',
          cuatrimestre: caratula.cuatrimestre?.toString() ?? '',
          periodoEscolar: caratula.periodoEscolar ?? '',
          asignatura: titulo,
          grupos: caratula.grupos ?? '',
          propositoAsignatura: caratula.propositoAsignatura ?? '',
          competenciaContribuye: caratula.competenciaAsignatura ?? '',
          tipoCompetencia: this.tipoCompetencia(caratula.tipoCompetencia),
          creditos: caratula.creditos ?? 0,
          modalidad: this.modalidad(caratula.modalidad),
          horasSaber: caratula.horasSaber ?? 0,
          horasSaberHacer: caratula.horasSaberHacer ?? 0,
          horasTotales: caratula.horasTotales ?? 0,
          horasSemana: caratula.horasSemana ?? 0
        },
        unidades
      }
    };
  }

  private toUnit(
    dto: PlaneacionEdicionDto['unidades'][number],
    index: number,
    referencias: ReferenciaBibliografica[]
  ): UnidadPlaneacion {
    const evaluaciones = dto.evaluaciones.map((item, evaluationIndex) =>
      this.toEvaluation(item, evaluationIndex)
    );

    return {
      id: dto.publicId ?? -(index + 1),
      numero: dto.numeroUnidad ?? undefined,
      nombre: dto.nombreUnidad,
      propositoEsperado: dto.propositoEsperado ?? '',
      horasSaber: dto.horasSaber ?? 0,
      horasSaberHacer: dto.horasSaberHacer ?? 0,
      horasTotales: dto.horasTotales ?? 0,
      porcentajeUnidad: dto.porcentajeUnidad ?? 0,
      periodoSemanas: dto.evaluaciones[0]?.periodoSemanas ?? 0,
      resultadoAprendizaje:
        dto.evaluaciones[0]?.resultadoAprendizaje ?? '',
      temas: dto.temas.map((item, topicIndex) => ({
        id: item.publicId ?? -(topicIndex + 1),
        tema: item.tema,
        saber: item.saberConceptual ?? '',
        saberHacer: item.saberHacer ?? '',
        saberSerConvivir: item.saberSer ?? ''
      })),
      evaluaciones,
      apertura: this.toActivities(dto.apertura ?? dto.secuencias ?? [], 'apertura'),
      desarrollo: this.toActivities(dto.desarrollo ?? [], 'desarrollo'),
      cierre: this.toActivities(dto.cierre ?? [], 'cierre'),
      referencias
    };
  }

  private toEvaluation(
    dto: EvaluacionPlaneacionEdicionDto,
    index: number
  ): EvaluacionUnidad {
    const id = dto.publicId ?? -(index + 1);
    const instrumento = dto.instrumentoEvaluacion?.trim();

    return {
      id,
      evidenciaAprendizaje: dto.evidenciaAprendizaje ?? '',
      ponderacion: dto.ponderacion ?? 0,
      tiposEvaluacion: [{
        id,
        fase: this.toPhase(dto.fase),
        tipoEvaluacion: this.tipoEvaluacionLabel(dto.tipoEvaluacion),
        agenteEvaluacion: this.agenteLabel(dto.agenteEvaluador)
      }],
      instrumentos: instrumento ? [{
        id,
        categoria: '',
        nombre: '',
        instrumento,
        ponderacion: 0
      }] : []
    };
  }

  private toActivities(
    items: SecuenciaPlaneacionEdicionDto[],
    phase: FaseSecuencia
  ): ActividadSecuencia[] {
    const phaseValue = this.phaseValue(phase);

    return items
      .filter(item => item.fase === phaseValue)
      .map((item, index) => ({
        id: item.publicId ?? -(index + 1),
        consecutivo: item.orden,
        metodoTecnica: this.methodLabel(phase, item.metodoTecnica, item.estrategia),
        actividadesDocentes: item.actividadDocente ?? '',
        actividadesEstudiantes: item.actividadEstudiante ?? '',
        evidenciaAprendizaje: item.evidenciaAprendizaje ?? '',
        recursos: item.mediosMateriales ?? ''
      }));
  }

  private toReference(
    item: ReferenciaPlaneacionEdicionDto,
    index: number
  ): ReferenciaBibliografica {
    return {
      id: item.publicId ?? -(index + 1),
      tipo: 'Otro',
      autor: '',
      anio: '',
      titulo: item.referenciaAPA,
      fuente: '',
      url: '',
      precargada: true
    };
  }

  private toEditDto(
    planeacion: PlaneacionDetail<string>
  ): PlaneacionEdicionDto {
    const publicId = String(planeacion.id);
    const original = this.detailDtos.get(publicId);

    if (!original) {
      throw new Error('No existe un detalle backend para guardar esta planeación.');
    }

    const caratula = planeacion.formulario.caratula;
    const references = planeacion.formulario.unidades
      .flatMap(unit => unit.referencias)
      .filter((item, index, items) =>
        items.findIndex(candidate => candidate.id === item.id) === index
      );

    return {
      publicId,
      estado: original.estado,
      caratula: {
        programaEducativo: this.nullableText(caratula.programaEducativo),
        cuatrimestre: this.nullableNumber(caratula.cuatrimestre, 'cuatrimestre'),
        nombreAsignatura: this.nullableText(caratula.asignatura),
        docentes: this.nullableText(caratula.docentes),
        periodoEscolar: this.nullableText(caratula.periodoEscolar),
        grupos: this.nullableText(caratula.grupos),
        propositoAsignatura: this.nullableText(caratula.propositoAsignatura),
        competenciaAsignatura: this.nullableText(caratula.competenciaContribuye),
        tipoCompetencia: this.nullableText(caratula.tipoCompetencia),
        creditos: this.nullableNumber(caratula.creditos, 'créditos'),
        modalidad: this.nullableText(caratula.modalidad),
        horasSaber: this.nullableNumber(caratula.horasSaber, 'horas del saber'),
        horasSaberHacer: this.nullableNumber(caratula.horasSaberHacer, 'horas del saber hacer'),
        horasTotales: this.nullableNumber(caratula.horasTotales, 'horas totales'),
        horasSemana: this.nullableNumber(caratula.horasSemana, 'horas por semana')
      },
      unidades: planeacion.formulario.unidades.map((unit, index) => {
        const originalUnit = original.unidades.find(item => item.publicId === unit.id);

        return {
          publicId: originalUnit?.publicId ?? null,
          numeroUnidad: unit.numero ?? null,
          nombreUnidad: unit.nombre,
          propositoEsperado: this.nullableText(unit.propositoEsperado),
          horasSaber: unit.horasSaber,
          horasSaberHacer: unit.horasSaberHacer,
          horasTotales: unit.horasTotales,
          porcentajeUnidad: unit.porcentajeUnidad,
          orden: index + 1,
          temas: unit.temas.map((topic, topicIndex) => ({
            publicId: originalUnit?.temas.some(item => item.publicId === topic.id)
              ? String(topic.id)
              : null,
            tema: topic.tema,
            saberConceptual: this.nullableText(topic.saber),
            saberHacer: this.nullableText(topic.saberHacer),
            saberSer: this.nullableText(topic.saberSerConvivir),
            orden: topicIndex + 1
          })),
          evaluaciones: this.toEvaluationDtos(
            unit,
            originalUnit?.evaluaciones ?? []
          ),
          secuencias: this.toSequenceDtos(
            unit,
            [
              ...(originalUnit?.apertura ?? originalUnit?.secuencias ?? []),
              ...(originalUnit?.desarrollo ?? []),
              ...(originalUnit?.cierre ?? [])
            ]
          )
        };
      }),
      referencias: references.map((reference, index) => ({
        publicId: original.referencias.some(item => item.publicId === reference.id)
          ? String(reference.id)
          : null,
        referenciaAPA: this.formatReference(reference),
        orden: index + 1
      }))
    };
  }

  private toEvaluationDtos(
    unit: UnidadPlaneacion,
    originals: EvaluacionPlaneacionEdicionDto[]
  ): EvaluacionPlaneacionEdicionDto[] {
    let order = 0;

    return unit.evaluaciones.flatMap(evaluation => {
      const original = originals.find(item => item.publicId === evaluation.id);
      const types = evaluation.tiposEvaluacion;

      if (types.length === 0) {
        throw new Error('Cada evaluación debe conservar al menos un tipo de evaluación.');
      }

      return types.map((type, typeIndex) => {
        const fase = this.phaseValue(type.fase);
        const agente = this.agentValue(type.agenteEvaluacion);
        const tipo = this.tipoEvaluacionValue(type.tipoEvaluacion);

        if (!fase || !agente) {
          throw new Error('La fase o el agente de evaluación no corresponde al contrato backend.');
        }

        if (type.tipoEvaluacion.trim() && tipo === null) {
          throw new Error(`El tipo de evaluación "${type.tipoEvaluacion}" no existe en el backend.`);
        }

        const instrument = evaluation.instrumentos[typeIndex] ??
          evaluation.instrumentos[0];
        order++;

        return {
          publicId: typeIndex === 0 ? original?.publicId ?? null : null,
          periodoSemanas: unit.periodoSemanas,
          resultadoAprendizaje: this.nullableText(unit.resultadoAprendizaje),
          evidenciaAprendizaje: this.nullableText(evaluation.evidenciaAprendizaje),
          fase,
          tipoEvaluacion: tipo,
          agenteEvaluador: agente,
          ponderacion: evaluation.ponderacion,
          instrumentoEvaluacion: this.nullableText(instrument?.instrumento),
          orden: order
        };
      });
    });
  }

  private toSequenceDtos(
    unit: UnidadPlaneacion,
    originals: SecuenciaPlaneacionEdicionDto[]
  ): SecuenciaPlaneacionEdicionDto[] {
    let order = 0;

    return (['apertura', 'desarrollo', 'cierre'] as FaseSecuencia[])
      .flatMap(phase => unit[phase].map(activity => {
        const original = originals.find(item => item.publicId === activity.id);
        const metodoTecnica = this.methodValue(activity.metodoTecnica);
        const estrategia: number | null = (metodoTecnica === null
          ? this.strategyValue(phase, activity.metodoTecnica) ?? original?.estrategia
          : null) ?? null;

        if (metodoTecnica === null && !estrategia) {
          throw new Error(`El método o estrategia "${activity.metodoTecnica}" no existe en el backend.`);
        }

        order++;
        return {
          publicId: original?.publicId ?? null,
          fase: this.phaseValue(phase),
          metodoTecnica,
          estrategia,
          actividadDocente: this.nullableText(activity.actividadesDocentes),
          actividadEstudiante: this.nullableText(activity.actividadesEstudiantes),
          evidenciaAprendizaje: this.nullableText(activity.evidenciaAprendizaje),
          mediosMateriales: this.nullableText(activity.recursos),
          orden: order
        };
      }));
  }

  private formatReference(reference: ReferenciaBibliografica): string {
    if (!reference.autor && !reference.anio && !reference.fuente && !reference.url) {
      return reference.titulo.trim();
    }

    return [
      reference.autor.trim(),
      reference.anio.trim() ? `(${reference.anio.trim()}).` : '',
      reference.titulo.trim(),
      reference.fuente.trim(),
      reference.url?.trim() ?? ''
    ].filter(Boolean).join(' ');
  }

  private toStatus(value: number): PlaneacionStatus {
    const status = ({
      1: 'borrador',
      2: 'en-proceso',
      3: 'revision',
      4: 'correcciones',
      5: 'aprobado',
      6: 'rechazada',
      7: 'finalizada',
      8: 'reabierta'
    } as Record<number, PlaneacionStatus>)[value];

    if (!status) throw new Error(`El estado de planeación ${value} no es válido.`);
    return status;
  }

  private toPhase(value: number): FaseSecuencia {
    return ({ 1: 'apertura', 2: 'desarrollo', 3: 'cierre' } as const)[
      value as 1 | 2 | 3
    ] ?? 'desarrollo';
  }

  private phaseValue(value: FaseSecuencia): number {
    return { apertura: 1, desarrollo: 2, cierre: 3 }[value];
  }

  private agenteLabel(value: number): AgenteEvaluacion {
    return ({
      1: 'Autoevaluación',
      2: 'Coevaluación',
      3: 'Heteroevaluación'
    } as Record<number, AgenteEvaluacion>)[value] ?? 'Heteroevaluación';
  }

  private agentValue(value: AgenteEvaluacion): number | null {
    return ({
      'Autoevaluación': 1,
      'Coevaluación': 2,
      'Heteroevaluación': 3
    } as Record<AgenteEvaluacion, number>)[value] ?? null;
  }

  private tipoEvaluacionLabel(value: number | null): string {
    if (value === null) return '';
    return ({
      1: 'Conceptual', 2: 'Producto', 3: 'Desempeño', 4: 'Ensayo',
      5: 'Estudio de caso', 6: 'Análisis de desempeño', 7: 'Proyecto',
      8: 'Práctica guiada', 9: 'Reporte académico', 10: 'Exposición',
      99: 'Otro'
    } as Record<number, string>)[value] ?? '';
  }

  private tipoEvaluacionValue(value: string): number | null {
    if (!value.trim()) return null;
    return ({
      'Conceptual': 1, 'Producto': 2, 'Desempeño': 3, 'Ensayo': 4,
      'Estudio de caso': 5, 'Análisis de desempeño': 6, 'Proyecto': 7,
      'Práctica guiada': 8, 'Práctica': 8, 'Reporte académico': 9,
      'Exposición': 10, 'Otro': 99
    } as Record<string, number>)[value] ?? null;
  }

  private strategyLabel(phase: FaseSecuencia, value: number): string {
    const entries = this.strategyEntries(phase);
    return entries.find(entry => entry[1] === value)?.[0] ?? '';
  }

  private methodLabel(
    phase: FaseSecuencia,
    method: number | null,
    legacyStrategy: number | null
  ): string {
    if (method === null) {
      return legacyStrategy === null ? '' : this.strategyLabel(phase, legacyStrategy);
    }

    return ({
      1: 'Webquest',
      2: 'Técnica expositiva',
      3: 'Conceptual',
      4: 'Taller',
      5: 'Ensayo',
      6: 'Análisis de desempeño',
      7: 'Estudio de caso',
      8: 'Lluvia de ideas',
      9: 'Cuadro sinóptico',
      10: 'Mapa mental',
      11: 'Mapa conceptual',
      12: 'Cuestionario para reflexionar sobre lo aprendido',
      13: 'Debate',
      14: 'Foro',
      15: 'Panel',
      16: 'Seminario',
      17: 'Mesa redonda',
      18: 'Proyecto de investigación',
      19: 'Aprendizaje basado en problemas',
      20: 'Aprendizaje por proyectos',
      21: 'Aprendizaje cooperativo',
      22: 'Práctica guiada',
      23: 'Prácticas de laboratorio',
      24: 'Investigación',
      25: 'Lectura comentada'
    } as Record<number, string>)[method] ?? '';
  }

  private methodValue(label: string): number | null {
    const methods: Record<string, number> = {
      'Webquest': 1,
      'Técnica expositiva': 2,
      'Conceptual': 3,
      'Taller': 4,
      'Ensayo': 5,
      'Análisis de desempeño': 6,
      'Estudio de caso': 7,
      'Lluvia de ideas': 8,
      'Cuadro sinóptico': 9,
      'Mapa mental': 10,
      'Mapa conceptual': 11,
      'Cuestionario para reflexionar sobre lo aprendido': 12,
      'Debate': 13,
      'Foro': 14,
      'Panel': 15,
      'Seminario': 16,
      'Mesa redonda': 17,
      'Proyecto de investigación': 18,
      'Aprendizaje basado en problemas': 19,
      'Aprendizaje por proyectos': 20,
      'Aprendizaje cooperativo': 21,
      'Práctica guiada': 22,
      'Prácticas de laboratorio': 23,
      'Investigación': 24,
      'Lectura comentada': 25
    };

    return methods[label] ?? null;
  }

  private strategyValue(
    phase: FaseSecuencia,
    label: string
  ): number | undefined {
    return this.strategyEntries(phase)
      .find(entry => entry[0] === label)?.[1];
  }

  private strategyEntries(phase: FaseSecuencia): Array<[string, number]> {
    if (phase === 'desarrollo') return [
      ['Dramatización', 1], ['Estudio de casos', 2], ['Debate', 3],
      ['Foro', 4], ['Panel', 5], ['Simposio', 6], ['Seminario', 7],
      ['Mesa redonda', 8], ['Coloquio', 9], ['Ensayo', 10],
      ['Taller', 11], ['Tutoría de pares', 12], ['Aprendizaje cooperativo', 13],
      ['Aprendizaje basado en problemas', 14], ['Aprendizaje por proyectos', 15],
      ['Simulación', 16], ['Juego de roles', 17], ['Aprendizaje situado', 18],
      ['Prácticas de laboratorio', 19], ['Grupos focales', 20]
    ];

    if (phase === 'cierre') return [
      ['Cuestionario para reflexionar sobre lo aprendido', 1], ['SQA', 2],
      ['Presentación multimedia', 3], ['Presentación de resultados de ABP', 4],
      ['Mapa mental', 6], ['Mapa conceptual', 7], ['Diagrama causa-efecto', 8],
      ['Tabla relacional', 9], ['Esquema', 10], ['Red semántica', 11],
      ['Cuadro sinóptico', 12], ['Cuadro comparativo', 13], ['Ensayo', 14],
      ['Video testimonial', 15], ['Análisis de artículos', 16], ['Debate', 17],
      ['Foro', 18], ['Simposio', 19], ['Seminario', 20], ['Panel', 22],
      ['Mesa redonda', 23], ['Presentación y análisis de reporte de prácticas', 24],
      ['Seguimiento por pares', 25]
    ];

    return [
      ['Preguntas generadoras', 1], ['Preguntas guía', 2],
      ['Preguntas exploratorias', 3], ['SQA', 6],
      ['Identificación de expectativas', 7], ['Lluvia de ideas', 8],
      ['Análisis de artículos', 9], ['Dinámica de presentación', 10],
      ['Analogía', 11], ['Clase magistral', 12], ['Técnica expositiva', 13],
      ['Mapa mental', 14], ['Mapa conceptual', 15], ['Diagrama causa-efecto', 16],
      ['Diagrama de flujo', 17], ['Tabla relacional', 21], ['Esquema', 22],
      ['Red semántica', 23], ['Cuadro sinóptico', 24], ['Cuadro comparativo', 25],
      ['Línea de tiempo', 26], ['Lectura comentada', 38], ['Investigación', 39],
      ['Webquest', 40], ['Presentación multimedia', 41], ['Cuestionario', 42],
      ['Resumen', 52], ['Demostración', 56], ['Tutoría de pares', 57],
      ['Ejercicios escritos', 59], ['Debate', 68], ['QQQ', 70],
      ['Síntesis', 71], ['Práctica guiada', 74], ['Práctica semiguiada', 75]
    ];
  }

  private tipoCompetencia(value: string | null): PlaneacionDetail['programa']['tipoCompetencia'] {
    return value === 'Base' || value === 'Transversal' || value === 'Específica'
      ? value
      : '';
  }

  private modalidad(value: string | null): PlaneacionDetail['programa']['modalidad'] {
    return value === 'Escolarizada' || value === 'Mixta' ||
      value === 'Dual' || value === 'No escolarizada'
      ? value
      : '';
  }

  private nullableText(value: string | null | undefined): string | null {
    const normalized = value?.trim() ?? '';
    return normalized || null;
  }

  private nullableNumber(value: string | number, label: string): number | null {
    if (value === '' || value === null || value === undefined) return null;
    const number = Number(value);
    if (!Number.isFinite(number)) throw new Error(`El campo ${label} debe ser numérico.`);
    return number;
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

  private apiUrl(path: string): string {
    return `${environment.apiUrl.replace(/\/$/, '')}/${path.replace(/^\//, '')}`;
  }

  getSeguimientoDirectivo(): Observable<SeguimientoPlaneacion[]> {
    return throwError(() => new Error(
      '[FALTA ENDPOINT] La API solo expone el resumen de seguimiento, no el listado con fechas y estados por planeación.'
    ));
  }
}
