import { NgClass } from '@angular/common';
import { Component, EventEmitter, Input, Output, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  LucideDynamicIcon,
  LucideBookOpen,
  LucideChevronLeft,
  LucideChevronRight,
  LucideCirclePlus,
  LucideTrash2,
  LucideClipboardCheck,
  LucideLayers3,
  LucideLibrary,
  LucideFileText
} from '@lucide/angular';

import {
  ActividadSecuencia,
  AgenteEvaluacion,
  CategoriaInstrumento,
  EvaluacionUnidad,
  FaseSecuencia,
  FormSection,
  InstrumentoEvaluacion,
  PlaneacionDetail,
  PlaneacionTutorial,
  ReferenciaBibliografica,
  TipoEvaluacionFase,
  TipoReferencia,
  UnidadPlaneacion
} from '../../../../core/models/planeacion.model';

type CaratulaKey =
  | 'programaEducativo'
  | 'docentes'
  | 'cuatrimestre'
  | 'periodoEscolar'
  | 'asignatura'
  | 'grupos'
  | 'propositoAsignatura'
  | 'competenciaContribuye'
  | 'tipoCompetencia'
  | 'creditos'
  | 'modalidad'
  | 'horasSaber'
  | 'horasSaberHacer'
  | 'horasTotales'
  | 'horasSemana';

type UnidadKey =
  | 'nombre'
  | 'propositoEsperado'
  | 'horasSaber'
  | 'horasSaberHacer'
  | 'horasTotales'
  | 'porcentajeUnidad'
  | 'periodoSemanas'
  | 'resultadoAprendizaje';

@Component({
  selector: 'app-planeacion-form',
  standalone: true,
  imports: [
    NgClass,
    FormsModule,
    LucideDynamicIcon
  ],
  templateUrl: './planeacion-form.html',
  styleUrl: './planeacion-form.css'
})
export class PlaneacionForm {
  @Input({ required: true }) planeacion!: PlaneacionDetail<string | number>;
  @Output() tutorialChange = new EventEmitter<PlaneacionTutorial>();

  selectedUnitIndex = signal(0);
  activeSection = signal<FormSection>('unidad');
  caratulaSelected = signal(true);

  caratulaIcon = LucideFileText;
  unidadIcon = LucideBookOpen;
  evaluacionIcon = LucideClipboardCheck;
  secuenciaIcon = LucideLayers3;
  referenciasIcon = LucideLibrary;

  plusIcon = LucideCirclePlus;
  trashIcon = LucideTrash2;
  prevIcon = LucideChevronLeft;
  nextIcon = LucideChevronRight;

  form = computed(() => this.planeacion?.formulario ?? null);

  selectedUnit = computed(() => {
    const currentForm = this.form();

    if (!currentForm) return null;

    return currentForm.unidades[this.selectedUnitIndex()] ?? null;
  });

  sections: {
    id: FormSection;
    label: string;
    icon: any;
  }[] = [
    {
      id: 'unidad',
      label: 'Unidad',
      icon: LucideBookOpen
    },
    {
      id: 'evaluacion',
      label: 'Evaluación',
      icon: LucideClipboardCheck
    },
    {
      id: 'secuencia',
      label: 'Secuencia',
      icon: LucideLayers3
    },
    {
      id: 'referencias',
      label: 'Referencias',
      icon: LucideLibrary
    }
  ];

  moments: {
    key: FaseSecuencia;
    label: string;
    help: string;
  }[] = [
    {
      key: 'apertura',
      label: 'Apertura',
      help: 'Diagnóstico, motivación, activación de conocimientos previos e inicio de la práctica guiada.'
    },
    {
      key: 'desarrollo',
      label: 'Desarrollo',
      help: 'Consolidación de habilidades mediante actividades, casos, prácticas, proyectos o problemas.'
    },
    {
      key: 'cierre',
      label: 'Cierre',
      help: 'Reflexión, autonomía, transferencia y cierre del resultado de aprendizaje.'
    }
  ];

