export interface AsignaturaRequestDto {
  nombre: string;
  clave: string;
  cuatrimestre: number;
  horasTotales: number;
  horasSemana: number;
  creditos: number;
  academiaPublicId: string | null;
}
