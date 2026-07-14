import { NgClass } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  LucideDynamicIcon,
  LucideImage,
  LucidePlus,
  LucideSearch,
  LucideSlidersHorizontal,
  LucidePenSquare,
  LucideTrash2,
  LucideX,
  LucideSave,
  LucideAlertTriangle,
  LucideUserCheck,
  LucideBookOpen,
  LucideGrid2x2,
  LucideSquareCheckBig
} from '@lucide/angular';

type AssignmentStatus = 'activa' | 'pendiente' | 'cerrada';

interface Asignacion {
  id: number;
  docente: string;
  revisor: string;
  asignatura: string;
  grupo: string;
  academia: string;
  periodo: string;
  estado: AssignmentStatus;
}

interface AsignacionForm {
  id: number | null;
  docente: string;
  revisor: string;
  asignatura: string;
  grupo: string;
  academia: string;
  periodo: string;
  estado: AssignmentStatus;
}

@Component({
  selector: 'app-asignacion-academica',
  standalone: true,
  imports: [NgClass, FormsModule, LucideDynamicIcon],
  templateUrl: './asignacion-academica.html',
  styleUrl: './asignacion-academica.css'
})
export class AsignacionAcademica {
  search = signal('');
  statusFilter = signal<'todos' | AssignmentStatus>('todos');

  modalMode = signal<'create' | 'edit' | null>(null);
  deleteTarget = signal<Asignacion | null>(null);

  form = signal<AsignacionForm>(this.emptyForm());

  pageIcon = LucideImage;
  plusIcon = LucidePlus;
  searchIcon = LucideSearch;
  filterIcon = LucideSlidersHorizontal;
  editIcon = LucidePenSquare;
  deleteIcon = LucideTrash2;
  closeIcon = LucideX;
  saveIcon = LucideSave;
  warningIcon = LucideAlertTriangle;
  docenteIcon = LucideUserCheck;
  subjectIcon = LucideBookOpen;
  groupIcon = LucideGrid2x2;
  reviewerIcon = LucideSquareCheckBig;

  docentes = ['Carlos Pérez', 'Ana López', 'Laura Sánchez', 'José Ramírez'];
  revisores = ['María González', 'Juan Martínez', 'Pedro Castillo'];
  asignaturas = ['Programación Web', 'Bases de Datos', 'Física Avanzada', 'Matemáticas'];
  grupos = ['TI-301', 'TI-502', 'IND-401', 'QUI-302'];
  academias = ['Desarrollo de Software', 'Bases de Datos', 'Matemáticas', 'Redes'];

  asignaciones = signal<Asignacion[]>([
    {
      id: 1,
      docente: 'Carlos Pérez',
      revisor: 'María González',
      asignatura: 'Programación Web',
      grupo: 'TI-502',
      academia: 'Desarrollo de Software',
      periodo: 'Enero - Abril 2026',
      estado: 'activa'
    },
    {
      id: 2,
      docente: 'Ana López',
      revisor: 'Juan Martínez',
      asignatura: 'Bases de Datos',
      grupo: 'TI-301',
      academia: 'Bases de Datos',
      periodo: 'Enero - Abril 2026',
      estado: 'pendiente'
    },
    {
      id: 3,
      docente: 'José Ramírez',
      revisor: 'Pedro Castillo',
      asignatura: 'Matemáticas',
      grupo: 'IND-401',
      academia: 'Matemáticas',
      periodo: 'Enero - Abril 2026',
      estado: 'cerrada'
    }
  ]);

  filteredAsignaciones = computed(() => {
    const query = this.search().trim().toLowerCase();
    const status = this.statusFilter();

    let items = [...this.asignaciones()];

    if (query) {
      items = items.filter(item =>
        item.docente.toLowerCase().includes(query) ||
        item.revisor.toLowerCase().includes(query) ||
        item.asignatura.toLowerCase().includes(query) ||
        item.grupo.toLowerCase().includes(query) ||
        item.academia.toLowerCase().includes(query)
      );
    }

    if (status !== 'todos') {
      items = items.filter(item => item.estado === status);
    }

    return items;
  });

  counters = computed(() => {
    const items = this.asignaciones();

    return {
      total: items.length,
      activas: items.filter(item => item.estado === 'activa').length,
      pendientes: items.filter(item => item.estado === 'pendiente').length,
      cerradas: items.filter(item => item.estado === 'cerrada').length
    };
  });

  isFormValid = computed(() => {
    const data = this.form();

    return (
      data.docente.trim() !== '' &&
      data.revisor.trim() !== '' &&
      data.asignatura.trim() !== '' &&
      data.grupo.trim() !== '' &&
      data.academia.trim() !== '' &&
      data.periodo.trim() !== ''
    );
  });

  setStatusFilter(value: string): void {
    this.statusFilter.set(value as 'todos' | AssignmentStatus);
  }

  openCreateModal(): void {
    this.form.set(this.emptyForm());
    this.modalMode.set('create');
  }

  openEditModal(item: Asignacion): void {
    this.form.set({ ...item });
    this.modalMode.set('edit');
  }

  closeModal(): void {
    this.modalMode.set(null);
    this.form.set(this.emptyForm());
  }

  updateField(field: keyof AsignacionForm, value: string): void {
    this.form.update(current => ({
      ...current,
      [field]: value
    }));
  }

  saveAsignacion(): void {
    if (!this.isFormValid()) return;

    const data = this.form();

    if (this.modalMode() === 'create') {
      this.asignaciones.update(items => [
        {
          id: this.nextId(),
          docente: data.docente,
          revisor: data.revisor,
          asignatura: data.asignatura,
          grupo: data.grupo,
          academia: data.academia,
          periodo: data.periodo,
          estado: data.estado
        },
        ...items
      ]);

      this.closeModal();
      return;
    }

    if (this.modalMode() === 'edit' && data.id !== null) {
      this.asignaciones.update(items =>
        items.map(item =>
          item.id === data.id
            ? {
                ...item,
                docente: data.docente,
                revisor: data.revisor,
                asignatura: data.asignatura,
                grupo: data.grupo,
                academia: data.academia,
                periodo: data.periodo,
                estado: data.estado
              }
            : item
        )
      );

      this.closeModal();
    }
  }

  openDeleteModal(item: Asignacion): void {
    this.deleteTarget.set(item);
  }

  closeDeleteModal(): void {
    this.deleteTarget.set(null);
  }

  confirmDelete(): void {
    const target = this.deleteTarget();

    if (!target) return;

    this.asignaciones.update(items => items.filter(item => item.id !== target.id));
    this.closeDeleteModal();
  }

  getStatusLabel(status: AssignmentStatus): string {
    if (status === 'activa') return 'Activa';
    if (status === 'pendiente') return 'Pendiente';
    return 'Cerrada';
  }

  getStatusClasses(status: AssignmentStatus): string {
    if (status === 'activa') return 'bg-green-100 text-green-700 ring-green-200';
    if (status === 'pendiente') return 'bg-amber-100 text-amber-700 ring-amber-200';
    return 'bg-slate-100 text-slate-600 ring-slate-200';
  }

  private nextId(): number {
    const ids = this.asignaciones().map(item => item.id);
    return ids.length === 0 ? 1 : Math.max(...ids) + 1;
  }

  private emptyForm(): AsignacionForm {
    return {
      id: null,
      docente: '',
      revisor: '',
      asignatura: '',
      grupo: '',
      academia: '',
      periodo: 'Enero - Abril 2026',
      estado: 'activa'
    };
  }
}