  caratulaFields: {
    key: CaratulaKey;
    label: string;
    type: 'text' | 'number' | 'textarea' | 'select';
    help: string;
    span?: string;
    options?: string[];
  }[] = [
    {
      key: 'programaEducativo',
      label: 'Programa educativo',
      type: 'text',
      help: 'Nombre del programa educativo.',
      span: 'md:col-span-2'
    },
    {
      key: 'docentes',
      label: 'Docente(s)',
      type: 'textarea',
      help: 'Nombre de los docentes o academia que elaboran la planeación.',
      span: 'md:col-span-2'
    },
    {
      key: 'cuatrimestre',
      label: 'Cuatrimestre',
      type: 'text',
      help: 'Grado en el que se imparte la asignatura.'
    },
    {
      key: 'periodoEscolar',
      label: 'Periodo escolar',
      type: 'text',
      help: 'Periodo cuatrimestral y año.'
    },
    {
      key: 'asignatura',
      label: 'Nombre de la asignatura',
      type: 'text',
      help: 'Debe coincidir con el programa de asignatura.'
    },
    {
      key: 'grupos',
      label: 'Grupo(s)',
      type: 'text',
      help: 'Grupos en los que se impartirá la asignatura.'
    },
    {
      key: 'propositoAsignatura',
      label: 'Propósito de la asignatura',
      type: 'textarea',
      help: 'Debe tomarse del programa de asignatura sin modificaciones.',
      span: 'md:col-span-2'
    },
    {
      key: 'competenciaContribuye',
      label: 'Competencia a la que contribuye',
      type: 'textarea',
      help: 'Debe tomarse del programa de asignatura sin modificaciones.',
      span: 'md:col-span-2'
    },
    {
      key: 'tipoCompetencia',
      label: 'Tipo de competencia',
      type: 'select',
      help: 'Base, transversal o específica.',
      options: ['Base', 'Transversal', 'Específica']
    },
    {
      key: 'modalidad',
      label: 'Modalidad',
      type: 'select',
      help: 'Modalidad en la que se imparte el programa.',
      options: ['Escolarizada', 'Mixta', 'Dual', 'No escolarizada']
    },
    {
      key: 'creditos',
      label: 'Créditos',
      type: 'number',
      help: 'Número de créditos de la asignatura.'
    },
    {
      key: 'horasSemana',
      label: 'Horas por semana',
      type: 'number',
      help: 'Horas semanales asignadas.'
    },
    {
      key: 'horasSaber',
      label: 'Horas del saber',
      type: 'number',
      help: 'Horas del saber indicadas en el programa.'
    },
    {
      key: 'horasSaberHacer',
      label: 'Horas del saber hacer',
      type: 'number',
      help: 'Horas del saber hacer indicadas en el programa.'
    },
    {
      key: 'horasTotales',
      label: 'Horas totales',
      type: 'number',
      help: 'Total de horas de la asignatura.'
    }
  ];

  unidadFields: {
    key: UnidadKey;
    label: string;
    type: 'text' | 'number' | 'textarea';
    help: string;
    span?: string;
  }[] = [
    {
      key: 'nombre',
      label: 'Nombre de la unidad de aprendizaje',
      type: 'text',
      help: 'Debe escribirse como aparece en el programa.',
      span: 'md:col-span-2'
    },
    {
      key: 'propositoEsperado',
      label: 'Propósito esperado',
      type: 'textarea',
      help: 'Debe copiarse del programa de asignatura.',
      span: 'md:col-span-2'
    },
    {
      key: 'horasSaber',
      label: 'Horas del saber',
      type: 'number',
      help: 'Horas del saber por unidad.'
    },
    {
      key: 'horasSaberHacer',
      label: 'Horas del saber hacer',
      type: 'number',
      help: 'Horas del saber hacer por unidad.'
    },
    {
      key: 'horasTotales',
      label: 'Horas totales',
      type: 'number',
      help: 'Suma de horas del saber y saber hacer.'
    },
    {
      key: 'porcentajeUnidad',
      label: 'Porcentaje de la unidad',
      type: 'number',
      help: 'Porcentaje de la unidad respecto al total de la asignatura.'
    }
  ];

  faseOptions: {
    value: FaseSecuencia;
    label: string;
  }[] = [
    { value: 'apertura', label: 'Apertura' },
    { value: 'desarrollo', label: 'Desarrollo' },
    { value: 'cierre', label: 'Cierre' }
  ];

  agenteOptions: AgenteEvaluacion[] = [
    'Autoevaluación',
    'Coevaluación',
    'Heteroevaluación'
  ];

