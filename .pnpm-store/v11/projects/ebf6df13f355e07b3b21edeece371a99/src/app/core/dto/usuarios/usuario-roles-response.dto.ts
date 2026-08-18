export interface RolUsuarioResponseDto {
  publicId: string;
  nombre: string;
  descripcion: string | null;
}

export interface UsuarioRolesResponseDto {
  usuarioPublicId: string;
  usuario: string;
  roles: RolUsuarioResponseDto[];
}
