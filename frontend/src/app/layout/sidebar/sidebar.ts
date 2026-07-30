import { Component, computed, inject, signal } from '@angular/core';
import { Router,RouterLink, RouterLinkActive } from '@angular/router';

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
  LucideUserPlus,
  LucideSchool,
  LucideBookOpen,
  LucideCalendarDays,
  LucidePanelLeftClose,
  LucidePanelLeftOpen,
  LucideLogOut,
  LucideSettings
} from '@lucide/angular';

import {
  AuthService,
  UserRole
} from '../../core/services/auth.service';

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
  imports: [
    RouterLink,
    RouterLinkActive,
    LucideDynamicIcon
  ],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  collapsed = signal(false);

  closeIcon = LucidePanelLeftClose;
  openIcon = LucidePanelLeftOpen;
  logoutIcon = LucideLogOut;

  user = computed(() => this.authService.currentUser());

  role = computed<UserRole>(() => {
    return this.user()?.role ?? 'DOCENTE';
  });

  roleLabel = computed(() => {
    const currentRole = this.role();

    if (currentRole === 'ADMIN') return 'Administrador / Directivo';
    if (currentRole === 'REVISOR') return 'Revisor';

    return 'Docente';
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
          icon: LucideLayoutDashboard
        },
        {
          label: 'Planeaciones',
          route: '/planeaciones',
          icon: LucideFileText
        },
        {
          label: 'Reportes',
          route: '/reportes',
          icon: LucideChartBar
        }
      ]
    },
    {
      title: 'Validaciones',
      icon: LucideShieldCheck,
      roles: ['REVISOR'],
      items: [
        {
          label: 'Validación',
          route: '/validacion',
          icon: LucideShieldCheck
        },
        {
          label: 'Reporte de Validaciones',
          route: '/validacion/reporte',
          icon: LucideChartBar
        }
      ]
    },
    {
      title: 'Administración',
      icon: LucideSettings,
      roles: ['ADMIN'],
      items: [
        {
          label: 'Usuarios',
          route: '/usuarios',
          icon: LucideUsers
        },
        {
          label: 'Carreras',
          route: '/carreras',
          icon: LucideSchool
        },
        {
          label: 'Asignaturas',
          route: '/asignaturas',
          icon: LucideBookOpen
        },
        {
          label: 'Ciclos y Periodos',
          route: '/periodos',
          icon: LucideCalendarDays
        },
        {
          label: 'Importación de Academias',
          route: '/importacion-academica',
          icon: LucideUpload
        },
        {
          label: 'Seguimiento de Planeaciones',
          route: '/seguimiento-planeaciones',
          icon: LucideCalendarDays
        },
      ]
    },
    {
      title: 'Carga Académica',
      icon: LucideGraduationCap,
      roles: ['ADMIN'],
      items: [
        {
          label: 'Academias',
          route: '/academias',
          icon: LucideGraduationCap
        },
        {
          label: 'Grupos',
          route: '/grupos',
          icon: LucideLayers3
        },
        {
          label: 'Asignación Académica',
          route: '/asignacion-academica',
          icon: LucideClipboardList
        },
        {
          label: 'Importar Profesores',
          route: '/importar-profesores',
          icon: LucideUserPlus
        }
      ]
    },
  ];

  sections = computed(() => {
    const currentRole = this.role();

    return this.allSections.filter(section =>
      section.roles.includes(currentRole)
    );
  });

  toggleSidebar(): void {
    this.collapsed.update(value => !value);
  }

logout(): void {
  this.authService.logout();

  this.router.navigateByUrl('/auth/login', {
    replaceUrl: true
  });
}
}