  categoriaInstrumentoOptions: CategoriaInstrumento[] = [
    'Conocimiento',
    'Producto',
    'Desempeño'
  ];

  tipoReferenciaOptions: TipoReferencia[] = [
    'Libro',
    'Artículo',
    'Sitio web',
    'Video',
    'Documento',
    'Otro'
  ];

  evaluationOptions = [
    'Conceptual',
    'Producto',
    'Desempeño',
    'Ensayo',
    'Estudio de caso',
    'Análisis de desempeño',
    'Reporte académico',
    'Proyecto',
    'Práctica guiada',
    'Exposición',
    'Otro',
    'Práctica semiguiada',
    'Ejercicios',
    'Prueba'
  ];

  instrumentOptions = [
    'Prueba de opción múltiple',
    'Prueba objetiva',
    'Prueba por competencias',
    'Lista de cotejo',
    'Guía de observación',
    'Escala estimativa',
    'Rúbrica',
    'Cuestionario de preguntas abiertas',
    'Cuestionario de reactivos objetivos'
  ];

  methodsByMoment: Record<FaseSecuencia, string[]> = {
    apertura: [
      'Preguntas generadoras',
      'Preguntas guía',
      'Preguntas exploratorias',
      'SQA',
      'Identificación de expectativas',
      'Lluvia de ideas',
      'Análisis de artículos',
      'Análisis de documentos',
      'Análisis de noticias',
      'Dinámica de presentación',
      'Analogía',
      'Clase magistral',
      'Técnica expositiva',
      'Mapa mental',
      'Mapa conceptual',
      'Diagrama causa-efecto',
      'Diagrama de flujo',
      'Tabla relacional',
      'Esquema',
      'Red semántica',
      'Cuadro sinóptico',
      'Cuadro comparativo',
      'Línea de tiempo',
      'Lectura comentada',
      'Investigación',
      'Webquest',
      'Presentación multimedia',
      'Cuestionario',
      'Resumen',
      'Demostración',
      'Tutoría de pares',
      'Ejercicios escritos',
      'Debate',
      'QQQ',
      'Síntesis',
      'Práctica guiada',
      'Práctica semiguiada'
    ],
    desarrollo: [
      'Dramatización',
      'Estudio de casos',
      'Debate',
      'Foro',
      'Panel',
      'Simposio',
      'Seminario',
      'Mesa redonda',
      'Coloquio',
      'Ensayo',
      'Taller',
      'Tutoría de pares',
      'Aprendizaje cooperativo',
      'Aprendizaje basado en problemas',
      'Aprendizaje por proyectos',
      'Simulación',
      'Juego de roles',
      'Aprendizaje situado',
      'Prácticas de laboratorio',
      'Grupos focales'
    ],
    cierre: [
      'Cuestionario para reflexionar sobre lo aprendido',
      'SQA',
      'Presentación multimedia',
      'Presentación de resultados de ABP',
      'Presentación de resultados de APP',
      'Presentación de resultados de estudio de caso',
      'Mapa mental',
      'Mapa conceptual',
      'Diagrama causa-efecto',
      'Tabla relacional',
      'Esquema',
      'Red semántica',
      'Cuadro sinóptico',
      'Cuadro comparativo',
      'Ensayo',
      'Video testimonial',
      'Análisis de artículos',
      'Debate',
      'Foro',
      'Simposio',
      'Seminario',
      'Panel',
      'Mesa redonda',
      'Presentación y análisis de reporte de prácticas',
      'Seguimiento por pares',
      'Análisis de desempeño'
    ]
  };

  completionPercentage(): number {
    const currentForm = this.form();

    if (!currentForm) return 0;

    const units = currentForm.unidades;

    if (units.length === 0) return 0;

    let completed = 0;
    let total = 0;

    units.forEach(unit => {
      total += 4;

      if (unit.nombre && unit.propositoEsperado) completed++;
      if (unit.resultadoAprendizaje && unit.evaluaciones.length > 0) completed++;
      if (
        unit.apertura.length > 0 &&
        unit.desarrollo.length > 0 &&
        unit.cierre.length > 0
      ) completed++;
      if (unit.referencias.length > 0) completed++;
    });

    return Math.round((completed / total) * 100);
  }

  isCaratula(): boolean {
    return this.caratulaSelected();
  }

