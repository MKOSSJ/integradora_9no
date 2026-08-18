export interface ImportacionCargaAcademicaErrorDto {
  fila: number;
  campo: string;
  valor: string | null;
  mensaje: string;
}

export interface ImportacionCargaAcademicaResultadoDto {
  totalFilas: number;
  procesadas: number;
  insertadas: number;
  omitidas: number;
  errores: ImportacionCargaAcademicaErrorDto[];
}

