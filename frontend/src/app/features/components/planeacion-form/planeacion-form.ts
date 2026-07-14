import { NgClass } from '@angular/common';
import { Component, computed, EventEmitter, Input, OnChanges, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  LucideDynamicIcon,
  LucidePlus,
  LucideTrash2,
  LucideBookOpen,
  LucideClipboardCheck,
  LucideLayers,
  LucideFileText,
  LucideListChecks,
  LucideCircleHelp,
  LucideCircleCheckBig,
  LucideArrowLeft,
  LucideArrowRight,
} from '@lucide/angular';

import {
  FormSection,
  MomentoDidactico,
  PlaneacionCaratula,
  PlaneacionDetail,
  PlaneacionFormulario,
  PlaneacionTutorial,
  UnidadPlaneacion,
} from '../../../core/models/planeacion.model';

interface FormField {
  key: string;
  label: string;
  help: string;
  type: 'text' | 'number' | 'textarea' | 'select';
  options?: string[];
  span?: string;
}

interface MomentConfig {
  key: 'apertura' | 'desarrollo' | 'cierre';
  label: string;
  help: string;
}

@Component({
  selector: 'app-planeacion-form',
  standalone: true,
  imports: [NgClass, FormsModule, LucideDynamicIcon],
  templateUrl: './planeacion-form.html',
  styleUrl: './planeacion-form.css',
})
export class PlaneacionForm implements OnChanges {
  @Input({ required: true }) planeacion!: PlaneacionDetail;
  @Output() tutorialChange = new EventEmitter<PlaneacionTutorial>();

  form = signal<PlaneacionFormulario | null>(null);
  selectedUnitIndex = signal(-1);
  activeSection = signal<FormSection>('unidad');

  plusIcon = LucidePlus;
  trashIcon = LucideTrash2;
  caratulaIcon = LucideFileText;
  unidadIcon = LucideBookOpen;
  evaluacionIcon = LucideClipboardCheck;
  secuenciaIcon = LucideLayers;
  referenciasIcon = LucideListChecks;
  helpIcon = LucideCircleHelp;
  checkIcon = LucideCircleCheckBig;
  prevIcon = LucideArrowLeft;
  nextIcon = LucideArrowRight;

  private readonly unitSectionOrder: FormSection[] = [
    'unidad',
    'evaluacion',
    'secuencia',
    'referencias',
  ];

  sections: {
    id: FormSection;
    label: string;
    description: string;
    icon: any;
  }[] = [
    {
      id: 'unidad',
      label: 'Información',
      description: 'Datos, horas y temas.',
      icon: LucideBookOpen,
    },
    {
      id: 'evaluacion',
      label: 'Evaluación',
      description: 'Evidencias e instrumentos.',
      icon: LucideClipboardCheck,
    },
    {
      id: 'secuencia',
      label: 'Secuencia',
      description: 'Apertura, desarrollo y cierre.',
      icon: LucideLayers,
    },
    {
      id: 'referencias',
      label: 'Referencias',
      description: 'Fuentes usadas.',
      icon: LucideListChecks,
    },
  ];

  moments: MomentConfig[] = [
    {
      key: 'apertura',
      label: 'Apertura',
      help: 'Diagnóstico, motivación, activación de conocimientos previos e inicio conceptual.',
    },
    {
      key: 'desarrollo',
      label: 'Desarrollo',
      help: 'Consolidación de habilidades mediante actividades prácticas, casos, proyectos o problemas.',
    },
    {
      key: 'cierre',
      label: 'Cierre',
      help: 'Integración, reflexión, autonomía y transferencia del aprendizaje.',
    },
  ];

