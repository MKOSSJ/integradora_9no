import { NgClass } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  LucideDynamicIcon,
  LucideSchool,
  LucidePlus,
  LucideSearch,
  LucideSlidersHorizontal,
  LucideUsers,
  LucidePenSquare,
  LucideTrash2,
  LucideX,
  LucideSave,
  LucideAlertTriangle,
  LucideBookOpen
} from '@lucide/angular';

type AcademiaStatus = 'activa' | 'inactiva';

interface Academia {
  id: number;
  nombre: string;
  clave: string;
  carrera: string;
  coordinador: string;
  docentes: number;
  estado: AcademiaStatus;
}

interface AcademiaForm {
  id: number | null;
  nombre: string;
  clave: string;
  carrera: string;
  coordinador: string;
  docentes: string;
  estado: AcademiaStatus;
}

@Component({
  selector: 'app-academias',
  standalone: true,
  imports: [NgClass, FormsModule, LucideDynamicIcon],
  templateUrl: './academias.html',
  styleUrl: './academias.css'
})
export class Academias {
  search = signal('');
  statusFilter = signal<'todos' | AcademiaStatus>('todos');

  modalMode = signal<'create' | 'edit' | null>(null);
  deleteTarget = signal<Academia | null>(null);

  form = signal<AcademiaForm>(this.emptyForm());

  schoolIcon = LucideSchool;
  plusIcon = LucidePlus;
  searchIcon = LucideSearch;
  filterIcon = LucideSlidersHorizontal;
  usersIcon = LucideUsers;
  editIcon = LucidePenSquare;
  deleteIcon = LucideTrash2;
  closeIcon = LucideX;
  saveIcon = LucideSave;
  warningIcon = LucideAlertTriangle;
  bookIcon = LucideBookOpen;

  academias = signal<Academia[]>([
    {
      id: 1,
      nombre: 'Academia de Desarrollo de Software',
      clave: 'ADS',
      carrera: 'Ingeniería en TI',
      coordinador: 'Carlos Pérez',
      docentes: 8,
      estado: 'activa'
    },
    {
      id: 2,
      nombre: 'Academia de Bases de Datos',
      clave: 'ABD',
      carrera: 'Ingeniería en TI',
      coordinador: 'María González',
      docentes: 6,
      estado: 'activa'
    },
    {
      id: 3,
      nombre: 'Academia de Matemáticas',
      clave: 'MAT',
      carrera: 'Ingeniería Industrial',
      coordinador: 'Ana López',
      docentes: 5,
      estado: 'inactiva'
    }
  ]);

  filteredAcademias = computed(() => {
    const query = this.search().trim().toLowerCase();
    const status = this.statusFilter();

    let items = [...this.academias()];

    if (query) {
      items = items.filter(item =>
        item.nombre.toLowerCase().includes(query) ||
        item.clave.toLowerCase().includes(query) ||
        item.carrera.toLowerCase().includes(query) ||
        item.coordinador.toLowerCase().includes(query)
      );
    }

    if (status !== 'todos') {
      items = items.filter(item => item.estado === status);
    }

    return items;
  });

  counters = computed(() => {
    const items = this.academias();

    return {
      total: items.length,
      activas: items.filter(item => item.estado === 'activa').length,
      inactivas: items.filter(item => item.estado === 'inactiva').length,
      docentes: items.reduce((total, item) => total + item.docentes, 0)
    };
  });

  isFormValid = computed(() => {
    const data = this.form();

    return (
      data.nombre.trim() !== '' &&
      data.clave.trim() !== '' &&
      data.carrera.trim() !== '' &&
      data.coordinador.trim() !== '' &&
      data.docentes.trim() !== ''
    );
  });

  setStatusFilter(value: string): void {
    this.statusFilter.set(value as 'todos' | AcademiaStatus);
  }

  openCreateModal(): void {
    this.form.set(this.emptyForm());
    this.modalMode.set('create');
  }

  openEditModal(item: Academia): void {
    this.form.set({
      id: item.id,
      nombre: item.nombre,
      clave: item.clave,
      carrera: item.carrera,
      coordinador: item.coordinador,
      docentes: String(item.docentes),
      estado: item.estado
    });

    this.modalMode.set('edit');
  }

  closeModal(): void {
    this.modalMode.set(null);
    this.form.set(this.emptyForm());
  }

  updateField(field: keyof AcademiaForm, value: string): void {
    this.form.update(current => ({
      ...current,
      [field]: value
    }));
  }

  saveAcademia(): void {
    if (!this.isFormValid()) return;

    const data = this.form();

    if (this.modalMode() === 'create') {
      const newItem: Academia = {
        id: this.nextId(),
        nombre: data.nombre.trim(),
        clave: data.clave.trim().toUpperCase(),
        carrera: data.carrera.trim(),
        coordinador: data.coordinador.trim(),
        docentes: Number(data.docentes),
        estado: data.estado
      };

      this.academias.update(items => [newItem, ...items]);
      this.closeModal();
      return;
    }

    if (this.modalMode() === 'edit' && data.id !== null) {
      this.academias.update(items =>
        items.map(item =>
          item.id === data.id
            ? {
                ...item,
                nombre: data.nombre.trim(),
                clave: data.clave.trim().toUpperCase(),
                carrera: data.carrera.trim(),
                coordinador: data.coordinador.trim(),
                docentes: Number(data.docentes),
                estado: data.estado
              }
            : item
        )
      );

      this.closeModal();
    }
  }

  openDeleteModal(item: Academia): void {
    this.deleteTarget.set(item);
  }

  closeDeleteModal(): void {
    this.deleteTarget.set(null);
  }

  confirmDelete(): void {
    const target = this.deleteTarget();

    if (!target) return;

    this.academias.update(items => items.filter(item => item.id !== target.id));
    this.closeDeleteModal();
  }

  getStatusClasses(status: AcademiaStatus): string {
    if (status === 'activa') return 'bg-green-100 text-green-700 ring-green-200';
    return 'bg-slate-100 text-slate-600 ring-slate-200';
  }

  private nextId(): number {
    const ids = this.academias().map(item => item.id);
    return ids.length === 0 ? 1 : Math.max(...ids) + 1;
  }

  private emptyForm(): AcademiaForm {
    return {
      id: null,
      nombre: '',
      clave: '',
      carrera: '',
      coordinador: '',
      docentes: '',
      estado: 'activa'
    };
  }
}