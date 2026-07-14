import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

import {
  MomentoDidactico,
  PlaneacionDetail,
  PlaneacionListItem,
  UnidadPlaneacion,
} from '../models/planeacion.model';

@Injectable({
  providedIn: 'root',
})
export class PlaneacionesService {
  private readonly planeaciones: PlaneacionDetail[] = [
    {
      id: 1,
      titulo: 'Matemáticas Básicas — Álgebra',
      descripcion:
        'Secuencia didáctica enfocada en conceptos fundamentales de álgebra para estudiantes de secundaria.',
      actualizacion: '2024-03-14',
      progreso: 100,
      status: 'borrador',
      autor: 'Carlos Pérez',
      fechaCreacion: '2024-03-15',
      ultimaModificacion: '2024-03-21',
      pdfPages: 5,
      programa: {
        nombre: 'Desarrollo Humano y Valores',
        clave: 'DHV-301',
        programaEducativo: 'Ingeniería en Tecnologías de la Información e Innovación Digital',
        cuatrimestre: 'Tercer cuatrimestre',
        creditos: 5,
        horasTotales: 75,
        proposito:
          'El estudiante desarrollará habilidades personales, sociales y profesionales mediante actividades reflexivas y colaborativas.',
      },
      formulario: {
        caratula: {
          programaEducativo: 'Ingeniería en Tecnologías de la Información e Innovación Digital',
          docentes: 'Carlos Pérez',
          cuatrimestre: 'Tercer cuatrimestre',
          periodoEscolar: 'Enero - Abril 2024',
          asignatura: 'Desarrollo Humano y Valores',
          grupos: 'TI-301',
          propositoAsignatura:
            'El estudiante desarrollará habilidades personales, sociales y profesionales mediante actividades reflexivas y colaborativas.',
          competenciaContribuye:
            'Desarrollar habilidades de comunicación, liderazgo y trabajo colaborativo.',
          tipoCompetencia: 'Transversal',
          creditos: 5,
          modalidad: 'Escolarizada',
          horasSaber: 30,
          horasSaberHacer: 45,
          horasTotales: 75,
          horasSemana: 5,
        },
        unidades: [
          {
            id: 1,
            nombre: 'Unidad I — Fundamentos del desarrollo humano',
            propositoEsperado:
              'Identificar los elementos principales del desarrollo humano y su aplicación en contextos académicos.',
            horasSaber: 10,
            horasSaberHacer: 15,
            horasTotales: 25,
            porcentajeUnidad: 33,
            duracionSemanas: 5,
            resultadoAprendizaje:
              'El estudiante identifica los fundamentos del desarrollo humano y los relaciona con su formación académica y profesional.',
            temas: [
              {
                id: 1,
                tema: 'Dimensiones del desarrollo humano',
                saber: 'Conceptos básicos del desarrollo humano.',
                saberHacer:
                  'Identificar dimensiones personales y sociales en situaciones académicas.',
                saberSerConvivir: 'Responsabilidad, empatía y respeto.',
              },
            ],
            evaluaciones: [
              {
                id: 1,
                evidenciaAprendizaje:
                  'Mapa conceptual sobre las dimensiones del desarrollo humano.',
                tipoEvaluacion: 'Heteroevaluación',
                ponderacion: 40,
                instrumentoEvaluacion: 'Rúbrica',
              },
              {
                id: 2,
                evidenciaAprendizaje:
                  'Reflexión escrita sobre la aplicación del desarrollo humano en la vida académica.',
                tipoEvaluacion: 'Autoevaluación',
                ponderacion: 60,
                instrumentoEvaluacion: 'Lista de cotejo',
              },
            ],
            apertura: {
              metodosTecnicas: 'Lluvia de ideas y preguntas generadoras.',
              actividadesDocentes:
                'El docente realiza preguntas iniciales para activar conocimientos previos y contextualizar el tema.',
              actividadesEstudiantes:
                'Los estudiantes participan compartiendo ideas, experiencias y conceptos previos.',
              evidenciaAprendizaje: 'Participación inicial y registro de ideas principales.',
              recursos: 'Pizarrón, marcadores y presentación multimedia.',
            },
            desarrollo: {
              metodosTecnicas: 'Aprendizaje cooperativo y análisis de casos.',
              actividadesDocentes:
                'El docente guía el análisis de casos y retroalimenta el trabajo de los equipos.',
              actividadesEstudiantes:
                'Los estudiantes analizan casos en equipos, identifican dimensiones del desarrollo humano y elaboran conclusiones.',
              evidenciaAprendizaje: 'Reporte de análisis de caso.',
              recursos: 'Caso impreso, computadora, proyector y rúbrica.',
            },
            cierre: {
              metodosTecnicas: 'Reflexión guiada y presentación de resultados.',
              actividadesDocentes:
                'El docente dirige una reflexión final sobre lo aprendido y orienta la retroalimentación.',
              actividadesEstudiantes:
                'Los estudiantes presentan conclusiones y proponen acciones de mejora personal.',
              evidenciaAprendizaje: 'Conclusión escrita individual.',
              recursos: 'Formato de reflexión y lista de cotejo.',
            },
            referencias:
              'Universidad Tecnológica de Huejotzingo. Programa de asignatura. Recursos digitales institucionales.',
          },
          this.createUnidadMock(2, 'Unidad II — Comunicación efectiva'),
        ],
      },
    },
    {
      id: 2,
      titulo: 'Física Avanzada — Mecánica',
      descripcion:
        'Introducción a los principios de la mecánica clásica y sus aplicaciones prácticas.',
      actualizacion: '2024-03-13',
      progreso: 68,
      status: 'revision',
      autor: 'María López',
      fechaCreacion: '2024-03-10',
      ultimaModificacion: '2024-03-13',
      pdfPages: 4,
      programa: {
        nombre: 'Física Avanzada',
        clave: 'FIS-401',
        programaEducativo: 'Ingeniería Industrial',
        cuatrimestre: 'Cuarto cuatrimestre',
        creditos: 6,
        horasTotales: 90,
        proposito: 'Analizar fenómenos físicos relacionados con la mecánica clásica.',
      },
      formulario: {
        caratula: {
          programaEducativo: 'Ingeniería Industrial',
          docentes: 'María López',
          cuatrimestre: 'Cuarto cuatrimestre',
          periodoEscolar: 'Enero - Abril 2024',
          asignatura: 'Física Avanzada',
          grupos: 'IND-401',
          propositoAsignatura: 'Analizar fenómenos físicos relacionados con la mecánica clásica.',
          competenciaContribuye: 'Resolver problemas físicos mediante modelos matemáticos.',
          tipoCompetencia: 'Específica',
          creditos: 6,
          modalidad: 'Escolarizada',
          horasSaber: 40,
          horasSaberHacer: 50,
          horasTotales: 90,
          horasSemana: 6,
        },
        unidades: [this.createUnidadMock(1, 'Unidad I — Cinemática')],
      },
    },
  ];

