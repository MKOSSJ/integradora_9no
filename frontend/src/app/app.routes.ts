import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'auth/login',
    pathMatch: 'full',
  },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'auth/recuperar-password',
    loadComponent: () =>
      import('./features/auth/recover-password/recover-password').then((m) => m.RecoverPassword),
  },
  {
    path: 'login',
    redirectTo: 'auth/login',
    pathMatch: 'full',
  },
  {
    path: 'auth/verificar-codigo',
    loadComponent: () =>
      import('./features/auth/verify-code/verify-code').then((m) => m.VerifyCode),
  },
  {
    path: 'auth/nueva-password',
    loadComponent: () =>
      import('./features/auth/new-password/new-password').then((m) => m.NewPassword),
  },
  {
    path: '',
    loadComponent: () => import('./layout/main-layout/main-layout').then((m) => m.MainLayout),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'planeaciones',
        loadComponent: () =>
          import('./features/planeaciones/pages/planeaciones-list/planeaciones-list').then(
            (m) => m.PlaneacionesList,
          ),
      },
      {
        path: 'planeaciones/:id',
        loadComponent: () =>
          import('./features/planeaciones/pages/planeacion-detail/planeacion-detail').then(
            (m) => m.PlaneacionDetailPage,
          ),
      },
      {
        path: 'reportes',
        loadComponent: () => import('./features/reportes/reportes').then((m) => m.Reportes),
      },

      /* Validación */
      {
        path: 'validacion',
        loadComponent: () =>
          import('./features/validacion/pages/validacion-list/validacion-list').then(
            (m) => m.ValidacionList,
          ),
      },
      {
        path: 'validacion/reporte',
        loadComponent: () =>
          import('./features/validacion/pages/reporte-validaciones/reporte-validaciones').then(
            (m) => m.ReporteValidaciones,
          ),
      },
      {
        path: 'validacion/:id',
        loadComponent: () =>
          import('./features/validacion/pages/validacion-detail/validacion-detail').then(
            (m) => m.ValidacionDetail,
          ),
      },

      /* Administración */
      {
        path: 'usuarios',
        loadComponent: () =>
          import('./features/admin/pages/usuarios/usuarios').then((m) => m.Usuarios),
      },
      {
        path: 'carreras',
        loadComponent: () =>
          import('./features/admin/pages/carreras/carreras').then((m) => m.Carreras),
      },
      {
        path: 'asignaturas',
        loadComponent: () =>
          import('./features/admin/pages/asignaturas/asignaturas').then((m) => m.Asignaturas),
      },
      {
        path: 'periodos',
        loadComponent: () =>
          import('./features/admin/pages/periodos/periodos').then((m) => m.Periodos),
      },
      {
        path: 'importacion-academica',
        loadComponent: () =>
          import('./features/admin/pages/importacion-academias/importacion-academias').then(
            (m) => m.ImportacionAcademias,
          ),
      },

      /* Carga Académica */
      {
        path: 'academias',
        loadComponent: () =>
          import('./features/admin/pages/academias/academias').then((m) => m.Academias),
      },
      {
        path: 'grupos',
        loadComponent: () => import('./features/admin/pages/grupos/grupos').then((m) => m.Grupos),
      },
      {
        path: 'asignacion-academica',
        loadComponent: () =>
          import('./features/admin/pages/asignacion-academica/asignacion-academica').then(
            (m) => m.AsignacionAcademica,
          ),
      },
      {
        path: 'importar-profesores',
        loadComponent: () =>
          import('./features/admin/pages/importar-profesores/importar-profesores').then(
            (m) => m.ImportarProfesores,
          ),
      },

      /* Administración / Directivo */
      {
        path: 'seguimiento-planeaciones',
        loadComponent: () =>
          import('./features/admin/pages/seguimiento-planeaciones/seguimiento-planeaciones').then(
            (m) => m.SeguimientoPlaneaciones,
          ),
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'auth/login',
  },
];
