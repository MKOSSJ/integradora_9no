import { DatePipe, NgClass } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
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

type ReportStatus = 'validada' | 'enviada' | 'elaboracion' | 'observada' | 'revision';

interface ReportItem {
  id: number;
  titulo: string;
  autor: string;
  estado: ReportStatus;
  progreso: number;
  fecha: string;
  carrera: string;
  grupo: string;
}

@Component({
  selector: 'app-reportes',
  standalone: true,
  imports: [NgClass, FormsModule, DatePipe, LucideDynamicIcon],
  templateUrl: './reportes.html',
  styleUrl: './reportes.css',
})
export class Reportes {
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

  reports = signal<ReportItem[]>([
    {
      id: 1,
      titulo: 'Matemáticas Básicas — Álgebra',
      autor: 'Carlos Pérez',
      estado: 'validada',
      progreso: 100,
      fecha: '2024-03-14',
      carrera: 'Ingeniería en TI',
      grupo: 'TI-301',
    },
    {
      id: 2,
      titulo: 'Física Avanzada — Mecánica',
      autor: 'María González',
      estado: 'enviada',
      progreso: 100,
      fecha: '2024-03-13',
      carrera: 'Ingeniería Industrial',
      grupo: 'IND-401',
    },
    {
      id: 3,
      titulo: 'Química Orgánica — Hidrocarburos',
      autor: 'Juan Martínez',
      estado: 'elaboracion',
      progreso: 60,
      fecha: '2024-03-11',
      carrera: 'Ingeniería Química',
      grupo: 'QUI-302',
    },
    {
      id: 4,
      titulo: 'Biología — Genética Mendeliana',
      autor: 'Ana López',
      estado: 'validada',
      progreso: 100,
      fecha: '2024-03-09',
      carrera: 'Biotecnología',
      grupo: 'BIO-201',
    },
    {
      id: 5,
      titulo: 'Historia Universal — Edad Media',
      autor: 'Pedro Ramírez',
      estado: 'observada',
      progreso: 80,
      fecha: '2024-03-07',
      carrera: 'Educación',
      grupo: 'EDU-202',
    },
    {
      id: 6,
      titulo: 'Programación Web — Componentes Angular',
      autor: 'Laura Sánchez',
      estado: 'revision',
      progreso: 72,
      fecha: '2024-03-05',
      carrera: 'Ingeniería en TI',
      grupo: 'TI-502',
    },
  ]);

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
      total: 124,
      validadas: 87,
      revision: 24,
      progreso: 13,
    };
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
    console.log('Exportar reporte');
  }
}