  caratulaFields: FormField[] = [
    {
      key: 'programaEducativo',
      label: 'Programa educativo',
      help: 'Escribe el nombre del programa educativo, por ejemplo TSU o Licenciatura.',
      type: 'text',
    },
    {
      key: 'docentes',
      label: 'Docente(s)',
      help: 'Incluye los nombres de los docentes o academia que elaboran la planeación.',
      type: 'text',
    },
    {
      key: 'cuatrimestre',
      label: 'Cuatrimestre',
      help: 'Indica el grado en que se imparte la asignatura.',
      type: 'text',
    },
    {
      key: 'periodoEscolar',
      label: 'Periodo escolar',
      help: 'Ejemplo: Septiembre - Diciembre 2024.',
      type: 'text',
    },
    {
      key: 'asignatura',
      label: 'Nombre de la asignatura',
      help: 'Debe escribirse tal como aparece en el programa de asignatura.',
      type: 'text',
    },
    {
      key: 'grupos',
      label: 'Grupo(s)',
      help: 'Detalla los grupos en los que se impartirá la asignatura.',
      type: 'text',
    },
    {
      key: 'propositoAsignatura',
      label: 'Propósito de la asignatura',
      help: 'Debe copiarse del programa de asignatura sin modificaciones.',
      type: 'textarea',
      span: 'md:col-span-2',
    },
    {
      key: 'competenciaContribuye',
      label: 'Competencia a la que contribuye',
      help: 'Debe copiarse del programa de asignatura sin modificar su contenido.',
      type: 'textarea',
      span: 'md:col-span-2',
    },
    {
      key: 'tipoCompetencia',
      label: 'Tipo de competencia',
      help: 'Puede ser Base, Transversal o Específica.',
      type: 'select',
      options: ['Base', 'Transversal', 'Específica'],
    },
    {
      key: 'creditos',
      label: 'Créditos',
      help: 'Número de créditos indicado en el programa.',
      type: 'number',
    },
    {
      key: 'modalidad',
      label: 'Modalidad',
      help: 'Escolarizada, Mixta, Dual o No escolarizada.',
      type: 'select',
      options: ['Escolarizada', 'Mixta', 'Dual', 'No escolarizada'],
    },
    {
      key: 'horasSaber',
      label: 'Horas del saber',
      help: 'Horas teóricas indicadas en el programa.',
      type: 'number',
    },
    {
      key: 'horasSaberHacer',
      label: 'Horas del saber hacer',
      help: 'Horas prácticas indicadas en el programa.',
      type: 'number',
    },
    {
      key: 'horasTotales',
      label: 'Horas totales',
      help: 'Suma total de horas de la asignatura.',
      type: 'number',
    },
    {
      key: 'horasSemana',
      label: 'Horas por semana',
      help: 'Horas semanales asignadas para impartir la asignatura.',
      type: 'number',
    },
  ];

  unidadFields: FormField[] = [
    {
      key: 'nombre',
      label: 'Nombre de la unidad de aprendizaje',
      help: 'Debe tomarse del programa de asignatura.',
      type: 'text',
      span: 'md:col-span-2',
    },
    {
      key: 'propositoEsperado',
      label: 'Propósito esperado',
      help: 'Describe el propósito esperado de la unidad.',
      type: 'textarea',
      span: 'md:col-span-2',
    },
    {
      key: 'horasSaber',
      label: 'Horas del saber',
      help: 'Horas teóricas de la unidad.',
      type: 'number',
    },
    {
      key: 'horasSaberHacer',
      label: 'Horas del saber hacer',
      help: 'Horas prácticas de la unidad.',
      type: 'number',
    },
    {
      key: 'horasTotales',
      label: 'Horas totales',
      help: 'Suma de horas del saber y saber hacer.',
      type: 'number',
    },
    {
      key: 'porcentajeUnidad',
      label: 'Porcentaje de la unidad',
      help: 'Se calcula con las horas totales de la unidad respecto al total de la asignatura.',
      type: 'number',
    },
  ];

  evaluationOptions = [
    'Autoevaluación',
    'Coevaluación',
    'Heteroevaluación',
    'Autoevaluación y coevaluación',
    'Coevaluación y heteroevaluación',
  ];

  instrumentOptions = [
    'Cuestionario de preguntas abiertas',
    'Prueba objetiva',
    'Prueba por competencias',
    'Lista de cotejo',
    'Guía de observación',
    'Escala estimativa',
    'Rúbrica',
  ];

  isCaratula = computed(() => this.selectedUnitIndex() === -1);

  selectedUnit = computed(() => {
    return this.form()?.unidades[this.selectedUnitIndex()] ?? null;
  });

