import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

import {
  LucideDynamicIcon,
  LucideLayoutDashboard,
  LucideFileText,
  LucideChartBar,
  LucideShieldCheck,
  LucideUsers,
  LucideUpload,
  LucideGraduationCap,
  LucideLayers3,
  LucideClipboardList,
  LucideSchool,
  LucideBookOpen,
  LucideCalendarDays,
  LucidePanelLeftClose,
  LucidePanelLeftOpen,
  LucideLogOut,
  LucideSettings,
} from '@lucide/angular';

import { AuthService, UserRole } from '../../core/services/auth.service';

interface SidebarItem {
  label: string;
  route: string;
  icon: any;
}

interface SidebarSection {
  title: string;
  icon: any;
  roles: UserRole[];
  items: SidebarItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, LucideDynamicIcon],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  collapsed = signal(false);

  closeIcon = LucidePanelLeftClose;
  openIcon = LucidePanelLeftOpen;
  logoutIcon = LucideLogOut;

  user = computed(() => this.authService.currentUser());

  role = computed<UserRole | null>(() => this.user()?.role ?? null);

  roleLabel = computed(() => {
    const currentRole = this.role();

    if (currentRole === 'DIRECTIVO') {
      return 'Directivo';
    }

    if (currentRole === 'REVISOR') {
      return 'Revisor';
    }

    if (currentRole === 'DOCENTE') {
      return 'Docente';
    }

    return '';
  });

  private readonly allSections: SidebarSection[] = [
    {
      title: 'Planeaciones',
      icon: LucideFileText,
      roles: ['DOCENTE', 'REVISOR'],
      items: [
        {
          label: 'Dashboard',
          route: '/dashboard',
          icon: LucideLayoutDashboard,
        },
        {
          label: 'Planeaciones',
          route: '/planeaciones',
          icon: LucideFileText,
        },
        {
          label: 'Reportes',
          route: '/reportes',
          icon: LucideChartBar,
        },
      ],
    },

    {
      title: 'Validaciones',
      icon: LucideShieldCheck,
      roles: ['REVISOR'],
      items: [
        {
          label: 'Validación',
          route: '/validacion',
          icon: LucideShieldCheck,
        },
        {
          label: 'Reporte de Validaciones',
          route: '/validacion/reporte',
          icon: LucideChartBar,
        },
      ],
    },

    {
      title: 'Administración',
      icon: LucideSettings,
      roles: ['DIRECTIVO'],
      items: [
        {
          label: 'Usuarios',
          route: '/usuarios',
          icon: LucideUsers,
        },
        {
          label: 'Carreras',
          route: '/carreras',
          icon: LucideSchool,
        },
        {
          label: 'Asignaturas',
          route: '/asignaturas',
          icon: LucideBookOpen,
        },
        {
          label: 'Ciclos y Periodos',
          route: '/ciclos',
          icon: LucideCalendarDays,
        },
        {
          label: 'Academias',
          route: '/academias',
          icon: LucideGraduationCap,
        },
        {
          label: 'Grupos',
          route: '/grupos',
          icon: LucideLayers3,
        },
        {
          label: 'Importaciones',
          route: '/importaciones',
          icon: LucideUpload,
        },
        {
          label: 'Programas de asignatura',
          route: '/programas-asignatura',
          icon: LucideFileText,
        },
        {
          label: 'Asignación de Revisores',
          route: '/asignacion-revisores',
          icon: LucideClipboardList,
        },
        {
          label: 'Seguimiento de planeaciones',
          route: '/seguimiento-planeaciones',
          icon: LucideChartBar,
        },
      ],
    },
  ];

  sections = computed(() => {
    const currentRoles = this.user()?.roles ?? [];

    return this.allSections.filter((section) =>
      section.roles.some((role) => currentRoles.includes(role)),
    );
  });

  toggleSidebar(): void {
    this.collapsed.update((value) => !value);
  }

  logout(): void {
    this.authService.logout(); 

    this.router.navigateByUrl('/auth/login', {
      replaceUrl: true,
    });
  }
} 
