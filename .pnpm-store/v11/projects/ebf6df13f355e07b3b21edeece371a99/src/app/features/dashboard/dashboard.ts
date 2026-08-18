import { NgClass } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
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
import {
  ResumenDashboard,
  ResumenDashboardDocente,
  ResumenDashboardResponse,
  ResumenDashboardRevisor,
  ResumenService
} from '../../core/services/resumen.service';

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
  private readonly resumenService = inject(ResumenService);

  user = this.authService.currentUser;
  summary = signal<ResumenDashboardResponse | null>(null);
  summaryError = signal('');

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

  constructor() {
    const role = this.user()?.role;

    if (!role) return;

    this.resumenService.dashboard(role).subscribe({
      next: summary => this.summary.set(summary),
      error: error => this.summaryError.set(
        error instanceof Error && error.message.trim()
          ? error.message
          : 'No fue posible cargar el resumen del dashboard.'
      )
    });
  }

  statCards = computed<StatCard[]>(() => {
    const role = this.user()?.role;
    const summary = this.summary();

    if (!role || !summary) return [];

    if (role === 'DIRECTIVO') {
      const director = summary as ResumenDashboard;
      return [
        {
          title: 'Usuarios registrados',
          value: String(director.usuariosRegistrados),
          description: 'Docentes, revisores y administradores',
          icon: LucideUsers,
          tone: 'teal'
        },
        {
          title: 'Academias',
          value: String(director.academias),
          description: 'Academias activas en el sistema',
          icon: LucideSchool,
          tone: 'blue'
        },
        {
          title: 'Grupos activos',
          value: String(director.gruposActivos),
          description: 'Grupos con carga académica',
          icon: LucideBookOpen,
          tone: 'green'
        },
        {
          title: 'Importaciones',
          value: String(director.importaciones),
          description: 'Archivos procesados recientemente',
          icon: LucideUploadCloud,
          tone: 'amber'
        }
      ];
    }

    if (role === 'REVISOR') {
      const revisor = summary as ResumenDashboardRevisor;
      return [
        {
          title: 'Planeaciones asignadas',
          value: String(revisor.planeaciones),
          description: 'Pendientes de revisión',
          icon: LucideFileText,
          tone: 'teal'
        },
        {
          title: 'Validadas',
          value: String(revisor.validadas),
          description: 'Planeaciones aprobadas',
          icon: LucideCircleCheckBig,
          tone: 'green'
        },
        {
          title: 'Con observaciones',
          value: String(revisor.correcciones),
          description: 'Requieren corrección',
          icon: LucideAlertCircle,
          tone: 'amber'
        },
      ];
    }

    const docente = summary as ResumenDashboardDocente;
    return [
      {
        title: 'Planeaciones',
        value: String(docente.planeaciones),
        description: 'Planeaciones registradas',
        icon: LucideFileText,
        tone: 'teal'
      },
      {
        title: 'Aprobadas',
        value: String(docente.aprobadas),
        description: 'Planeaciones validadas',
        icon: LucideCircleCheckBig,
        tone: 'green'
      },
      {
        title: 'Pendientes',
        value: String(docente.pendientes),
        description: 'En espera de revisión',
        icon: LucideClock3,
        tone: 'amber'
      },
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

  readonly recentActivity = signal<ActivityItem[]>([]);
}
