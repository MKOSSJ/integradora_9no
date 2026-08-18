import { DatePipe, NgClass } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {LucideDynamicIcon,LucideDownload,
  LucideSearch,
  LucideCalendarDays,
  LucideSlidersHorizontal,
  LucideChevronLeft,
  LucideChevronRight,
  LucideFileText,
  LucideCircleCheckBig,
  LucideClock3,
  LucideAlertTriangle,
  LucideTrendingUp,
  LucideArrowUpDown,
} from '@lucide/angular';
import { ReportePlaneacion, ReportStatus, RepositorioService } from '../../core/services/repositorio.service';

@Component({
  selector: 'app-reportes',
  standalone: true,
  imports: [NgClass, FormsModule, DatePipe, LucideDynamicIcon],
  templateUrl: './reportes.html',
  styleUrl: './reportes.css',
})
export class Reportes {
  private readonly repositorioService = inject(RepositorioService);
  search = signal('');
  statusFilter = signal<'todos' | ReportStatus>('todos');
  sortBy = signal<'recent' | 'progress' | 'title'>('recent');

  downloadIcon = LucideDownload;
  searchIcon = LucideSearch;
  calendarIcon = LucideCalendarDays;
  filterIcon = LucideSlidersHorizontal;
  prevIcon = LucideChevronLeft;
  nextIcon = LucideChevronRight;
  reportIcon = LucideFileText;
  checkIcon = LucideCircleCheckBig;
  clockIcon = LucideClock3;
  alertIcon = LucideAlertTriangle;
  progressIcon = LucideTrendingUp;
  sortIcon = LucideArrowUpDown;

  reports = signal<ReportePlaneacion[]>([]);
  loadError = signal('');

  constructor() {
    this.repositorioService.loadAll().subscribe({
      next: reports => this.reports.set(reports),
      error: error => this.loadError.set(
        error instanceof Error && error.message.trim()
          ? error.message
          : 'No fue posible cargar el repositorio de planeaciones.'
      )
    });
  }

  filteredReports = computed(() => {
    const query = this.search().trim().toLowerCase();
    const status = this.statusFilter();

    let items = [...this.reports()];

    if (query) {
      items = items.filter(
        (item) =>
          item.titulo.toLowerCase().includes(query) ||
          item.autor.toLowerCase().includes(query) ||
          item.carrera.toLowerCase().includes(query) ||
          item.grupo.toLowerCase().includes(query),
      );
    }

    if (status !== 'todos') {
      items = items.filter((item) => item.estado === status);
    }

    if (this.sortBy() === 'title') {
      items.sort((a, b) => a.titulo.localeCompare(b.titulo));
    }

    if (this.sortBy() === 'progress') {
      items.sort((a, b) => b.progreso - a.progreso);
    }

    if (this.sortBy() === 'recent') {
      items.sort((a, b) => new Date(b.fecha).getTime() - new Date(a.fecha).getTime());
    }

    return items;
  });

  counters = computed(() => {
    const items = this.reports();

    return {
      total: items.length,
      validadas: items.filter(item => item.estado === 'validada').length,
      revision: items.filter(item => item.estado === 'revision').length,
      progreso: items.filter(item => item.estado === 'elaboracion').length,
    };
  });

  approvalRate = computed(() => {
    const { total, validadas } = this.counters();
    return total === 0 ? 0 : Math.round((validadas / total) * 100);
  });

  getStatusLabel(status: ReportStatus): string {
    if (status === 'validada') return 'Validada';
    if (status === 'enviada') return 'Enviada';
    if (status === 'elaboracion') return 'En elaboración';
    if (status === 'observada') return 'Observada';
    return 'En revisión';
  }

  getStatusClasses(status: ReportStatus): string {
    if (status === 'validada') return 'bg-green-100 text-green-700 ring-green-200';
    if (status === 'enviada') return 'bg-amber-100 text-amber-700 ring-amber-200';
    if (status === 'elaboracion') return 'bg-slate-100 text-slate-700 ring-slate-200';
    if (status === 'observada') return 'bg-red-100 text-red-700 ring-red-200';
    return 'bg-cyan-100 text-cyan-700 ring-cyan-200';
  }

  getProgressColor(progress: number): string {
    if (progress >= 90) return 'from-teal-500 to-cyan-500';
    if (progress >= 70) return 'from-amber-500 to-orange-500';
    return 'from-slate-400 to-slate-500';
  }

  exportReport(): void {
    this.loadError.set('La API no expone una exportación consolidada de reportes.');
  }
}
