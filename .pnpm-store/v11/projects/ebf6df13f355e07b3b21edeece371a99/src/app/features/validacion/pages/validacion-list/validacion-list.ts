import { NgClass } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  LucideDynamicIcon,
  LucideSearch,
  LucideSlidersHorizontal,
  LucideClock3,
  LucideCircleCheckBig,
  LucideTriangleAlert,
  LucideListTodo,
  LucideArrowUpDown
} from '@lucide/angular';

import {
  RevisionItem,
  RevisionStatus
} from '../../../../core/models/validacion.model';

import { ValidacionService } from '../../../../core/services/validacion.service';
import { ValidacionTable } from '../../components/validacion-table/validacion-table';

@Component({
  selector: 'app-validacion-list',
  standalone: true,
  imports: [
    NgClass,
    FormsModule,
    LucideDynamicIcon,
    ValidacionTable
  ],
  templateUrl: './validacion-list.html',
  styleUrl: './validacion-list.css'
})
export class ValidacionList {
  private validacionService = inject(ValidacionService);

  search = signal('');
  statusFilter = signal<'todos' | RevisionStatus>('todos');
  sortBy = signal<'recent' | 'title'>('recent');

  revisions = signal<RevisionItem[]>([]);

  searchIcon = LucideSearch;
  filterIcon = LucideSlidersHorizontal;
  pendingIcon = LucideClock3;
  approvedIcon = LucideCircleCheckBig;
  correctionsIcon = LucideTriangleAlert;
  listIcon = LucideListTodo;
  sortIcon = LucideArrowUpDown;

  constructor() {
    this.validacionService.getRevisions().subscribe(data => {
      this.revisions.set(data);
    });
  }

  counters = computed(() => {
    const data = this.revisions();

    return {
      pendientes: data.filter(item => item.estado === 'pendiente').length,
      aprobadas: data.filter(item => item.estado === 'aprobado').length,
      correcciones: data.filter(item => item.estado === 'correcciones').length
    };
  });

  filteredRevisions = computed(() => {
    const query = this.search().trim().toLowerCase();
    const status = this.statusFilter();

    let items = [...this.revisions()];

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
    } else {
      items.sort(
        (a, b) =>
          new Date(b.fechaEnvio).getTime() -
          new Date(a.fechaEnvio).getTime()
      );
    }

    return items;
  });
}