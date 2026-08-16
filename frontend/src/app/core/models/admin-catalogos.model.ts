export type SystemRole = 'ADMIN' | 'DOCENTE' | 'REVISOR' ;
export type RolAcademia = 'Docente' | 'Revisor';
export type EntityStatus = 'activo' | 'inactivo';

export interface UsuarioAcademia {
  academiaId: number;
  academiaNombre: string;
  rolEnAcademia: RolAcademia;
  activo: boolean;
}

export interface UsuarioAdmin {
  id: number;
  publicId: string;
  nombre: string;
  apellidoPaterno: string;
  apellidoMaterno?: string;
  email: string;
  telefono?: string;
  roles: SystemRole[];
  academias: UsuarioAcademia[];
  estado: EntityStatus;
  ultimoAcceso?: string;
}

export interface Carrera {
  id: number;
  publicId: string;
  nombre: string;
  clave: string;
  nivel?: string;
  estado: EntityStatus;
}

export interface CicloEscolar {
  id: number;
  publicId: string;
  nombre: string;
  fechaInicio: string;
  fechaFin: string;
  estado: EntityStatus;
}

export interface Periodo {
  id: number;
  publicId: string;
  cicloEscolarId: number;
  cicloEscolarNombre: string;
  nombre: string;
  fechaInicio: string;
  fechaFin: string;
  estado: EntityStatus;
}

export interface Academia {
  id: number;
  publicId: string;
  nombre: string;
  descripcion?: string;
  estado: EntityStatus;
  totalUsuarios: number;
  totalAsignaturas: number;
}

export interface Asignatura {
  id: number;
  publicId: string;
  academiaId?: number;
  academiaNombre?: string;
  nombre: string;
  clave: string;
  cuatrimestre: string;
  horasTotales: number;
  horasSemana: number;
  creditos: number;
  estado: EntityStatus;
}

export interface Grupo {
  id: number;
  publicId: string;
  nombre: string;
  cuatrimestre: string;
  carreraId: number;
  carreraNombre: string;
  periodoId: number;
  periodoNombre: string;
  estado: EntityStatus;
}

export interface CargaAcademica {
  id: number;
  publicId: string;
  periodoId: number;
  periodoNombre: string;
  grupoId: number;
  grupoNombre: string;
  asignaturaId: number;
  asignaturaNombre: string;
  docenteId: number;
  docenteNombre: string;
  revisorId?: number;
  revisorNombre?: string;
  academiaId?: number;
  academiaNombre?: string;
  estado: EntityStatus;
}
