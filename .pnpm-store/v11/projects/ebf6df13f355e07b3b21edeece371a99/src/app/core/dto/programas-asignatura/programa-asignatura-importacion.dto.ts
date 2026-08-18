export interface ProgramaAsignaturaImportacionResultadoDto {
  archivo: string;
  programaAsignaturaPublicId: string | null;
  asignatura: string | null;
  clave: string | null;
  unidadesExtraidas: number;
  datosGuardados: boolean;
  errores: string[];
}

