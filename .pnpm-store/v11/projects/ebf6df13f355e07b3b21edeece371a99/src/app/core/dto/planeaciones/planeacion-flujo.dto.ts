export interface PlaneacionResumenDto {
  publicId: string;
  asignatura: string;
  periodo: string;
  grupos: string;
  docentes: string;
  estado: number;
  revisorPublicId: string | null;
  revisor: string | null;
  ultimaModificacion: string | null;
}

export interface PlaneacionEdicionDto {
  publicId: string;
  estado: number;
  caratula: CaratulaPlaneacionEdicionDto;
  unidades: UnidadPlaneacionEdicionDto[];
  referencias: ReferenciaPlaneacionEdicionDto[];
}

export interface CaratulaPlaneacionEdicionDto {
  programaEducativo: string | null;
  cuatrimestre: number | null;
  nombreAsignatura: string | null;
  docentes: string | null;
  periodoEscolar: string | null;
  grupos: string | null;
  propositoAsignatura: string | null;
  competenciaAsignatura: string | null;
  tipoCompetencia: string | null;
  creditos: number | null;
  modalidad: string | null;
  horasSaber: number | null;
  horasSaberHacer: number | null;
  horasTotales: number | null;
  horasSemana: number | null;
}

export interface UnidadPlaneacionEdicionDto {
  publicId: string | null;
  numeroUnidad: number | null;
  nombreUnidad: string;
  propositoEsperado: string | null;
  horasSaber: number | null;
  horasSaberHacer: number | null;
  horasTotales: number | null;
  porcentajeUnidad: number | null;
  orden: number;
  temas: TemaPlaneacionEdicionDto[];
  evaluaciones: EvaluacionPlaneacionEdicionDto[];
  secuencias?: SecuenciaPlaneacionEdicionDto[];
  apertura?: SecuenciaPlaneacionEdicionDto[];
  desarrollo?: SecuenciaPlaneacionEdicionDto[];
  cierre?: SecuenciaPlaneacionEdicionDto[];
}

export interface TemaPlaneacionEdicionDto {
  publicId: string | null;
  tema: string;
  saberConceptual: string | null;
  saberHacer: string | null;
  saberSer: string | null;
  orden: number;
}

export interface EvaluacionPlaneacionEdicionDto {
  publicId: string | null;
  periodoSemanas: number | null;
  resultadoAprendizaje: string | null;
  evidenciaAprendizaje: string | null;
  fase: number;
  tipoEvaluacion: number | null;
  agenteEvaluador: number;
  ponderacion: number | null;
  instrumentoEvaluacion: string | null;
  orden: number;
}

export interface SecuenciaPlaneacionEdicionDto {
  publicId: string | null;
  fase: number | null;
  metodoTecnica: number | null;
  estrategia: number | null;
  actividadDocente: string | null;
  actividadEstudiante: string | null;
  evidenciaAprendizaje: string | null;
  mediosMateriales: string | null;
  orden: number;
}

export interface ReferenciaPlaneacionEdicionDto {
  publicId: string | null;
  referenciaAPA: string;
  orden: number;
}

export interface CrearComentarioCorreccionDto {
  mensaje: string;
}

export interface ComentarioCorreccionDto {
  publicId: string;
  usuarioPublicId: string;
  usuario: string;
  rolEnChat: string;
  mensaje: string;
  fecha: string;
}

export interface ComentariosCorreccionDto {
  estadoPlaneacion: number;
  ocultosPorAprobacion: boolean;
  comentarios: ComentarioCorreccionDto[];
}