  getPlaneaciones(): Observable<PlaneacionListItem[]> {
    return of(this.planeaciones);
  }

  getPlaneacionById(id: number): Observable<PlaneacionDetail | undefined> {
    return of(this.planeaciones.find((item) => item.id === id));
  }

  saveDraft(id: number): Observable<boolean> {
    console.log('Guardar borrador', id);
    return of(true);
  }

  submitForApproval(id: number): Observable<boolean> {
    console.log('Enviar para aprobación', id);
    return of(true);
  }

  private createUnidadMock(id: number, nombre: string): UnidadPlaneacion {
    return {
      id,
      nombre,
      propositoEsperado: '',
      horasSaber: 0,
      horasSaberHacer: 0,
      horasTotales: 0,
      porcentajeUnidad: 0,
      duracionSemanas: 0,
      resultadoAprendizaje: '',
      temas: [
        {
          id: id * 100 + 1,
          tema: '',
          saber: '',
          saberHacer: '',
          saberSerConvivir: '',
        },
      ],
      evaluaciones: [
        {
          id: id * 100 + 2,
          evidenciaAprendizaje: '',
          tipoEvaluacion: '',
          ponderacion: 0,
          instrumentoEvaluacion: '',
        },
      ],
      apertura: this.emptyMoment(),
      desarrollo: this.emptyMoment(),
      cierre: this.emptyMoment(),
      referencias: '',
    };
  }

  private emptyMoment(): MomentoDidactico {
    return {
      metodosTecnicas: '',
      actividadesDocentes: '',
      actividadesEstudiantes: '',
      evidenciaAprendizaje: '',
      recursos: '',
    };
  }
}
