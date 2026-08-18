import { DatePipe, NgClass } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import {
  LucideDynamicIcon,
  LucidePlus,
  LucideSearch,
  LucideSlidersHorizontal,
  LucideArrowUpDown,
  LucideEye,
  LucidePenSquare,
} from '@lucide/angular';

import { PlaneacionesService } from '../../../../core/services/planeaciones.service';
import {
  PlaneacionListItem,
  PlaneacionStatus
} from '../../../../core/models/planeacion.model';

@Component({
  selector: 'app-planeaciones-list',
  standalone: true,
  imports: [
    NgClass,
    FormsModule,
    RouterLink,
    DatePipe,
    LucideDynamicIcon
  ],
  templateUrl: './planeaciones-list.html',
  styleUrl: './planeaciones-list.css',
})
export class PlaneacionesList {
  private planeacionesService = inject(PlaneacionesService);

  search = signal('');
  sortBy = signal<'recent' | 'title' | 'progress'>('recent');
  statusFilter = signal<'todos' | PlaneacionStatus>('todos');

  planeaciones = signal<PlaneacionListItem<string>[]>([]);

  plusIcon = LucidePlus;
  searchIcon = LucideSearch;
  filterIcon = LucideSlidersHorizontal;
  sortIcon = LucideArrowUpDown;
  eyeIcon = LucideEye;
  editIcon = LucidePenSquare;

  constructor() {
    this.planeacionesService.getPlaneaciones().subscribe((data) => {
      this.planeaciones.set(data);
    });
  }

  filteredPlaneaciones = computed(() => {
    const query = this.search().toLowerCase().trim();
    const status = this.statusFilter();

    let items = [...this.planeaciones()];

    if (query) {
      items = items.filter(
        (item) =>
          item.titulo.toLowerCase().includes(query) ||
          item.descripcion.toLowerCase().includes(query)
      );
    }

    if (status !== 'todos') {
      items = items.filter((item) => item.status === status);
    }

    if (this.sortBy() === 'title') {
      items.sort((a, b) => a.titulo.localeCompare(b.titulo));
    }

    if (this.sortBy() === 'progress') {
      items.sort((a, b) => b.progreso - a.progreso);
    }

    if (this.sortBy() === 'recent') {
      items.sort(
        (a, b) =>
          new Date(b.actualizacion).getTime() -
          new Date(a.actualizacion).getTime()
      );
    }

    return items;
  });

  counters = computed(() => {
  const items = this.planeaciones();

  return {
    total: items.length,
    aprobadas: items.filter((item) => item.status === 'aprobado').length,
    pendientes: items.filter((item) => item.status === 'pendiente').length,
    revision: items.filter((item) => item.status === 'revision').length,
    correcciones: items.filter((item) => item.status === 'correcciones').length,
    borradores: items.filter((item) => item.status === 'borrador').length,
  };
});

  canEdit(item: PlaneacionListItem<string>): boolean {
    return item.status === 'borrador' ||
      item.status === 'en-proceso' ||
      item.status === 'correcciones' ||
      item.status === 'reabierta';
  }

  getStatusLabel(status: PlaneacionStatus): string {
    if (status === 'aprobado') return 'Aprobado';
    if (status === 'borrador') return 'Borrador';
    if (status === 'en-proceso') return 'En proceso';
    if (status === 'revision') return 'En revisión';
    if (status === 'pendiente') return 'Pendiente';
    if (status === 'correcciones') return 'Correcciones';
    if (status === 'rechazada') return 'Rechazada';
    if (status === 'finalizada') return 'Finalizada';
    return 'Reabierta';
  }

  getStatusClasses(status: PlaneacionStatus): string {
    if (status === 'aprobado') {
      return 'bg-green-100 text-green-700 ring-green-200';
    }

    if (status === 'borrador') {
      return 'bg-slate-100 text-slate-700 ring-slate-200';
    }

    if (status === 'revision') {
      return 'bg-cyan-100 text-cyan-700 ring-cyan-200';
    }

    if (status === 'finalizada') {
      return 'bg-green-100 text-green-700 ring-green-200';
    }

    if (status === 'pendiente') {
      return 'bg-amber-100 text-amber-700 ring-amber-200';
    }

    return 'bg-orange-100 text-orange-700 ring-orange-200';
  }
  getBoardItems(
    group: 'edicion' | 'revision' | 'finalizadas'
  ): PlaneacionListItem<string>[] {
  const items = this.filteredPlaneaciones();

  if (group === 'edicion') {
    return items.filter(
      item => this.canEdit(item)
    );
  }

  if (group === 'revision') {
    return items.filter(
      item => item.status === 'pendiente' || item.status === 'revision'
    );
  }

  return items.filter(item =>
    item.status === 'aprobado' ||
    item.status === 'rechazada' ||
    item.status === 'finalizada'
  );
}

getStatusTone(status: PlaneacionStatus): PlaneacionStatus {
  if (status === 'en-proceso' || status === 'reabierta') return 'borrador';
  if (status === 'rechazada') return 'correcciones';
  if (status === 'finalizada') return 'aprobado';
  return status;
}
getStatusStripeClasses(status: PlaneacionStatus): string {
  if (status === 'aprobado') {
    return 'bg-green-500';
  }

  if (status === 'revision') {
    return 'bg-cyan-500';
  }

  if (status === 'pendiente') {
    return 'bg-amber-500';
  }

  if (status === 'correcciones') {
    return 'bg-orange-500';
  }

  return 'bg-teal-500';
}

getStatusDotClasses(status: PlaneacionStatus): string {
  if (status === 'aprobado') {
    return 'bg-green-500';
  }

  if (status === 'revision') {
    return 'bg-cyan-500';
  }

  if (status === 'pendiente') {
    return 'bg-amber-500';
  }

  if (status === 'correcciones') {
    return 'bg-orange-500';
  }

  return 'bg-teal-500';
}

getProgressBarClasses(status: PlaneacionStatus): string {
  if (status === 'aprobado') {
    return 'bg-green-500';
  }

  if (status === 'revision') {
    return 'bg-cyan-500';
  }

  if (status === 'pendiente') {
    return 'bg-amber-500';
  }

  if (status === 'correcciones') {
    return 'bg-orange-500';
  }

  return 'bg-teal-500';
}
}
