export interface PlaneacionAsignacionRevisor {
  [key: string]: unknown;
  id: string;
  publicId: string;
  asignatura: string;
  docente: string;
  periodo: string;
  grupos: string;
  estado: string;
  revisorPublicId: string;
  revisorNombre: string;
  resultadoGeneracion: string;
}

export interface GeneracionPlaneacionVisual {
  programaAsignaturaPublicId: string;
  planeacionPublicId: string | null;
  asignatura: string;
  docentes: string;
  periodo: string;
  grupos: string;
  estado: string;
  resultado: string;
  resultadoGeneracion: string;
}

export interface GeneracionPlaneacionesVisualResultado {
  totalProgramas: number;
  planeacionesCreadas: number;
  yaExistentes: number;
  omitidas: number;
  planeaciones: GeneracionPlaneacionVisual[];
}
