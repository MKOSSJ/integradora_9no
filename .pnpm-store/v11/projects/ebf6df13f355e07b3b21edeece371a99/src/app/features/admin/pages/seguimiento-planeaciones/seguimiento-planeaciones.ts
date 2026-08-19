import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LucideDynamicIcon, LucideEye, LucideSearch } from '@lucide/angular';
import { PlaneacionResumenResponseDto } from '../../../../core/dto/planeaciones/planeaciones-directivo.dto';
import { PlaneacionesDirectivoService } from '../../../../core/services/planeaciones-directivo.service';

@Component({ selector: 'app-seguimiento-planeaciones', standalone: true, imports: [DatePipe, FormsModule, RouterLink, LucideDynamicIcon], templateUrl: './seguimiento-planeaciones.html' })
export class SeguimientoPlaneaciones {
  private readonly service = inject(PlaneacionesDirectivoService);
  items = signal<PlaneacionResumenResponseDto[]>([]); search = signal(''); estado = signal(''); error = signal('');
  searchIcon = LucideSearch; eyeIcon = LucideEye;
  filtered = computed(() => { const q = this.search().trim().toLowerCase(); const e = this.estado(); return this.items().filter(item => (!q || [item.asignatura,item.docentes,item.periodo,item.grupos,item.revisor].some(value => value?.toLowerCase().includes(q))) && (!e || String(item.estado) === e)); });
  counters = computed(() => ({ total: this.items().length, revision: this.items().filter(x => x.estado === 3).length, correcciones: this.items().filter(x => x.estado === 4).length, aprobadas: this.items().filter(x => x.estado === 5).length }));
  estados = [1,2,3,4,5,6,7,8];
  ngOnInit(): void { this.service.loadSeguimiento().subscribe({ next: items => this.items.set(items), error: () => this.error.set('No fue posible cargar las planeaciones.') }); }
  estadoLabel(value: number): string { return ({ 1:'Borrador',2:'En proceso',3:'En revisión',4:'Correcciones',5:'Aprobada',6:'Rechazada',7:'Finalizada',8:'Reabierta' } as Record<number,string>)[value] ?? 'Sin estado'; }
}
