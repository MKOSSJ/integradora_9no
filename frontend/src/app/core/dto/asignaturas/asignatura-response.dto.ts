export interface AsignaturaResponseDto {
  publicId: string;
  academiaPublicId: string | null;
  nombre: string;
  clave: string;
  cuatrimestre: number;
  horasTotales: number;
  horasSemana: number;
  creditos: number;
  activo: boolean;
}

export interface AcademiaResponseDto {
  publicId: string;
  nombre: string;
  descripcion: string | null;
  activo: boolean;
}