  tutorial = computed(() => {
    if (this.isCaratula()) {
      return {
        title: 'Tutorial de carátula',
        text: 'Llena los datos generales de la asignatura. Propósito, competencia, créditos y horas deben copiarse del programa sin modificarlos.',
      };
    }

    const section = this.activeSection();

    if (section === 'unidad') {
      return {
        title: 'Tutorial de unidad',
        text: 'Registra nombre de la unidad, propósito esperado, horas, porcentaje y temas con sus saberes.',
      };
    }

    if (section === 'evaluacion') {
      return {
        title: 'Tutorial de evaluación',
        text: 'Agrega evidencias, tipo de evaluación, ponderación e instrumento. La suma de ponderaciones por unidad debe ser 100%.',
      };
    }

    if (section === 'secuencia') {
      return {
        title: 'Tutorial de secuencia',
        text: 'Describe apertura, desarrollo y cierre. Incluye métodos, actividades docentes, actividades del estudiante, evidencias y recursos.',
      };
    }

    return {
      title: 'Tutorial de referencias',
      text: 'Agrega referencias bibliográficas y digitales utilizadas para esta unidad.',
    };
  });

  sideOptions = computed(() => {
    if (this.isCaratula()) {
      return {
        title: 'Puntos clave de carátula',
        items: [
          'El propósito debe copiarse del programa de asignatura.',
          'La competencia debe escribirse sin modificar el contenido.',
          'El tipo de competencia puede ser Base, Transversal o Específica.',
          'Las horas y créditos deben coincidir con el programa.',
        ],
      };
    }

    const section = this.activeSection();

    if (section === 'unidad') {
      return {
        title: 'Puntos clave de unidad',
        items: [
          'El nombre de la unidad se toma del programa.',
          'El porcentaje se calcula según las horas totales de la unidad.',
          'Los temas deben incluir saber, saber hacer y saber ser-convivir.',
          'Puedes agregar más temas si la unidad lo requiere.',
        ],
      };
    }

    if (section === 'evaluacion') {
      return {
        title: 'Puntos clave de evaluación',
        items: [
          'La suma de ponderaciones debe ser 100%.',
          'Incluye evidencias congruentes con el resultado de aprendizaje.',
          'Usa tipos como autoevaluación, coevaluación o heteroevaluación.',
          'Selecciona instrumentos como rúbrica, lista de cotejo o guía de observación.',
        ],
      };
    }

    if (section === 'secuencia') {
      return {
        title: 'Puntos clave de secuencia',
        items: [
          'Apertura: diagnóstico, motivación y conocimientos previos.',
          'Desarrollo: práctica, proyectos, casos o problemas.',
          'Cierre: reflexión, autonomía y transferencia.',
          'Describe actividades del docente y del estudiante por pasos.',
        ],
      };
    }

    return {
      title: 'Puntos clave de referencias',
      items: [
        'Agrega bibliografía del programa de asignatura.',
        'Incluye libros, artículos, guías o recursos digitales.',
        'Evita referencias incompletas.',
        'Puedes usar una referencia por línea.',
      ],
    };
  });

  completionPercentage = computed(() => {
    const current = this.form();

    if (!current) return 0;

    let total = 0;
    let completed = 0;

    const check = (value: unknown) => {
      total++;

      if (value !== null && value !== undefined && String(value).trim() !== '') {
        completed++;
      }
    };

    Object.values(current.caratula).forEach(check);

    current.unidades.forEach((unit) => {
      check(unit.nombre);
      check(unit.propositoEsperado);
      check(unit.horasSaber);
      check(unit.horasSaberHacer);
      check(unit.horasTotales);
      check(unit.porcentajeUnidad);
      check(unit.duracionSemanas);
      check(unit.resultadoAprendizaje);
      check(unit.referencias);

      unit.temas.forEach((topic) => {
        check(topic.tema);
        check(topic.saber);
        check(topic.saberHacer);
        check(topic.saberSerConvivir);
      });

      unit.evaluaciones.forEach((evaluation) => {
        check(evaluation.evidenciaAprendizaje);
        check(evaluation.tipoEvaluacion);
        check(evaluation.ponderacion);
        check(evaluation.instrumentoEvaluacion);
      });

      [unit.apertura, unit.desarrollo, unit.cierre].forEach((moment) => {
        check(moment.metodosTecnicas);
        check(moment.actividadesDocentes);
        check(moment.actividadesEstudiantes);
        check(moment.evidenciaAprendizaje);
        check(moment.recursos);
      });
    });

    return total === 0 ? 0 : Math.round((completed / total) * 100);
  });

