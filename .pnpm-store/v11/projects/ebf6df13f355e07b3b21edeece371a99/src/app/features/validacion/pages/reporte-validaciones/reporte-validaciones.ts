import { DatePipe, NgClass } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import {
  LucideDynamicIcon,
  LucideFileBarChart,
  LucideDownload,
  LucideSearch,
  LucideSlidersHorizontal,
  LucideArrowUpDown,
  LucideCircleCheckBig,
  LucideClock3,
  LucideRefreshCcw,
  LucideTriangleAlert,
  LucideEye,
  LucideChevronRight,
  LucideCalendarDays
} from '@lucide/angular';

import { ValidacionService } from '../../../../core/services/validacion.service';
import {
  RevisionItem,
  RevisionStatus
} from '../../../../core/models/validacion.model';

@Component({
  selector: 'app-reporte-validaciones',
  standalone: true,
  imports: [
    NgClass,
    DatePipe,
    FormsModule,
    RouterLink,
    LucideDynamicIcon
  ],
  templateUrl: './reporte-validaciones.html',
  styleUrl: './reporte-validaciones.css'
})
export class ReporteValidaciones {
  private validacionService = inject(ValidacionService);

  search = signal('');
  statusFilter = signal<'todos' | RevisionStatus>('todos');
  sortBy = signal<'recent' | 'title' | 'status'>('recent');

  validaciones = signal<RevisionItem[]>([]);

  reportIcon = LucideFileBarChart;
  downloadIcon = LucideDownload;
  searchIcon = LucideSearch;
  filterIcon = LucideSlidersHorizontal;
  sortIcon = LucideArrowUpDown;
  approvedIcon = LucideCircleCheckBig;
  pendingIcon = LucideClock3;
  revisionIcon = LucideRefreshCcw;
  correctionsIcon = LucideTriangleAlert;
  eyeIcon = LucideEye;
  arrowIcon = LucideChevronRight;
  calendarIcon = LucideCalendarDays;

  constructor() {
    this.validacionService.getRevisions().subscribe(data => {
      this.validaciones.set(data);
    });
  }

  filteredValidaciones = computed(() => {
    const query = this.search().trim().toLowerCase();
    const status = this.statusFilter();

    let items = [...this.validaciones()];

    if (query) {
      items = items.filter(item =>
        item.titulo.toLowerCase().includes(query) ||
        item.autor.toLowerCase().includes(query) ||
        item.carrera.toLowerCase().includes(query) ||
        item.grupo.toLowerCase().includes(query)
      );
    }

    if (status !== 'todos') {
      items = items.filter(item => item.estado === status);
    }

    if (this.sortBy() === 'title') {
      items.sort((a, b) => a.titulo.localeCompare(b.titulo));
    }

    if (this.sortBy() === 'status') {
      items.sort((a, b) => a.estado.localeCompare(b.estado));
    }

    if (this.sortBy() === 'recent') {
      items.sort(
        (a, b) =>
          new Date(b.fechaEnvio).getTime() -
          new Date(a.fechaEnvio).getTime()
      );
    }

    return items;
  });

  counters = computed(() => {
    const items = this.validaciones();

    const total = items.length;
    const aprobados = items.filter(item => item.estado === 'aprobado').length;
    const pendientes = items.filter(item => item.estado === 'pendiente').length;
    const revision = items.filter(item => item.estado === 'revision').length;
    const correcciones = items.filter(item => item.estado === 'correcciones').length;

    return {
      total,
      aprobados,
      pendientes,
      revision,
      correcciones,
      porcentajeAprobados: total === 0 ? 0 : Math.round((aprobados / total) * 100),
      porcentajePendientes: total === 0 ? 0 : Math.round((pendientes / total) * 100),
      porcentajeRevision: total === 0 ? 0 : Math.round((revision / total) * 100),
      porcentajeCorrecciones: total === 0 ? 0 : Math.round((correcciones / total) * 100)
    };
  });

  exportReport(): void {
    console.log('Exportar reporte de validaciones');
  }

  getStatusLabel(status: RevisionStatus): string {
    if (status === 'aprobado') return 'Aprobado';
    if (status === 'borrador') return 'Borrador';
    if (status === 'revision') return 'En revisión';
    if (status === 'pendiente') return 'Pendiente';
    return 'Correcciones';
  }

  getStatusClasses(status: RevisionStatus): string {
    if (status === 'aprobado') {
      return 'bg-green-100 text-green-700 ring-green-200';
    }

    if (status === 'borrador') {
      return 'bg-slate-100 text-slate-700 ring-slate-200';
    }

    if (status === 'revision') {
      return 'bg-cyan-100 text-cyan-700 ring-cyan-200';
    }

    if (status === 'pendiente') {
      return 'bg-amber-100 text-amber-700 ring-amber-200';
    }

    return 'bg-orange-100 text-orange-700 ring-orange-200';
  }

  getProgressByStatus(status: RevisionStatus): number {
    if (status === 'aprobado') return 100;
    if (status === 'revision') return 70;
    if (status === 'pendiente') return 35;
    if (status === 'correcciones') return 85;
    return 15;
  }

  getProgressClasses(status: RevisionStatus): string {
    if (status === 'aprobado') return 'from-green-500 to-emerald-500';
    if (status === 'revision') return 'from-cyan-500 to-teal-500';
    if (status === 'pendiente') return 'from-amber-500 to-orange-500';
    if (status === 'correcciones') return 'from-orange-500 to-red-500';
    return 'from-slate-400 to-slate-500';
  }
}