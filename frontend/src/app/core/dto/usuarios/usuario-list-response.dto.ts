export interface UsuarioListResponseDto {
  publicId: string;
  nombre: string;
  apellidoPaterno: string;
  apellidoMaterno: string | null;
  email: string | null;
  telefono: string | null;
  ultimoAcceso: string | null;
}