  ngOnChanges(): void {
    this.form.set(structuredClone(this.planeacion.formulario));
    this.selectedUnitIndex.set(-1);
    this.activeSection.set('unidad');

    queueMicrotask(() => {
      this.emitTutorial();
    });
  }

  emitTutorial(): void {
    this.tutorialChange.emit({
      title: this.tutorial().title,
      text: this.tutorial().text,
      options: this.sideOptions().items,
    });
  }

  selectCaratula(): void {
    this.selectedUnitIndex.set(-1);
    this.emitTutorial();
  }

  selectUnit(index: number): void {
    this.selectedUnitIndex.set(index);
    this.activeSection.set('unidad');
    this.emitTutorial();
  }

  setSection(section: FormSection): void {
    this.activeSection.set(section);
    this.emitTutorial();
  }

  goPrevious(): void {
    if (this.isCaratula()) {
      return;
    }

    const currentSection = this.activeSection();
    const currentIndex = this.unitSectionOrder.indexOf(currentSection);

    if (currentIndex > 0) {
      this.activeSection.set(this.unitSectionOrder[currentIndex - 1]);
      this.emitTutorial();
      return;
    }

    this.selectCaratula();
  }

  goNext(): void {
    const currentForm = this.form();
    if (!currentForm) return;

    if (this.isCaratula()) {
      if (currentForm.unidades.length > 0) {
        this.selectUnit(0);
      }
      return;
    }

    const currentSection = this.activeSection();
    const currentIndex = this.unitSectionOrder.indexOf(currentSection);

    if (currentIndex < this.unitSectionOrder.length - 1) {
      this.activeSection.set(this.unitSectionOrder[currentIndex + 1]);
      this.emitTutorial();
      return;
    }

    const nextUnitIndex = this.selectedUnitIndex() + 1;

    if (nextUnitIndex < currentForm.unidades.length) {
      this.selectUnit(nextUnitIndex);
      return;
    }

    this.addUnit();
  }

  getNextLabel(): string {
    if (this.isCaratula()) {
      return 'Ir a Unidad 1';
    }

    const currentSection = this.activeSection();

    if (currentSection === 'unidad') return 'Continuar a evaluación';
    if (currentSection === 'evaluacion') return 'Continuar a secuencia';
    if (currentSection === 'secuencia') return 'Continuar a referencias';

    const currentForm = this.form();
    const nextUnitIndex = this.selectedUnitIndex() + 1;

    if (currentForm && nextUnitIndex < currentForm.unidades.length) {
      return `Ir a Unidad ${nextUnitIndex + 1}`;
    }

    return 'Agregar otra unidad';
  }

  getCurrentStepLabel(): string {
    if (this.isCaratula()) {
      return 'Carátula';
    }

    const unitNumber = this.selectedUnitIndex() + 1;

    if (this.activeSection() === 'unidad') return `Unidad ${unitNumber} · Información`;
    if (this.activeSection() === 'evaluacion') return `Unidad ${unitNumber} · Evaluación`;
    if (this.activeSection() === 'secuencia') return `Unidad ${unitNumber} · Secuencia`;

    return `Unidad ${unitNumber} · Referencias`;
  }

  getCaratulaValue(key: string): string | number {
    return (this.form()?.caratula as any)?.[key] ?? '';
  }

  updateCaratulaField(key: string, value: string | number): void {
    const current = this.form();
    if (!current) return;

    this.form.set({
      ...current,
      caratula: {
        ...current.caratula,
        [key]: value,
      } as PlaneacionCaratula,
    });
  }

  getUnitValue(key: string): string | number {
    return (this.selectedUnit() as any)?.[key] ?? '';
  }

