import { NgClass } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  LucideDynamicIcon,
  LucideGrid2x2,
  LucidePlus,
  LucideSearch,
  LucideSlidersHorizontal,
  LucideUsers,
  LucidePenSquare,
  LucideTrash2,
  LucideX,
  LucideSave,
  LucideAlertTriangle
} from '@lucide/angular';

type GrupoStatus = 'activo' | 'inactivo';
type Turno = 'Matutino' | 'Vespertino' | 'Mixto';

interface Grupo {
  id: number;
  nombre: string;
  carrera: string;
  cuatrimestre: string;
  periodo: string;
  turno: Turno;
  alumnos: number;
  estado: GrupoStatus;
}

interface GrupoForm {
  id: number | null;
  nombre: string;
  carrera: string;
  cuatrimestre: string;
  periodo: string;
  turno: Turno;
  alumnos: string;
  estado: GrupoStatus;
}

@Component({
  selector: 'app-grupos',
  standalone: true,
  imports: [NgClass, FormsModule, LucideDynamicIcon],
  templateUrl: './grupos.html',
  styleUrl: './grupos.css'
})
export class Grupos {
  search = signal('');
  statusFilter = signal<'todos' | GrupoStatus>('todos');

  modalMode = signal<'create' | 'edit' | null>(null);
  deleteTarget = signal<Grupo | null>(null);

  form = signal<GrupoForm>(this.emptyForm());

  groupIcon = LucideGrid2x2;
  plusIcon = LucidePlus;
  searchIcon = LucideSearch;
  filterIcon = LucideSlidersHorizontal;
  usersIcon = LucideUsers;
  editIcon = LucidePenSquare;
  deleteIcon = LucideTrash2;
  closeIcon = LucideX;
  saveIcon = LucideSave;
  warningIcon = LucideAlertTriangle;

  grupos = signal<Grupo[]>([
    {
      id: 1,
      nombre: 'TI-301',
      carrera: 'Ingeniería en TI',
      cuatrimestre: '3°',
      periodo: 'Enero - Abril 2026',
      turno: 'Matutino',
      alumnos: 32,
      estado: 'activo'
    },
    {
      id: 2,
      nombre: 'TI-502',
      carrera: 'Ingeniería en TI',
      cuatrimestre: '5°',
      periodo: 'Enero - Abril 2026',
      turno: 'Vespertino',
      alumnos: 28,
      estado: 'activo'
    },
    {
      id: 3,
      nombre: 'IND-401',
      carrera: 'Ingeniería Industrial',
      cuatrimestre: '4°',
      periodo: 'Enero - Abril 2026',
      turno: 'Matutino',
      alumnos: 35,
      estado: 'inactivo'
    }
  ]);

  filteredGrupos = computed(() => {
    const query = this.search().trim().toLowerCase();
    const status = this.statusFilter();

    let items = [...this.grupos()];

    if (query) {
      items = items.filter(item =>
        item.nombre.toLowerCase().includes(query) ||
        item.carrera.toLowerCase().includes(query) ||
        item.periodo.toLowerCase().includes(query)
      );
    }

    if (status !== 'todos') {
      items = items.filter(item => item.estado === status);
    }

    return items;
  });

  counters = computed(() => {
    const items = this.grupos();

    return {
      total: items.length,
      activos: items.filter(item => item.estado === 'activo').length,
      inactivos: items.filter(item => item.estado === 'inactivo').length,
      alumnos: items.reduce((total, item) => total + item.alumnos, 0)
    };
  });

  isFormValid = computed(() => {
    const data = this.form();

    return (
      data.nombre.trim() !== '' &&
      data.carrera.trim() !== '' &&
      data.cuatrimestre.trim() !== '' &&
      data.periodo.trim() !== '' &&
      data.alumnos.trim() !== ''
    );
  });

  setStatusFilter(value: string): void {
    this.statusFilter.set(value as 'todos' | GrupoStatus);
  }

  openCreateModal(): void {
    this.form.set(this.emptyForm());
    this.modalMode.set('create');
  }

  openEditModal(item: Grupo): void {
    this.form.set({
      id: item.id,
      nombre: item.nombre,
      carrera: item.carrera,
      cuatrimestre: item.cuatrimestre,
      periodo: item.periodo,
      turno: item.turno,
      alumnos: String(item.alumnos),
      estado: item.estado
    });

    this.modalMode.set('edit');
  }

  closeModal(): void {
    this.modalMode.set(null);
    this.form.set(this.emptyForm());
  }

  updateField(field: keyof GrupoForm, value: string): void {
    this.form.update(current => ({
      ...current,
      [field]: value
    }));
  }

  saveGrupo(): void {
    if (!this.isFormValid()) return;

    const data = this.form();

    if (this.modalMode() === 'create') {
      this.grupos.update(items => [
        {
          id: this.nextId(),
          nombre: data.nombre.trim().toUpperCase(),
          carrera: data.carrera.trim(),
          cuatrimestre: data.cuatrimestre.trim(),
          periodo: data.periodo.trim(),
          turno: data.turno,
          alumnos: Number(data.alumnos),
          estado: data.estado
        },
        ...items
      ]);

      this.closeModal();
      return;
    }

    if (this.modalMode() === 'edit' && data.id !== null) {
      this.grupos.update(items =>
        items.map(item =>
          item.id === data.id
            ? {
                ...item,
                nombre: data.nombre.trim().toUpperCase(),
                carrera: data.carrera.trim(),
                cuatrimestre: data.cuatrimestre.trim(),
                periodo: data.periodo.trim(),
                turno: data.turno,
                alumnos: Number(data.alumnos),
                estado: data.estado
              }
            : item
        )
      );

      this.closeModal();
    }
  }

  openDeleteModal(item: Grupo): void {
    this.deleteTarget.set(item);
  }

  closeDeleteModal(): void {
    this.deleteTarget.set(null);
  }

  confirmDelete(): void {
    const target = this.deleteTarget();

    if (!target) return;

    this.grupos.update(items => items.filter(item => item.id !== target.id));
    this.closeDeleteModal();
  }

  getStatusClasses(status: GrupoStatus): string {
    if (status === 'activo') return 'bg-green-100 text-green-700 ring-green-200';
    return 'bg-slate-100 text-slate-600 ring-slate-200';
  }

  private nextId(): number {
    const ids = this.grupos().map(item => item.id);
    return ids.length === 0 ? 1 : Math.max(...ids) + 1;
  }

  private emptyForm(): GrupoForm {
    return {
      id: null,
      nombre: '',
      carrera: '',
      cuatrimestre: '',
      periodo: 'Enero - Abril 2026',
      turno: 'Matutino',
      alumnos: '',
      estado: 'activo'
    };
  }
}