export type PlaneacionStatus =
  | 'aprobado'
  | 'borrador'
  | 'revision'
  | 'pendiente'
  | 'correcciones';

export type PlaneacionTab =
  | 'vista-previa'
  | 'programa'
  | 'formulario';

export type FormSection =
  | 'unidad'
  | 'evaluacion'
  | 'secuencia'
  | 'referencias';

export type TipoCompetencia =
  | 'Base'
  | 'Transversal'
  | 'Específica';

export type ModalidadAsignatura =
  | 'Escolarizada'
  | 'Mixta'
  | 'Dual'
  | 'No escolarizada';

export type FaseSecuencia =
  | 'apertura'
  | 'desarrollo'
  | 'cierre';

export type AgenteEvaluacion =
  | 'Autoevaluación'
  | 'Coevaluación'
  | 'Heteroevaluación';

export type CategoriaInstrumento =
  | 'Conocimiento'
  | 'Producto'
  | 'Desempeño';

export type TipoReferencia =
  | 'Libro'
  | 'Artículo'
  | 'Sitio web'
  | 'Video'
  | 'Documento'
  | 'Otro';

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

export interface ReferenciaBibliografica {
  id: number;
  tipo: TipoReferencia;
  autor: string;
  anio: string;
  titulo: string;
  fuente: string;
  url?: string;
  precargada?: boolean;
}

export interface ProgramaAsignatura {
  id?: number;
  nombre: string;
  clave: string;
  programaEducativo: string;
  cuatrimestre: string;
  creditos: number;
  horasTotales: number;
  horasSaber: number;
  horasSaberHacer: number;
  horasSemana: number;
  proposito: string;
  competencia: string;
  tipoCompetencia: TipoCompetencia;
  modalidad: ModalidadAsignatura;
  referenciasBase: ReferenciaBibliografica[];
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
  tipoCompetencia: TipoCompetencia;
  creditos: number;
  modalidad: ModalidadAsignatura;
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

export interface TipoEvaluacionFase {
  id: number;
  fase: FaseSecuencia;
  tipoEvaluacion: string;
  agenteEvaluacion: AgenteEvaluacion;
}

export interface InstrumentoEvaluacion {
  id: number;
  categoria: CategoriaInstrumento;
  nombre: string;
  instrumento: string;
  ponderacion: number;
}

export interface EvaluacionUnidad {
  id: number;
  evidenciaAprendizaje: string;
  ponderacion: number;
  tiposEvaluacion: TipoEvaluacionFase[];
  instrumentos: InstrumentoEvaluacion[];
}

export interface ActividadSecuencia {
  id: number;
  consecutivo: number;
  metodoTecnica: string;
  actividadesDocentes: string;
  actividadesEstudiantes: string;
  evidenciaAprendizaje: string;
  recursos: string;
}

export interface UnidadPlaneacion {
  id: number;
  numero?: number;
  nombre: string;
  propositoEsperado: string;
  horasSaber: number;
  horasSaberHacer: number;
  horasTotales: number;
  porcentajeUnidad: number;

  periodoSemanas: number;
  resultadoAprendizaje: string;

  temas: TemaUnidad[];
  evaluaciones: EvaluacionUnidad[];

  apertura: ActividadSecuencia[];
  desarrollo: ActividadSecuencia[];
  cierre: ActividadSecuencia[];

  referencias: ReferenciaBibliografica[];
}

export interface PlaneacionFormulario {
  titulo: string;

  periodoId: number | null;
  asignaturaId: number | null;
  academiaId: number | null;
  programaAsignaturaId: number | null;
  revisorId: number | null;

  docenteIds: number[];
  grupoIds: number[];

  caratula: PlaneacionCaratula;
  unidades: UnidadPlaneacion[];
}

export interface PlaneacionDetail extends PlaneacionListItem {
  autor: string;
  fechaCreacion: string;
  ultimaModificacion: string;
  fechaEnvioRevision?: string;
  fechaLimiteCaptura?: string;
  fechaValidacion?: string;
  fechaAutorizacion?: string;

  pdfPages: number;
  programa: ProgramaAsignatura;
  formulario: PlaneacionFormulario;
}

/* Seguimiento para administrador/directivo */

export type SeguimientoEstado =
  | 'en-tiempo'
  | 'por-vencer'
  | 'vencida'
  | 'completada';

export interface SeguimientoPlaneacion {
  id: number;
  titulo: string;
  docente: string;
  asignatura: string;
  grupos: string;
  status: PlaneacionStatus;

  fechaCreacion: string;
  fechaLimiteCaptura: string;
  fechaEnvioRevision?: string;
  fechaValidacion?: string;
  fechaAutorizacion?: string;

  diasRestantes: number;
  estadoSeguimiento: SeguimientoEstado;
}