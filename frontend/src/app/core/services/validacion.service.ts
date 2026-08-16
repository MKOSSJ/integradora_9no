import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

import { RevisionDetail, RevisionItem } from '../models/validacion.model';

@Injectable({
  providedIn: 'root',
})
export class ValidacionService {
  private readonly revisions: RevisionDetail[] = [
    {
      id: 1,
      titulo: 'Física Avanzada — Mecánica',
      descripcion: 'Secuencia didáctica enviada para revisión académica.',
      actualizacion: '2024-03-14',
      progreso: 100,
      status: 'pendiente',
      reviewStatus: 'pendiente',

      autor: 'Carlos Pérez',
      enviadoPor: 'Carlos Pérez',
      carrera: 'Ingeniería Industrial',
      grupo: 'IND-401',

      fechaCreacion: '2024-03-10',
      ultimaModificacion: '2024-03-14',
      fechaEnvio: '2024-03-14',
      fechaEnvioRevision: '2024-03-14',
      pdfPages: 5,

      comentariosRevision: [
        'Revisar que las evidencias de cierre correspondan al resultado de aprendizaje.',
        'La estructura general de la secuencia es clara.',
      ],

      programa: {
        id: 1,
        nombre: 'Física Avanzada',
        clave: 'FIS-401',
        programaEducativo: 'Ingeniería Industrial',
        cuatrimestre: 'Cuarto cuatrimestre',
        creditos: 6,
        horasTotales: 90,
        horasSaber: 40,
        horasSaberHacer: 50,
        horasSemana: 6,
        proposito: 'Analizar fenómenos físicos relacionados con la mecánica clásica.',
        competencia: 'Resolver problemas físicos mediante modelos matemáticos.',
        tipoCompetencia: 'Específica',
        modalidad: 'Escolarizada',
        referenciasBase: [],
      },

      formulario: {
        titulo: 'Física Avanzada — Mecánica',

        periodoId: 1,
        asignaturaId: 1,
        academiaId: 1,
        programaAsignaturaId: 1,
        revisorId: 2,

        docenteIds: [1],
        grupoIds: [1],

        caratula: {
          programaEducativo: 'Ingeniería Industrial',
          docentes: 'Carlos Pérez',
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

        unidades: [],
      },
    },
    {
      id: 2,
      titulo: 'Historia Universal — Edad Media',
      descripcion: 'Secuencia actualmente en revisión.',
      actualizacion: '2024-03-12',
      progreso: 100,
      status: 'revision',
      reviewStatus: 'revision',

      autor: 'María González',
      enviadoPor: 'María González',
      carrera: 'Educación',
      grupo: 'EDU-202',

      fechaCreacion: '2024-03-08',
      ultimaModificacion: '2024-03-12',
      fechaEnvio: '2024-03-12',
      fechaEnvioRevision: '2024-03-12',
      pdfPages: 4,

      comentariosRevision: [],

      programa: {
        id: 2,
        nombre: 'Historia Universal',
        clave: 'HIS-204',
        programaEducativo: 'Educación',
        cuatrimestre: 'Segundo cuatrimestre',
        creditos: 5,
        horasTotales: 75,
        horasSaber: 35,
        horasSaberHacer: 40,
        horasSemana: 5,
        proposito: 'Comprender procesos históricos relevantes y su impacto social.',
        competencia: 'Analizar hechos históricos mediante pensamiento crítico.',
        tipoCompetencia: 'Base',
        modalidad: 'Escolarizada',
        referenciasBase: [],
      },

      formulario: {
        titulo: 'Historia Universal — Edad Media',

        periodoId: 1,
        asignaturaId: 2,
        academiaId: 2,
        programaAsignaturaId: 2,
        revisorId: 2,

        docenteIds: [2],
        grupoIds: [2],

        caratula: {
          programaEducativo: 'Educación',
          docentes: 'María González',
          cuatrimestre: 'Segundo cuatrimestre',
          periodoEscolar: 'Enero - Abril 2024',
          asignatura: 'Historia Universal',
          grupos: 'EDU-202',
          propositoAsignatura: 'Comprender procesos históricos relevantes y su impacto social.',
          competenciaContribuye: 'Analizar hechos históricos mediante pensamiento crítico.',
          tipoCompetencia: 'Base',
          creditos: 5,
          modalidad: 'Escolarizada',
          horasSaber: 35,
          horasSaberHacer: 40,
          horasTotales: 75,
          horasSemana: 5,
        },

        unidades: [],
      },
    },
    {
      id: 3,
      titulo: 'Química Orgánica — Hidrocarburos',
      descripcion: 'Secuencia aprobada por el revisor.',
      actualizacion: '2024-03-11',
      progreso: 100,
      status: 'aprobado',
      reviewStatus: 'aprobado',

      autor: 'Carlos Pérez',
      enviadoPor: 'Carlos Pérez',
      carrera: 'Ingeniería Química',
      grupo: 'QUI-302',

      fechaCreacion: '2024-03-09',
      ultimaModificacion: '2024-03-11',
      fechaEnvio: '2024-03-11',
      fechaEnvioRevision: '2024-03-11',
      fechaValidacion: '2024-03-12',
      pdfPages: 5,

      comentariosRevision: [
        'Secuencia aprobada. Cumple con los criterios de revisión.',
      ],

      programa: {
        id: 3,
        nombre: 'Química Orgánica',
        clave: 'QUI-302',
        programaEducativo: 'Ingeniería Química',
        cuatrimestre: 'Tercer cuatrimestre',
        creditos: 6,
        horasTotales: 90,
        horasSaber: 40,
        horasSaberHacer: 50,
        horasSemana: 6,
        proposito: 'Estudiar compuestos orgánicos y sus propiedades.',
        competencia: 'Analizar estructuras químicas orgánicas.',
        tipoCompetencia: 'Específica',
        modalidad: 'Escolarizada',
        referenciasBase: [],
      },

      formulario: {
        titulo: 'Química Orgánica — Hidrocarburos',

        periodoId: 1,
        asignaturaId: 3,
        academiaId: 3,
        programaAsignaturaId: 3,
        revisorId: 2,

        docenteIds: [1],
        grupoIds: [3],

        caratula: {
          programaEducativo: 'Ingeniería Química',
          docentes: 'Carlos Pérez',
          cuatrimestre: 'Tercer cuatrimestre',
          periodoEscolar: 'Enero - Abril 2024',
          asignatura: 'Química Orgánica',
          grupos: 'QUI-302',
          propositoAsignatura: 'Estudiar compuestos orgánicos y sus propiedades.',
          competenciaContribuye: 'Analizar estructuras químicas orgánicas.',
          tipoCompetencia: 'Específica',
          creditos: 6,
          modalidad: 'Escolarizada',
          horasSaber: 40,
          horasSaberHacer: 50,
          horasTotales: 90,
          horasSemana: 6,
        },

        unidades: [],
      },
    },
    {
      id: 4,
      titulo: 'Biología — Genética Mendeliana',
      descripcion: 'Secuencia con correcciones solicitadas.',
      actualizacion: '2024-03-10',
      progreso: 80,
      status: 'correcciones',
      reviewStatus: 'correcciones',

      autor: 'Ana López',
      enviadoPor: 'Ana López',
      carrera: 'Biotecnología',
      grupo: 'BIO-201',

      fechaCreacion: '2024-03-07',
      ultimaModificacion: '2024-03-10',
      fechaEnvio: '2024-03-10',
      fechaEnvioRevision: '2024-03-10',
      pdfPages: 3,

      comentariosRevision: [
        'Agregar mayor claridad en los instrumentos de evaluación.',
        'Revisar la ponderación de evidencias.',
      ],

      programa: {
        id: 4,
        nombre: 'Biología',
        clave: 'BIO-201',
        programaEducativo: 'Biotecnología',
        cuatrimestre: 'Segundo cuatrimestre',
        creditos: 5,
        horasTotales: 75,
        horasSaber: 35,
        horasSaberHacer: 40,
        horasSemana: 5,
        proposito: 'Comprender los principios básicos de la genética mendeliana.',
        competencia: 'Interpretar patrones de herencia genética.',
        tipoCompetencia: 'Específica',
        modalidad: 'Escolarizada',
        referenciasBase: [],
      },

      formulario: {
        titulo: 'Biología — Genética Mendeliana',

        periodoId: 1,
        asignaturaId: 4,
        academiaId: 4,
        programaAsignaturaId: 4,
        revisorId: 2,

        docenteIds: [4],
        grupoIds: [4],

        caratula: {
          programaEducativo: 'Biotecnología',
          docentes: 'Ana López',
          cuatrimestre: 'Segundo cuatrimestre',
          periodoEscolar: 'Enero - Abril 2024',
          asignatura: 'Biología',
          grupos: 'BIO-201',
          propositoAsignatura: 'Comprender los principios básicos de la genética mendeliana.',
          competenciaContribuye: 'Interpretar patrones de herencia genética.',
          tipoCompetencia: 'Específica',
          creditos: 5,
          modalidad: 'Escolarizada',
          horasSaber: 35,
          horasSaberHacer: 40,
          horasTotales: 75,
          horasSemana: 5,
        },

        unidades: [],
      },
    },
  ];

  getRevisions(): Observable<RevisionItem[]> {
    return of(
      this.revisions
        .filter((item) => item.reviewStatus !== 'borrador')
        .map((item) => ({
          id: item.id,
          titulo: item.titulo,
          autor: item.autor,
          estado: item.reviewStatus,
          fechaEnvio: item.fechaEnvio,
          carrera: item.carrera,
          grupo: item.grupo,
        })),
    );
  }

  getRevisionById(id: number): Observable<RevisionDetail | undefined> {
    return of(this.revisions.find((item) => item.id === id));
  }

  startRevision(id: number): Observable<boolean> {
    const revision = this.revisions.find((item) => item.id === id);

    if (revision && revision.reviewStatus === 'pendiente') {
      revision.reviewStatus = 'revision';
      revision.status = 'revision';
      revision.ultimaModificacion = new Date().toISOString();
      revision.actualizacion = new Date().toISOString();
    }

    return of(true);
  }

  approveRevision(id: number): Observable<boolean> {
    const revision = this.revisions.find((item) => item.id === id);

    if (revision && this.canReviewerEdit(revision.reviewStatus)) {
      revision.reviewStatus = 'aprobado';
      revision.status = 'aprobado';
      revision.fechaValidacion = new Date().toISOString();
      revision.ultimaModificacion = new Date().toISOString();
      revision.actualizacion = new Date().toISOString();
    }

    return of(true);
  }

  requestCorrections(id: number): Observable<boolean> {
    const revision = this.revisions.find((item) => item.id === id);

    if (revision && this.canReviewerEdit(revision.reviewStatus)) {
      revision.reviewStatus = 'correcciones';
      revision.status = 'correcciones';
      revision.ultimaModificacion = new Date().toISOString();
      revision.actualizacion = new Date().toISOString();
    }

    return of(true);
  }

  addComment(id: number, comment: string): Observable<boolean> {
    const revision = this.revisions.find((item) => item.id === id);

    if (revision && this.canReviewerEdit(revision.reviewStatus) && comment.trim()) {
      revision.comentariosRevision.push(comment.trim());
      revision.ultimaModificacion = new Date().toISOString();
      revision.actualizacion = new Date().toISOString();
    }

    return of(true);
  }

  private canReviewerEdit(status: string): boolean {
    return status === 'pendiente' || status === 'revision';
  }
}