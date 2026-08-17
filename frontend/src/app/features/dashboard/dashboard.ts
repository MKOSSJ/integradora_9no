import { NgClass } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import {
  LucideDynamicIcon,
  LucideLayoutDashboard,
  LucideFileText,
  LucideChartColumn,
  LucideClock3,
  LucideCircleCheckBig,
  LucideAlertCircle,
  LucideUsers,
  LucideBookOpen,
  LucideClipboardCheck,
  LucideCalendarDays,
  LucideUploadCloud,
  LucideSchool,
  LucideArrowRight,
  LucideTrendingUp
} from '@lucide/angular';

import { AuthService } from '../../core/services/auth.service';

interface StatCard {
  title: string;
  value: string;
  description: string;
  icon: any;
  tone: 'teal' | 'blue' | 'amber' | 'green' | 'rose';
}

interface QuickAction {
  title: string;
  description: string;
  route: string;
  icon: any;
}

interface ActivityItem {
  title: string;
  description: string;
  time: string;
  status: 'success' | 'warning' | 'info';
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    NgClass,
    RouterLink,
    LucideDynamicIcon
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {
  private authService = inject(AuthService);

  user = this.authService.currentUser;

  dashboardIcon = LucideLayoutDashboard;
  arrowIcon = LucideArrowRight;
  trendingIcon = LucideTrendingUp;

  roleLabel = computed(() => {
    const role = this.user()?.role;

    if (role === 'DIRECTIVO') return 'Directivo';
    if (role === 'REVISOR') return 'Revisor';
    return 'Docente';
  });

  welcomeTitle = computed(() => {
    const role = this.user()?.role;

    if (role === 'DIRECTIVO') return 'Panel de administración';
    if (role === 'REVISOR') return 'Panel de revisión académica';
    return 'Panel docente';
  });

  welcomeDescription = computed(() => {
    const role = this.user()?.role;

    if (role === 'DIRECTIVO') {
      return 'Gestiona usuarios, academias, grupos y carga académica desde un solo lugar.';
    }

    if (role === 'REVISOR') {
      return 'Consulta planeaciones asignadas, revisa avances y genera reportes de validación.';
    }

    return 'Consulta tus planeaciones, da seguimiento a tus actividades y revisa tus reportes.';
  });

  statCards = computed<StatCard[]>(() => {
    const role = this.user()?.role;

    if (role === 'DIRECTIVO') {
      return [
        {
          title: 'Usuarios registrados',
          value: '128',
          description: 'Docentes, revisores y administradores',
          icon: LucideUsers,
          tone: 'teal'
        },
        {
          title: 'Academias',
          value: '12',
          description: 'Academias activas en el sistema',
          icon: LucideSchool,
          tone: 'blue'
        },
        {
          title: 'Grupos activos',
          value: '36',
          description: 'Grupos con carga académica',
          icon: LucideBookOpen,
          tone: 'green'
        },
        {
          title: 'Importaciones',
          value: '8',
          description: 'Archivos procesados recientemente',
          icon: LucideUploadCloud,
          tone: 'amber'
        }
      ];
    }

    if (role === 'REVISOR') {
      return [
        {
          title: 'Planeaciones asignadas',
          value: '18',
          description: 'Pendientes de revisión',
          icon: LucideFileText,
          tone: 'teal'
        },
        {
          title: 'Validadas',
          value: '11',
          description: 'Planeaciones aprobadas',
          icon: LucideCircleCheckBig,
          tone: 'green'
        },
        {
          title: 'Con observaciones',
          value: '5',
          description: 'Requieren corrección',
          icon: LucideAlertCircle,
          tone: 'amber'
        },
        {
          title: 'Reportes',
          value: '4',
          description: 'Reportes generados',
          icon: LucideChartColumn,
          tone: 'blue'
        }
      ];
    }

    return [
      {
        title: 'Planeaciones',
        value: '6',
        description: 'Planeaciones registradas',
        icon: LucideFileText,
        tone: 'teal'
      },
      {
        title: 'Aprobadas',
        value: '3',
        description: 'Planeaciones validadas',
        icon: LucideCircleCheckBig,
        tone: 'green'
      },
      {
        title: 'Pendientes',
        value: '2',
        description: 'En espera de revisión',
        icon: LucideClock3,
        tone: 'amber'
      },
      {
        title: 'Reportes',
        value: '5',
        description: 'Reportes disponibles',
        icon: LucideChartColumn,
        tone: 'blue'
      }
    ];
  });

  quickActions = computed<QuickAction[]>(() => {
    const role = this.user()?.role;

    if (role === 'DIRECTIVO') {
      return [
        {
          title: 'Gestionar usuarios',
          description: 'Alta, edición y control de usuarios.',
          route: '/usuarios',
          icon: LucideUsers
        },
        {
          title: 'Asignación de revisores',
          description: 'Asigna revisores a las planeaciones generadas.',
          route: '/asignacion-revisores',
          icon: LucideClipboardCheck
        },
        {
          title: 'Importaciones',
          description: 'Importa programas y genera planeaciones.',
          route: '/importaciones',
          icon: LucideUploadCloud
        }
      ];
    }

    if (role === 'REVISOR') {
      return [
        {
          title: 'Validar planeaciones',
          description: 'Revisa documentos enviados por docentes.',
          route: '/validacion',
          icon: LucideClipboardCheck
        },
        {
          title: 'Ver planeaciones',
          description: 'Consulta tus planeaciones asignadas.',
          route: '/planeaciones',
          icon: LucideFileText
        },
        {
          title: 'Reporte de validaciones',
          description: 'Consulta el avance de revisión.',
          route: '/reporte-validaciones',
          icon: LucideChartColumn
        }
      ];
    }

    return [
      {
        title: 'Mis planeaciones',
        description: 'Consulta y administra tus planeaciones.',
        route: '/planeaciones',
        icon: LucideFileText
      },
      {
        title: 'Reportes',
        description: 'Revisa el estado de tus documentos.',
        route: '/reportes',
        icon: LucideChartColumn
      },
      {
        title: 'Calendario académico',
        description: 'Consulta fechas importantes.',
        route: '/dashboard',
        icon: LucideCalendarDays
      }
    ];
  });

  recentActivity = computed<ActivityItem[]>(() => {
    const role = this.user()?.role;

    if (role === 'DIRECTIVO') {
      return [
        {
          title: 'Carga académica actualizada',
          description: 'Se asignaron nuevos grupos a docentes.',
          time: 'Hace 10 minutos',
          status: 'success'
        },
        {
          title: 'Importación pendiente',
          description: 'Hay un archivo de profesores por revisar.',
          time: 'Hace 35 minutos',
          status: 'warning'
        },
        {
          title: 'Nuevo usuario registrado',
          description: 'Se agregó un nuevo docente al sistema.',
          time: 'Hace 1 hora',
          status: 'info'
        }
      ];
    }

    if (role === 'REVISOR') {
      return [
        {
          title: 'Planeación recibida',
          description: 'Un docente envió una planeación para revisión.',
          time: 'Hace 12 minutos',
          status: 'info'
        },
        {
          title: 'Validación completada',
          description: 'Se aprobó una planeación didáctica.',
          time: 'Hace 45 minutos',
          status: 'success'
        },
        {
          title: 'Observaciones enviadas',
          description: 'Se solicitaron correcciones a una planeación.',
          time: 'Hace 2 horas',
          status: 'warning'
        }
      ];
    }

    return [
      {
        title: 'Planeación actualizada',
        description: 'Se guardaron cambios en una planeación.',
        time: 'Hace 15 minutos',
        status: 'success'
      },
      {
        title: 'Revisión pendiente',
        description: 'Una planeación está en espera de validación.',
        time: 'Hace 1 hora',
        status: 'warning'
      },
      {
        title: 'Reporte generado',
        description: 'Se generó un reporte académico.',
        time: 'Hace 3 horas',
        status: 'info'
      }
    ];
  });
}
