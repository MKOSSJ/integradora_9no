export type SystemRole = 'DIRECTIVO' | 'DOCENTE' | 'REVISOR';
export type RolAcademia = 'Docente' | 'Revisor';
export type EntityStatus = 'activo' | 'inactivo';
export type DataSourceOrigin = 'backend' | 'local';

export interface UsuarioAdmin {
  source: 'local';
  id: string;
  publicId: string;
  nombre: string;
  apellidoPaterno: string;
  apellidoMaterno?: string;
  email: string;
  telefono?: string;
  roles: SystemRole[];
  academiaPublicId?: string;
  academiaNombre?: string;
  rolEnAcademia?: RolAcademia | '';
  estado: EntityStatus;
  ultimoAcceso?: string;
}

export interface UsuarioBackendListItem {
  [key: string]: unknown;
  source: 'backend';
  id: string;
  publicId: string;
  nombre: string;
  apellidoPaterno: string;
  apellidoMaterno: string;
  email: string;
  telefono: string;
  ultimoAcceso: string;
  roles: SystemRole[];
  academiaNombre: string;
  rolEnAcademia: string;
  estado: string;
}

export interface Carrera {
  id: string;
  publicId: string;
  nombre: string;
  clave: string;
  nivel: string;
  estado: EntityStatus;
}

export interface CicloEscolar {
  id: string;
  publicId: string;
  nombre: string;
  fechaInicio: string;
  fechaFin: string;
  estado: EntityStatus;
}

export interface Periodo {
  id: string;
  publicId: string;
  cicloEscolarPublicId: string;
  cicloEscolarNombre: string;
  nombre: string;
  fechaInicio: string;
  fechaFin: string;
  estado: EntityStatus;
}

export interface Academia {
  id: string;
  publicId: string;
  nombre: string;
  descripcion?: string;
  estado: EntityStatus;
  totalUsuarios: number;
  totalAsignaturas: number;
}

export interface Asignatura {
  id: string;
  publicId: string;
  academiaPublicId: string;
  academiaNombre: string;
  nombre: string;
  clave: string;
  cuatrimestre: number;
  horasTotales: number;
  horasSemana: number;
  creditos: number;
  estado: EntityStatus;
}

export interface Grupo {
  id: string;
  publicId: string;
  nombre: string;
  cuatrimestre: number;
  carreraPublicId: string;
  carreraNombre: string;
  periodoPublicId: string;
  periodoNombre: string;
  estado: EntityStatus;
}

export interface CargaAcademica {
  source: DataSourceOrigin;
  id: string;
  publicId: string;
  periodoPublicId: string;
  periodoNombre: string;
  grupoPublicId: string;
  grupoNombre: string;
  asignaturaPublicId: string;
  asignaturaNombre: string;
  docentePublicId: string;
  docenteNombre: string;
  docenteRoles?: string[];
  revisorPublicId?: string;
  revisorNombre?: string;
  revisorRoles?: string[];
  academiaPublicId?: string;
  academiaNombre?: string;
  estado: EntityStatus;
}
