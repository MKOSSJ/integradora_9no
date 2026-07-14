export type PlaneacionStatus = 'aprobado' | 'borrador' | 'revision' | 'pendiente' | 'correcciones';

export type PlaneacionTab = 'vista-previa' | 'programa' | 'formulario';

export type FormSection = 'unidad' | 'evaluacion' | 'secuencia' | 'referencias';

export interface PlaneacionTutorial {
  title: string;
  text: string;
  options: string[];
}

export interface PlaneacionListItem {
  id: number;
  titulo: string;
  descripcion: string;
  actualizacion: string;
  progreso: number;
  status: PlaneacionStatus;
}

export interface ProgramaAsignatura {
  nombre: string;
  clave: string;
  programaEducativo: string;
  cuatrimestre: string;
  creditos: number;
  horasTotales: number;
  proposito: string;
}

export interface PlaneacionCaratula {
  programaEducativo: string;
  docentes: string;
  cuatrimestre: string;
  periodoEscolar: string;
  asignatura: string;
  grupos: string;
  propositoAsignatura: string;
  competenciaContribuye: string;
  tipoCompetencia: string;
  creditos: number;
  modalidad: string;
  horasSaber: number;
  horasSaberHacer: number;
  horasTotales: number;
  horasSemana: number;
}

export interface TemaUnidad {
  id: number;
  tema: string;
  saber: string;
  saberHacer: string;
  saberSerConvivir: string;
}

export interface EvaluacionUnidad {
  id: number;
  evidenciaAprendizaje: string;
  tipoEvaluacion: string;
  ponderacion: number;
  instrumentoEvaluacion: string;
}

export interface MomentoDidactico {
  metodosTecnicas: string;
  actividadesDocentes: string;
  actividadesEstudiantes: string;
  evidenciaAprendizaje: string;
  recursos: string;
}

export interface UnidadPlaneacion {
  id: number;
  nombre: string;
  propositoEsperado: string;
  horasSaber: number;
  horasSaberHacer: number;
  horasTotales: number;
  porcentajeUnidad: number;
  duracionSemanas: number;
  resultadoAprendizaje: string;
  temas: TemaUnidad[];
  evaluaciones: EvaluacionUnidad[];
  apertura: MomentoDidactico;
  desarrollo: MomentoDidactico;
  cierre: MomentoDidactico;
  referencias: string;
}

export interface PlaneacionFormulario {
  caratula: PlaneacionCaratula;
  unidades: UnidadPlaneacion[];
}

export interface PlaneacionDetail extends PlaneacionListItem {
  autor: string;
  fechaCreacion: string;
  ultimaModificacion: string;
  pdfPages: number;
  programa: ProgramaAsignatura;
  formulario: PlaneacionFormulario;
}
