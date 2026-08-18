export interface CargaAcademicaRequestDto {
  periodoPublicId: string;
  grupoPublicId: string;
  asignaturaPublicId: string;
  docentePublicId: string;
  revisorPublicId: string | null;
  academiaPublicId: string | null;
}
