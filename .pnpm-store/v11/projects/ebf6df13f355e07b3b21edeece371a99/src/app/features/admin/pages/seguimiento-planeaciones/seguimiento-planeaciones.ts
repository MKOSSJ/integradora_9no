import { DatePipe, NgClass } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import {
  LucideDynamicIcon,
  LucideCalendarClock,
  LucideFileText,
  LucideEye
} from '@lucide/angular';

import {
  PlaneacionStatus,
  SeguimientoEstado,
  SeguimientoPlaneacion
} from '../../../../core/models/planeacion.model';

import { PlaneacionesService } from '../../../../core/services/planeaciones.service';

@Component({
  selector: 'app-seguimiento-planeaciones',
  standalone: true,
  imports: [
    NgClass,
    DatePipe,
    RouterLink,
    LucideDynamicIcon
  ],
  templateUrl: './seguimiento-planeaciones.html',
  styleUrl: './seguimiento-planeaciones.css'
})
export class SeguimientoPlaneaciones implements OnInit {
  private readonly planeacionesService = inject(PlaneacionesService);

  items = signal<SeguimientoPlaneacion[]>([]);
  loadError = signal('');

  calendarIcon = LucideCalendarClock;
  fileIcon = LucideFileText;
  eyeIcon = LucideEye;

  counters = computed(() => {
    const data = this.items();

    return {
      total: data.length,
      enTiempo: data.filter(item => item.estadoSeguimiento === 'en-tiempo').length,
      porVencer: data.filter(item => item.estadoSeguimiento === 'por-vencer').length,
      vencidas: data.filter(item => item.estadoSeguimiento === 'vencida').length,
      completadas: data.filter(item => item.estadoSeguimiento === 'completada').length
    };
  });

  ngOnInit(): void {
    this.planeacionesService
      .getSeguimientoDirectivo()
      .subscribe({
        next: (items: SeguimientoPlaneacion[]) => this.items.set(items),
        error: error => this.loadError.set(
          error instanceof Error && error.message.trim()
            ? error.message
            : 'No fue posible cargar el seguimiento de planeaciones.'
        )
      });
  }

  getStatusLabel(status: PlaneacionStatus): string {
    if (status === 'aprobado') return 'Aprobado';
    if (status === 'borrador') return 'Borrador';
    if (status === 'revision') return 'En revisión';
    if (status === 'pendiente') return 'Pendiente';
    return 'Correcciones';
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

    if (status === 'pendiente') {
      return 'bg-amber-100 text-amber-700 ring-amber-200';
    }

    return 'bg-orange-100 text-orange-700 ring-orange-200';
  }

  getSeguimientoLabel(status: SeguimientoEstado): string {
    if (status === 'completada') return 'Completada';
    if (status === 'vencida') return 'Vencida';
    if (status === 'por-vencer') return 'Por vencer';
    if (status === 'sin-fecha') return 'Sin fecha límite';
    return 'En tiempo';
  }

  getSeguimientoClasses(status: SeguimientoEstado): string {
    if (status === 'completada') {
      return 'bg-green-100 text-green-700 ring-green-200';
    }

    if (status === 'vencida') {
      return 'bg-red-100 text-red-700 ring-red-200';
    }

    if (status === 'por-vencer') {
      return 'bg-orange-100 text-orange-700 ring-orange-200';
    }

    if (status === 'sin-fecha') {
      return 'bg-slate-100 text-slate-600 ring-slate-200';
    }

    return 'bg-cyan-100 text-cyan-700 ring-cyan-200';
  }

  getDaysText(item: SeguimientoPlaneacion): string {
    if (item.estadoSeguimiento === 'completada') {
      return 'Finalizada';
    }

    if (item.diasRestantes === undefined) {
      return 'Sin fecha límite';
    }

    if (item.diasRestantes < 0) {
      return `${Math.abs(item.diasRestantes)} días vencida`;
    }

    if (item.diasRestantes === 0) {
      return 'Vence hoy';
    }

    return `${item.diasRestantes} días restantes`;
  }
}