  updateUnitField(key: string, value: string | number): void {
    const current = this.form();
    if (!current) return;

    const unit = this.selectedUnit();
    if (!unit) return;

    const units = [...current.unidades];

    units[this.selectedUnitIndex()] = {
      ...unit,
      [key]: value,
    } as UnidadPlaneacion;

    this.form.set({
      ...current,
      unidades: units,
    });
  }

  updateMomentField(
    momentKey: 'apertura' | 'desarrollo' | 'cierre',
    field: keyof MomentoDidactico,
    value: string,
  ): void {
    const current = this.form();
    const unit = this.selectedUnit();

    if (!current || !unit) return;

    const units = [...current.unidades];

    units[this.selectedUnitIndex()] = {
      ...unit,
      [momentKey]: {
        ...unit[momentKey],
        [field]: value,
      },
    };

    this.form.set({
      ...current,
      unidades: units,
    });
  }

  addUnit(): void {
    const current = this.form();
    if (!current) return;

    const nextNumber = current.unidades.length + 1;

    const newUnit: UnidadPlaneacion = {
      id: Date.now(),
      nombre: `Unidad ${nextNumber}`,
      propositoEsperado: '',
      horasSaber: 0,
      horasSaberHacer: 0,
      horasTotales: 0,
      porcentajeUnidad: 0,
      duracionSemanas: 0,
      resultadoAprendizaje: '',
      temas: [
        {
          id: Date.now() + 1,
          tema: '',
          saber: '',
          saberHacer: '',
          saberSerConvivir: '',
        },
      ],
      evaluaciones: [
        {
          id: Date.now() + 2,
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

    this.form.set({
      ...current,
      unidades: [...current.unidades, newUnit],
    });

    this.selectedUnitIndex.set(current.unidades.length);
    this.activeSection.set('unidad');
    this.emitTutorial();
  }

  removeUnit(index: number): void {
    const current = this.form();

    if (!current || current.unidades.length <= 1) return;

    const units = current.unidades.filter((_, unitIndex) => unitIndex !== index);

    this.form.set({
      ...current,
      unidades: units,
    });

    this.selectedUnitIndex.set(Math.max(0, index - 1));
    this.activeSection.set('unidad');
    this.emitTutorial();
  }

  addTopic(): void {
    const current = this.form();
    const unit = this.selectedUnit();

    if (!current || !unit) return;

    const units = [...current.unidades];

    units[this.selectedUnitIndex()] = {
      ...unit,
      temas: [
        ...unit.temas,
        {
          id: Date.now(),
          tema: '',
          saber: '',
          saberHacer: '',
          saberSerConvivir: '',
        },
      ],
    };

    this.form.set({
      ...current,
      unidades: units,
    });
  }

  updateTopicField(index: number, key: string, value: string): void {
    const current = this.form();
    const unit = this.selectedUnit();

    if (!current || !unit) return;

    const topics = [...unit.temas];

    topics[index] = {
      ...topics[index],
      [key]: value,
    };

    const units = [...current.unidades];

    units[this.selectedUnitIndex()] = {
      ...unit,
      temas: topics,
    };

    this.form.set({
      ...current,
      unidades: units,
    });
  }

  addEvaluation(): void {
    const current = this.form();
    const unit = this.selectedUnit();

    if (!current || !unit) return;

    const units = [...current.unidades];

    units[this.selectedUnitIndex()] = {
      ...unit,
      evaluaciones: [
        ...unit.evaluaciones,
        {
          id: Date.now(),
          evidenciaAprendizaje: '',
          tipoEvaluacion: '',
          ponderacion: 0,
          instrumentoEvaluacion: '',
        },
      ],
    };

    this.form.set({
      ...current,
      unidades: units,
    });
  }

  updateEvaluationField(index: number, key: string, value: string | number): void {
    const current = this.form();
    const unit = this.selectedUnit();

    if (!current || !unit) return;

    const evaluations = [...unit.evaluaciones];

    evaluations[index] = {
      ...evaluations[index],
      [key]: value,
    };

    const units = [...current.unidades];

    units[this.selectedUnitIndex()] = {
      ...unit,
      evaluaciones: evaluations,
    };

    this.form.set({
      ...current,
      unidades: units,
    });
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
