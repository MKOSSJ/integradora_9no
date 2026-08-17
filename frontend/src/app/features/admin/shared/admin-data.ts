export const CARRERAS = [
  { id: 1, publicId: 'carrera-ti', nombre: 'Ingeniería en Tecnologías de la Información', clave: 'ITI', nivel: 'Ingeniería', estado: 'activo' },
  { id: 2, publicId: 'carrera-industrial', nombre: 'Ingeniería Industrial', clave: 'IND', nivel: 'Ingeniería', estado: 'activo' },
  { id: 3, publicId: 'carrera-mecatronica', nombre: 'Ingeniería Mecatrónica', clave: 'MEC', nivel: 'Ingeniería', estado: 'inactivo' }
];

export const CICLOS = [
  { id: 1, publicId: 'ciclo-2026', nombre: 'Ciclo Escolar 2026', fechaInicio: '2026-01-01', fechaFin: '2026-12-31', estado: 'activo' },
  { id: 2, publicId: 'ciclo-2025', nombre: 'Ciclo Escolar 2025', fechaInicio: '2025-01-01', fechaFin: '2025-12-31', estado: 'inactivo' }
];

export const PERIODOS = [
  { id: 1, publicId: 'periodo-ene-abr-2026', cicloEscolarId: 1, cicloEscolarNombre: 'Ciclo Escolar 2026', nombre: 'Enero - Abril 2026', fechaInicio: '2026-01-05', fechaFin: '2026-04-25', estado: 'activo' },
  { id: 2, publicId: 'periodo-may-ago-2026', cicloEscolarId: 1, cicloEscolarNombre: 'Ciclo Escolar 2026', nombre: 'Mayo - Agosto 2026', fechaInicio: '2026-05-04', fechaFin: '2026-08-21', estado: 'activo' }
];

export const ACADEMIAS = [
  { id: 1, publicId: 'academia-software', nombre: 'Academia de Desarrollo de Software', descripcion: 'Área enfocada en programación y diseño de software.', estado: 'activo', totalUsuarios: 8, totalAsignaturas: 5 },
  { id: 2, publicId: 'academia-bd', nombre: 'Academia de Bases de Datos', descripcion: 'Área responsable de bases de datos y modelado.', estado: 'activo', totalUsuarios: 6, totalAsignaturas: 4 },
  { id: 3, publicId: 'academia-matematicas', nombre: 'Academia de Matemáticas', descripcion: 'Área de formación matemática.', estado: 'inactivo', totalUsuarios: 4, totalAsignaturas: 3 }
];

export const USUARIOS = [
  { id: 1, publicId: 'usuario-admin', nombre: 'Administrador', apellidoPaterno: 'Sistema', apellidoMaterno: '', email: 'admin@uth.edu.mx', telefono: '7710000000', roles: ['DIRECTIVO'], academiaNombre: '', rolEnAcademia: '', estado: 'activo', ultimoAcceso: '2026-07-11' },
  { id: 2, publicId: 'usuario-carlos', nombre: 'Carlos', apellidoPaterno: 'Pérez', apellidoMaterno: 'López', email: 'carlos.perez@uth.edu.mx', telefono: '7711234567', roles: ['DOCENTE'], academiaNombre: 'Academia de Desarrollo de Software', rolEnAcademia: 'Docente', estado: 'activo', ultimoAcceso: '2026-07-10' },
  { id: 3, publicId: 'usuario-maria', nombre: 'María', apellidoPaterno: 'González', apellidoMaterno: 'Ruiz', email: 'maria.gonzalez@uth.edu.mx', telefono: '7714567890', roles: ['DOCENTE', 'REVISOR'], academiaNombre: 'Academia de Bases de Datos', rolEnAcademia: 'Revisor', estado: 'activo', ultimoAcceso: '2026-07-09' },
  { id: 4, publicId: 'usuario-directivo', nombre: 'Laura', apellidoPaterno: 'Sánchez', apellidoMaterno: 'Mora', email: 'laura.sanchez@uth.edu.mx', telefono: '7719876543', roles: ['DIRECTIVO'], academiaNombre: '', rolEnAcademia: '', estado: 'activo', ultimoAcceso: '2026-07-08' }
];

export const ASIGNATURAS = [
  { id: 1, publicId: 'asignatura-web', academiaId: 1, academiaNombre: 'Academia de Desarrollo de Software', nombre: 'Programación Web', clave: 'PW-501', cuatrimestre: '5°', horasTotales: 105, horasSemana: 7, creditos: 6.5, estado: 'activo' },
  { id: 2, publicId: 'asignatura-bd', academiaId: 2, academiaNombre: 'Academia de Bases de Datos', nombre: 'Bases de Datos', clave: 'BD-301', cuatrimestre: '3°', horasTotales: 90, horasSemana: 6, creditos: 5.5, estado: 'activo' },
  { id: 3, publicId: 'asignatura-mat', academiaId: 3, academiaNombre: 'Academia de Matemáticas', nombre: 'Matemáticas', clave: 'MAT-101', cuatrimestre: '1°', horasTotales: 105, horasSemana: 7, creditos: 6.5, estado: 'inactivo' }
];

export const GRUPOS = [
  { id: 1, publicId: 'grupo-ti-301', nombre: 'TI-301', cuatrimestre: '3°', carreraId: 1, carreraNombre: 'Ingeniería en Tecnologías de la Información', periodoId: 1, periodoNombre: 'Enero - Abril 2026', estado: 'activo' },
  { id: 2, publicId: 'grupo-ti-502', nombre: 'TI-502', cuatrimestre: '5°', carreraId: 1, carreraNombre: 'Ingeniería en Tecnologías de la Información', periodoId: 1, periodoNombre: 'Enero - Abril 2026', estado: 'activo' },
  { id: 3, publicId: 'grupo-ind-401', nombre: 'IND-401', cuatrimestre: '4°', carreraId: 2, carreraNombre: 'Ingeniería Industrial', periodoId: 2, periodoNombre: 'Mayo - Agosto 2026', estado: 'inactivo' }
];

export const CARGA_ACADEMICA = [
  { id: 1, publicId: 'carga-web-ti502', periodoId: 1, periodoNombre: 'Enero - Abril 2026', grupoId: 2, grupoNombre: 'TI-502', asignaturaId: 1, asignaturaNombre: 'Programación Web', docenteId: 2, docenteNombre: 'Carlos Pérez López', revisorId: 3, revisorNombre: 'María González Ruiz', academiaId: 1, academiaNombre: 'Academia de Desarrollo de Software', estado: 'activo' },
  { id: 2, publicId: 'carga-bd-ti301', periodoId: 1, periodoNombre: 'Enero - Abril 2026', grupoId: 1, grupoNombre: 'TI-301', asignaturaId: 2, asignaturaNombre: 'Bases de Datos', docenteId: 3, docenteNombre: 'María González Ruiz', revisorId: 4, revisorNombre: 'Laura Sánchez Mora', academiaId: 2, academiaNombre: 'Academia de Bases de Datos', estado: 'activo' }
];

export const STATUS_OPTIONS = [
  { label: 'Activo', value: 'activo' },
  { label: 'Inactivo', value: 'inactivo' }
];

export const ROLE_OPTIONS = [
  { label: 'Docente', value: 'DOCENTE' },
  { label: 'Revisor', value: 'REVISOR' },
  { label: 'Directivo', value: 'DIRECTIVO' }
];

export const ROL_ACADEMIA_OPTIONS = [
  { label: 'Docente', value: 'Docente' },
  { label: 'Revisor', value: 'Revisor' }
];
