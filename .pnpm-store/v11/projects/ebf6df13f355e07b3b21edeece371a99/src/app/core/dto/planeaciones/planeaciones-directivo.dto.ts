export interface GeneracionPlaneacionDetalleDto {
  programaAsignaturaPublicId: string;
  asignatura: string;
  planeacionPublicId: string | null;
  estado: string;
  mensaje: string | null;
}

export interface GeneracionPlaneacionesResultadoDto {
  totalProgramas: number;
  planeacionesCreadas: number;
  yaExistentes: number;
  omitidas: number;
  planeaciones: GeneracionPlaneacionDetalleDto[];
}

export interface AsignarRevisorPlaneacionRequestDto {
  revisorPublicId: string;
}

export interface PlaneacionResumenResponseDto {
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

export interface PlaneacionCaratulaResponseDto {
  nombreAsignatura: string | null;
  docentes: string | null;
  periodoEscolar: string | null;
  grupos: string | null;
}

export interface PlaneacionEdicionResponseDto {
  publicId: string;
  estado: number;
  caratula: PlaneacionCaratulaResponseDto;
}

export interface PlaneacionDetalleConArchivosResponseDto {
  planeacion: PlaneacionEdicionResponseDto;
  archivos: {
    programaAsignatura: ArchivoRelacionadoResponseDto;
    planeacionDidactica: ArchivoRelacionadoResponseDto;
  };
}

export interface ArchivoRelacionadoResponseDto {
  disponible: boolean;
  nombre: string | null;
  mimeType: string | null;
  urlVisualizacion: string | null;
  urlDescarga: string | null;
}