  selectCaratula(): void {
    this.caratulaSelected.set(true);

    this.tutorialChange.emit({
      title: 'Carátula',
      text: 'La carátula identifica la planeación. Puede venir precargada desde el programa, la carga académica y los grupos asignados.',
      options: [
        'Revisa que programa educativo, asignatura, docentes y grupos sean correctos.',
        'El propósito y la competencia deben conservarse como aparecen en el programa.',
        'El revisor sí podrá consultar esta información.'
      ]
    });
  }

  selectUnit(index: number): void {
    this.caratulaSelected.set(false);
    this.selectedUnitIndex.set(index);
    this.activeSection.set('unidad');
    this.emitSectionTutorial();
  }

  setSection(section: FormSection): void {
    this.activeSection.set(section);
    this.emitSectionTutorial();
  }

  getCaratulaValue(key: CaratulaKey): string | number {
    return this.planeacion.formulario.caratula[key];
  }

  updateCaratulaField(key: CaratulaKey, value: string | number): void {
    const caratula = this.planeacion.formulario.caratula;

    if (
      key === 'creditos' ||
      key === 'horasSaber' ||
      key === 'horasSaberHacer' ||
      key === 'horasTotales' ||
      key === 'horasSemana'
    ) {
      (caratula[key] as number) = Number(value);
      return;
    }

    (caratula[key] as string) = String(value);
  }

  getUnitValue(key: UnidadKey): string | number {
    const unit = this.selectedUnit();

    if (!unit) return '';

    return unit[key];
  }

  updateUnitField(key: UnidadKey, value: string | number): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    if (
      key === 'horasSaber' ||
      key === 'horasSaberHacer' ||
      key === 'horasTotales' ||
      key === 'porcentajeUnidad' ||
      key === 'periodoSemanas'
    ) {
      (unit[key] as number) = Number(value);
      return;
    }

