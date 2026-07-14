import { NgClass } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import {
  LucideDynamicIcon,
  LucideHouse,
  LucideFileText,
  LucideChartColumn,
  LucideSquareCheckBig,
  LucideUsers,
  LucideUpload,
  LucideSchool,
  LucideGrid2x2,
  LucideImage,
  LucideUserPlus,
  LucideLogOut,
  LucideBookOpen,
  LucideShieldCheck,
  LucideDatabase,
  LucidePanelLeftClose,
  LucidePanelLeftOpen
} from '@lucide/angular';

import { AuthService } from '../../core/services/auth.service';

interface MenuItem {
  label: string;
  route: string;
  icon: any;
}

interface MenuSection {
  title: string;
  icon: any;
  items: MenuItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    NgClass,
    RouterLink,
    RouterLinkActive,
    LucideDynamicIcon
  ],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar {
  private authService = inject(AuthService);

  user = this.authService.currentUser;

  collapsed = signal(false);

  closeIcon = LucidePanelLeftClose;
  openIcon = LucidePanelLeftOpen;
  logoutIcon = LucideLogOut;

  roleLabel = computed(() => {
    const role = this.user()?.role;

    if (role === 'ADMIN') return 'Administrador';
    if (role === 'REVISOR') return 'Revisor';
    return 'Docente';
  });

  sections = computed<MenuSection[]>(() => {
    const currentUser = this.user();

    if (!currentUser) {
      return [];
    }

    if (currentUser.role === 'ADMIN') {
      return [
        {
          title: 'Administración',
          icon: LucideShieldCheck,
          items: [
            {
              label: 'Usuarios',
              route: '/usuarios',
              icon: LucideUsers
            },
            {
              label: 'Importación de Academias',
              route: '/importacion-academica',
              icon: LucideUpload
            }
          ]
        },
        {
          title: 'Carga Académica',
          icon: LucideDatabase,
          items: [
            {
              label: 'Academias',
              route: '/academias',
              icon: LucideSchool
            },
            {
              label: 'Grupos',
              route: '/grupos',
              icon: LucideGrid2x2
            },
            {
              label: 'Asignación Académica',
              route: '/asignacion-academica',
              icon: LucideImage
            },
            {
              label: 'Importar Profesores',
              route: '/importar-profesores',
              icon: LucideUserPlus
            }
          ]
        }
      ];
    }

    if (currentUser.role === 'REVISOR') {
      return [
        {
          title: 'Docente',
          icon: LucideBookOpen,
          items: [
            {
              label: 'Dashboard',
              route: '/dashboard',
              icon: LucideHouse
            },
            {
              label: 'Planeaciones',
              route: '/planeaciones',
              icon: LucideFileText
            },
            {
              label: 'Reportes',
              route: '/reportes',
              icon: LucideChartColumn
            }
          ]
        },
        {
          title: 'Validaciones',
          icon: LucideSquareCheckBig,
          items: [
            {
              label: 'Validación',
              route: '/validacion',
              icon: LucideSquareCheckBig
            },
            {
              label: 'Reporte de Validaciones',
              route: '/validacion/reporte',
              icon: LucideChartColumn
            }
          ]
        }
      ];
    }

    return [
      {
        title: 'Docente',
        icon: LucideBookOpen,
        items: [
          {
            label: 'Dashboard',
            route: '/dashboard',
            icon: LucideHouse
          },
          {
            label: 'Planeaciones',
            route: '/planeaciones',
            icon: LucideFileText
          },
          {
            label: 'Reportes',
            route: '/reportes',
            icon: LucideChartColumn
          }
        ]
      }
    ];
  });

  toggleSidebar(): void {
    this.collapsed.update(value => !value);
  }

  logout(): void {
    this.authService.logout();
  }
}