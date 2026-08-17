export interface CargaAcademicaResponseDto {
  publicId: string;
  periodoPublicId: string;
  grupoPublicId: string;
  asignaturaPublicId: string;
  docentePublicId: string;
  revisorPublicId: string | null;
  academiaPublicId: string | null;
  activo: boolean;
}