    (unit[key] as string) = String(value);
  }

  addTopic(): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    unit.temas.push({
      id: Date.now(),
      tema: '',
      saber: '',
      saberHacer: '',
      saberSerConvivir: ''
    });
  }

  updateTopicField(
    topicIndex: number,
    key: 'tema' | 'saber' | 'saberHacer' | 'saberSerConvivir',
    value: string
  ): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    unit.temas[topicIndex][key] = value;
  }

  addEvaluation(): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    unit.evaluaciones.push({
      id: Date.now(),
      evidenciaAprendizaje: '',
      ponderacion: 100,
      tiposEvaluacion: [
        {
          id: Date.now() + 1,
          fase: 'apertura',
          tipoEvaluacion: 'Conceptual',
          agenteEvaluacion: 'Heteroevaluación'
        },
        {
          id: Date.now() + 2,
          fase: 'desarrollo',
          tipoEvaluacion: '',
          agenteEvaluacion: 'Heteroevaluación'
        },
        {
          id: Date.now() + 3,
          fase: 'cierre',
          tipoEvaluacion: 'Análisis de desempeño',
          agenteEvaluacion: 'Autoevaluación'
        }
      ],
      instrumentos: []
    });
  }

  removeEvaluation(evalIndex: number): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    unit.evaluaciones.splice(evalIndex, 1);
  }

  updateEvaluationField(
    evalIndex: number,
    key: 'evidenciaAprendizaje' | 'ponderacion',
    value: string | number
  ): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    if (key === 'ponderacion') {
      unit.evaluaciones[evalIndex].ponderacion = Number(value);
      return;
    }

    unit.evaluaciones[evalIndex][key] = String(value);
  }

  addEvaluationType(evalIndex: number): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    unit.evaluaciones[evalIndex].tiposEvaluacion.push({
      id: Date.now(),
      fase: 'apertura',
      tipoEvaluacion: '',
      agenteEvaluacion: 'Heteroevaluación'
    });
  }

  removeEvaluationType(evalIndex: number, typeIndex: number): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    unit.evaluaciones[evalIndex].tiposEvaluacion.splice(typeIndex, 1);
  }

  updateEvaluationTypeField(
    evalIndex: number,
    typeIndex: number,
    key: keyof TipoEvaluacionFase,
    value: string
  ): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    const type = unit.evaluaciones[evalIndex].tiposEvaluacion[typeIndex];

    if (key === 'id') return;

    if (key === 'fase') {
      type.fase = value as FaseSecuencia;
      return;
    }

    if (key === 'agenteEvaluacion') {
      type.agenteEvaluacion = value as AgenteEvaluacion;
      return;
    }

    type.tipoEvaluacion = value;
  }

  addInstrument(evalIndex: number): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    unit.evaluaciones[evalIndex].instrumentos.push({
      id: Date.now(),
      categoria: 'Producto',
      nombre: '',
      instrumento: '',
      ponderacion: 0
    });
  }

  removeInstrument(evalIndex: number, instrumentIndex: number): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    unit.evaluaciones[evalIndex].instrumentos.splice(instrumentIndex, 1);
  }

  updateInstrumentField(
    evalIndex: number,
    instrumentIndex: number,
    key: keyof InstrumentoEvaluacion,
    value: string | number
  ): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    const instrument = unit.evaluaciones[evalIndex].instrumentos[instrumentIndex];

    if (key === 'id') return;

    if (key === 'ponderacion') {
      instrument.ponderacion = Number(value);
      return;
    }

    if (key === 'categoria') {
      instrument.categoria = value as CategoriaInstrumento;
      return;
    }

    instrument[key] = String(value);
  }

  getEvaluationTotal(unit: UnidadPlaneacion): number {
    return unit.evaluaciones.reduce(
      (total, evaluation) => total + Number(evaluation.ponderacion || 0),
      0
    );
  }

  getInstrumentTotal(evaluation: EvaluacionUnidad): number {
    return evaluation.instrumentos.reduce(
      (total, instrument) => total + Number(instrument.ponderacion || 0),
      0
    );
  }

  getMomentActivities(moment: FaseSecuencia): ActividadSecuencia[] {
    const unit = this.selectedUnit();

    if (!unit) return [];

    return unit[moment];
  }

  getMethodsForMoment(moment: FaseSecuencia): string[] {
    return this.methodsByMoment[moment];
  }

  addActivity(moment: FaseSecuencia): void {
  const unit = this.selectedUnit();

  if (!unit) return;

  unit[moment].push({
    id: Date.now(),
    consecutivo: 0,
    metodoTecnica: '',
    actividadesDocentes: '',
    actividadesEstudiantes: '',
    evidenciaAprendizaje: '',
    recursos: ''
  });

  this.reorderConsecutives();
}

  removeActivity(moment: FaseSecuencia, activityIndex: number): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    unit[moment].splice(activityIndex, 1);
    this.reorderConsecutives();
  }

  updateActivityField(
    moment: FaseSecuencia,
    activityIndex: number,
    key: keyof ActividadSecuencia,
    value: string | number
  ): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    const activity = unit[moment][activityIndex];

    if (key === 'id') return;

    if (key === 'consecutivo') {
      activity.consecutivo = Number(value);
      return;
    }

    activity[key] = String(value) as never;
  }

  getNextConsecutive(): number {
    const currentForm = this.form();

    if (!currentForm) return 1;

    const activities = currentForm.unidades.flatMap(unit => [
      ...unit.apertura,
      ...unit.desarrollo,
      ...unit.cierre
    ]);

    if (activities.length === 0) return 1;

    return Math.max(...activities.map(activity => activity.consecutivo)) + 1;
  }

  reorderConsecutives(): void {
    const currentForm = this.form();

    if (!currentForm) return;

    let counter = 1;

    currentForm.unidades.forEach(unit => {
      this.moments.forEach(moment => {
        unit[moment.key].forEach(activity => {
          activity.consecutivo = counter;
          counter++;
        });
      });
    });
  }

  addReference(): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    unit.referencias.push({
      id: Date.now(),
      tipo: 'Sitio web',
      autor: '',
      anio: '',
      titulo: '',
      fuente: '',
      url: '',
      precargada: false
    });
  }

  removeReference(referenceIndex: number): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    unit.referencias.splice(referenceIndex, 1);
  }

  updateReferenceField(
    referenceIndex: number,
    key: keyof ReferenciaBibliografica,
    value: string | boolean
  ): void {
    const unit = this.selectedUnit();

    if (!unit) return;

    const reference = unit.referencias[referenceIndex];

    if (key === 'id') return;

    if (key === 'precargada') {
      reference.precargada = Boolean(value);
      return;
    }

    if (key === 'tipo') {
      reference.tipo = value as TipoReferencia;
      return;
    }

    reference[key] = String(value) as never;
  }

  goPrevious(): void {
    if (this.isCaratula()) return;

    const currentSectionIndex = this.sections.findIndex(
      section => section.id === this.activeSection()
    );

    if (currentSectionIndex > 0) {
      this.activeSection.set(this.sections[currentSectionIndex - 1].id);
      this.emitSectionTutorial();
      return;
    }

    if (this.selectedUnitIndex() > 0) {
      this.selectedUnitIndex.update(value => value - 1);
      this.activeSection.set('referencias');
      this.emitSectionTutorial();
      return;
    }

    this.selectCaratula();
  }

  goNext(): void {
    const currentForm = this.form();

    if (!currentForm) return;

    if (this.isCaratula()) {
      this.caratulaSelected.set(false);
      this.selectedUnitIndex.set(0);
      this.activeSection.set('unidad');
      this.emitSectionTutorial();
      return;
    }

    const currentSectionIndex = this.sections.findIndex(
      section => section.id === this.activeSection()
    );

    if (currentSectionIndex < this.sections.length - 1) {
      this.activeSection.set(this.sections[currentSectionIndex + 1].id);
      this.emitSectionTutorial();
      return;
    }

    if (this.selectedUnitIndex() < currentForm.unidades.length - 1) {
      this.selectedUnitIndex.update(value => value + 1);
      this.activeSection.set('unidad');
      this.emitSectionTutorial();
    }
  }

  getCurrentStepLabel(): string {
    if (this.isCaratula()) return 'Carátula';

    const unit = this.selectedUnit();
    const section = this.sections.find(item => item.id === this.activeSection());

    return `Unidad ${this.selectedUnitIndex() + 1} · ${section?.label ?? ''} · ${unit?.nombre ?? ''}`;
  }

  getNextLabel(): string {
    const currentForm = this.form();

    if (!currentForm) return 'Siguiente';

    if (this.isCaratula()) return 'Ir a Unidad 1';

    const currentSectionIndex = this.sections.findIndex(
      section => section.id === this.activeSection()
    );

    if (currentSectionIndex < this.sections.length - 1) {
      return `Ir a ${this.sections[currentSectionIndex + 1].label}`;
    }

    if (this.selectedUnitIndex() < currentForm.unidades.length - 1) {
      return `Ir a Unidad ${this.selectedUnitIndex() + 2}`;
    }

    return 'Finalizar';
  }

  private emitSectionTutorial(): void {
    if (this.activeSection() === 'unidad') {
      this.tutorialChange.emit({
        title: 'Información de la unidad',
        text: 'Completa la información de la unidad de aprendizaje respetando los datos del programa de asignatura.',
        options: [
          'Verifica el nombre de la unidad y el propósito esperado.',
          'Completa saber, saber hacer y saber ser-convivir por cada tema.',
          'No agregues unidades manualmente; las unidades vienen precargadas.'
        ]
      });
      return;
    }

    if (this.activeSection() === 'evaluacion') {
      this.tutorialChange.emit({
        title: 'Sistema de evaluación',
        text: 'Registra el periodo en semanas, resultado de aprendizaje, evidencias, tipos de evaluación e instrumentos.',
        options: [
          'La suma de las evidencias debe ser 100%.',
          'Incluye tipo de evaluación por apertura, desarrollo y cierre.',
          'Los instrumentos deben incluir porcentaje.'
        ]
      });
      return;
    }

    if (this.activeSection() === 'secuencia') {
      this.tutorialChange.emit({
        title: 'Secuencia didáctica',
        text: 'Agrega actividades en apertura, desarrollo y cierre con numeración consecutiva.',
        options: [
          'Selecciona métodos y técnicas desde el combo.',
          'Describe actividades docentes y de estudiantes por pasos.',
          'Cada actividad debe tener evidencia y recursos.'
        ]
      });
      return;
    }

    this.tutorialChange.emit({
      title: 'Referencias bibliográficas y digitales',
      text: 'Agrega referencias bibliográficas o digitales relacionadas con la unidad.',
      options: [
        'Puedes conservar referencias precargadas.',
        'Agrega autor, año, título, fuente y URL cuando aplique.',
        'Las referencias deben sustentar las actividades y evidencias.'
      ]
    });
  }
}
