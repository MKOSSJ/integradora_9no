import { NgClass, DatePipe } from '@angular/common';
import { Component, Input, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  LucideDynamicIcon,
  LucidePlus,
  LucideSearch,
  LucideSlidersHorizontal,
  LucidePenSquare,
  LucideTrash2,
  LucideX,
  LucideSave,
  LucideAlertTriangle
} from '@lucide/angular';

import { AdminCrudConfig, AdminField } from './admin-crud.types';

@Component({
  selector: 'app-admin-crud-page',
  standalone: true,
  imports: [NgClass, DatePipe, FormsModule, LucideDynamicIcon],
  templateUrl: './admin-crud-page.html',
  styleUrl: './admin-crud-page.css'
})
export class AdminCrudPage implements OnInit {
  @Input({ required: true }) config!: AdminCrudConfig;

  search = signal('');
  statusFilter = signal<'todos' | 'activo' | 'inactivo'>('todos');
  modalMode = signal<'create' | 'edit' | null>(null);
  deleteTarget = signal<Record<string, any> | null>(null);
  form = signal<Record<string, any>>({});
  items = signal<Record<string, any>[]>([]);

  plusIcon = LucidePlus;
  searchIcon = LucideSearch;
  filterIcon = LucideSlidersHorizontal;
  editIcon = LucidePenSquare;
  deleteIcon = LucideTrash2;
  closeIcon = LucideX;
  saveIcon = LucideSave;
  warningIcon = LucideAlertTriangle;

  ngOnInit(): void {
    this.items.set(this.config.initialItems.map(item => ({ ...item })));
    this.form.set(this.createEmptyForm());
  }

  filteredItems = computed(() => {
    const query = this.search().trim().toLowerCase();
    const status = this.statusFilter();

    let data = [...this.items()];

    if (query) {
      data = data.filter(item =>
        this.config.searchKeys.some(key =>
          String(item[key] ?? '').toLowerCase().includes(query)
        )
      );
    }

    if (status !== 'todos') {
      data = data.filter(item => item['estado'] === status);
    }

    return data;
  });

  counterValues = computed(() => {
    const data = this.items();

    return {
      total: data.length,
      activos: data.filter(item => item['estado'] === 'activo').length,
      inactivos: data.filter(item => item['estado'] === 'inactivo').length,
      docentes: data.filter(item => this.valueAsArray(item['roles']).includes('DOCENTE')).length,
      revisores: data.filter(item => this.valueAsArray(item['roles']).includes('REVISOR')).length,
      directivos: data.filter(item => this.valueAsArray(item['roles']).includes('DIRECTIVO')).length,
      administradores: data.filter(item => this.valueAsArray(item['roles']).includes('ADMIN')).length
    } as Record<string, number>;
  });

  isFormValid = computed(() => {
    const form = this.form();

    return this.config.fields
      .filter(field => field.required)
      .every(field => {
        const value = form[field.key];

        if (Array.isArray(value)) return value.length > 0;

        return String(value ?? '').trim() !== '';
      });
  });

  openCreateModal(): void {
    this.form.set(this.createEmptyForm());
    this.modalMode.set('create');
  }

  openEditModal(item: Record<string, any>): void {
    this.form.set({ ...item });
    this.modalMode.set('edit');
  }

  closeModal(): void {
    this.modalMode.set(null);
    this.form.set(this.createEmptyForm());
  }

  updateField(field: AdminField, value: any): void {
    this.form.update(current => ({
      ...current,
      [field.key]: value
    }));
  }

  toggleMultiValue(field: AdminField, value: string | number): void {
    this.form.update(current => {
      const values = this.valueAsArray(current[field.key]);
      const exists = values.includes(value);

      return {
        ...current,
        [field.key]: exists
          ? values.filter(item => item !== value)
          : [...values, value]
      };
    });
  }

  hasMultiValue(field: AdminField, value: string | number): boolean {
    return this.valueAsArray(this.form()[field.key]).includes(value);
  }

  saveItem(): void {
    if (!this.isFormValid()) return;

    const form = this.form();

    if (this.modalMode() === 'create') {
      const newItem = {
        ...form,
        id: this.nextId(),
        publicId: form['publicId'] || `${this.config.entityLabel}-${Date.now()}`
      };

      this.items.update(current => [newItem, ...current]);
      this.closeModal();
      return;
    }

    if (this.modalMode() === 'edit') {
      this.items.update(current =>
        current.map(item => item['id'] === form['id'] ? { ...form } : item)
      );

      this.closeModal();
    }
  }

  openDeleteModal(item: Record<string, any>): void {
    this.deleteTarget.set(item);
  }

  closeDeleteModal(): void {
    this.deleteTarget.set(null);
  }

  confirmDelete(): void {
    const target = this.deleteTarget();

    if (!target) return;

    this.items.update(current =>
      current.map(item =>
        item['id'] === target['id']
          ? { ...item, estado: 'inactivo' }
          : item
      )
    );

    this.closeDeleteModal();
  }

  displayValue(item: Record<string, any>, key: string): string {
    const value = item[key];

    if (Array.isArray(value)) return value.join(', ');

    return String(value ?? '');
  }

  valueAsArray(value: any): any[] {
    if (Array.isArray(value)) return value;
    if (value === null || value === undefined || value === '') return [];
    return [value];
  }

  getStatusClasses(status: string): string {
    if (status === 'activo') return 'bg-green-100 text-green-700 ring-green-200';
    if (status === 'inactivo') return 'bg-slate-100 text-slate-600 ring-slate-200';
    return 'bg-amber-100 text-amber-700 ring-amber-200';
  }

  getCounterClasses(tone?: string): string {
    if (tone === 'green') return 'border-green-200 bg-gradient-to-br from-green-50 to-white text-green-600';
    if (tone === 'cyan') return 'border-cyan-200 bg-gradient-to-br from-cyan-50 to-white text-cyan-600';
    if (tone === 'amber') return 'border-amber-200 bg-gradient-to-br from-amber-50 to-white text-amber-600';
    if (tone === 'purple') return 'border-purple-200 bg-gradient-to-br from-purple-50 to-white text-purple-600';
    if (tone === 'red') return 'border-red-200 bg-gradient-to-br from-red-50 to-white text-red-600';
    return 'border-slate-200 bg-white text-slate-900';
  }

  private nextId(): number {
    const ids = this.items().map(item => Number(item['id']));
    return ids.length === 0 ? 1 : Math.max(...ids) + 1;
  }

  private createEmptyForm(): Record<string, any> {
    const form: Record<string, any> = {
      id: null,
      publicId: '',
      estado: 'activo'
    };

    for (const field of this.config.fields) {
      if (field.type === 'multiselect') {
        form[field.key] = [];
      } else if (field.key === 'estado') {
        form[field.key] = 'activo';
      } else {
        form[field.key] = '';
      }
    }

    return form;
  }
}
