import { NgClass, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnDestroy, OnInit, computed, signal } from '@angular/core';
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
  LucideAlertTriangle,
  LucideCircleCheckBig
} from '@lucide/angular';

import { AdminCrudConfig, AdminField, AdminOption } from './admin-crud.types';

@Component({
  selector: 'app-admin-crud-page',
  standalone: true,
  imports: [NgClass, DatePipe, FormsModule, LucideDynamicIcon],
  templateUrl: './admin-crud-page.html',
  styleUrl: './admin-crud-page.css'
})
export class AdminCrudPage implements OnInit, OnDestroy {
  @Input({ required: true }) config!: AdminCrudConfig;

  search = signal('');
  statusFilter = signal<'todos' | 'activo' | 'inactivo'>('todos');
  modalMode = signal<'create' | 'edit' | null>(null);
  deleteTarget = signal<Record<string, any> | null>(null);
  form = signal<Record<string, any>>({});
  items = signal<Record<string, any>[]>([]);
  statusNotice = signal<{
    message: string;
    type: 'success' | 'error';
  } | null>(null);

  plusIcon = LucidePlus;
  searchIcon = LucideSearch;
  filterIcon = LucideSlidersHorizontal;
  editIcon = LucidePenSquare;
  deleteIcon = LucideTrash2;
  closeIcon = LucideX;
  saveIcon = LucideSave;
  warningIcon = LucideAlertTriangle;
  successIcon = LucideCircleCheckBig;

  private noticeTimeout: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.form.set(this.createEmptyForm());

    if (this.config.dataSource) {
      this.config.dataSource.load().subscribe({
        next: items => this.items.set(items.map(item => ({ ...item }))),
        error: error => this.handleRequestError(
          error,
          'No fue posible cargar los registros.'
        )
      });
      return;
    }

    this.items.set(this.config.initialItems.map(item => ({ ...item })));
  }

  ngOnDestroy(): void {
    if (this.noticeTimeout) {
      clearTimeout(this.noticeTimeout);
    }
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

    return this.config.fields.every(field =>
      this.isFieldValid(field, form[field.key])
    );
  });

  openCreateModal(): void {
    this.clearStatusNotice();

    if (this.blockUnsupportedAction('create')) return;

    this.form.set(this.createEmptyForm());
    this.modalMode.set('create');
  }

  openEditModal(item: Record<string, any>): void {
    this.clearStatusNotice();

    if (this.blockUnsupportedAction('edit')) return;

    this.form.set({ ...item });
    this.modalMode.set('edit');
  }

  closeModal(): void {
    this.clearStatusNotice();
    this.modalMode.set(null);
    this.form.set(this.createEmptyForm());
  }

  updateField(field: AdminField, value: any): void {
    if (this.isFieldReadonly(field)) return;

    this.form.update(current => ({
      ...current,
      [field.key]: value
    }));
  }

  toggleMultiValue(field: AdminField, value: string | number): void {
    if (this.isFieldReadonly(field)) return;

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

  getFieldOptions(field: AdminField): AdminOption[] {
    return field.optionsFor?.(this.form()) ?? field.options ?? [];
  }

  isFieldReadonly(field: AdminField): boolean {
    return field.readonlyWhen?.(this.form()) ?? false;
  }

  preventInvalidNumberInput(event: KeyboardEvent, field: AdminField): void {
    if (field.type !== 'number') return;

    const invalidKeys = ['e', 'E', '+'];

    if ((field.min ?? Number.NEGATIVE_INFINITY) >= 0) {
      invalidKeys.push('-');
    }

    if (field.step === 1) {
      invalidKeys.push('.', ',');
    }

    if (invalidKeys.includes(event.key)) {
      event.preventDefault();
    }
  }

  closeStatusNotice(): void {
    this.clearStatusNotice();
  }

  saveItem(): void {
    if (!this.isFormValid()) return;

    const form = this.form();
    const dataSource = this.config.dataSource;

    if (this.modalMode() === 'create') {
      if (dataSource) {
        try {
          dataSource.create({ ...form }).subscribe({
            next: newItem => {
              this.items.update(current => [{ ...newItem }, ...current]);
              this.closeModal();
              this.showSuccessNotice('create');
            },
            error: error => this.handleRequestError(
              error,
              'No fue posible guardar los cambios.'
            )
          });
        } catch (error) {
          this.handleRequestError(error, 'No fue posible guardar los cambios.');
        }
        return;
      }

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
      if (dataSource) {
        try {
          dataSource.update({ ...form }).subscribe({
            next: updatedItem => {
              this.items.update(current =>
                current.map(item =>
                  item['id'] === form['id'] ? { ...updatedItem } : item
                )
              );
              this.closeModal();
              this.showSuccessNotice('update');
            },
            error: error => this.handleRequestError(
              error,
              'No fue posible guardar los cambios.'
            )
          });
        } catch (error) {
          this.handleRequestError(error, 'No fue posible guardar los cambios.');
        }
        return;
      }

      this.items.update(current =>
        current.map(item => item['id'] === form['id'] ? { ...form } : item)
      );

      this.closeModal();
    }
  }

  openDeleteModal(item: Record<string, any>): void {
    this.clearStatusNotice();

    if (this.blockUnsupportedAction('delete')) return;

    this.deleteTarget.set(item);
  }

  closeDeleteModal(): void {
    this.clearStatusNotice();
    this.deleteTarget.set(null);
  }

  confirmDelete(): void {
    const target = this.deleteTarget();

    if (!target) return;

    if (this.config.dataSource) {
      try {
        this.config.dataSource.delete(target).subscribe({
          next: () => {
            this.markItemInactive(target);
            this.closeDeleteModal();
            this.showSuccessNotice('delete');
          },
          error: error => this.handleRequestError(
            error,
            'No fue posible dar de baja el registro.'
          )
        });
      } catch (error) {
        this.handleRequestError(
          error,
          'No fue posible dar de baja el registro.'
        );
      }
      return;
    }

    this.markItemInactive(target);
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

  getStatusModalIconClasses(type: 'success' | 'error'): string {
    if (type === 'success') {
      return 'bg-green-100 text-green-600';
    }

    return 'bg-red-100 text-red-600';
  }

  getStatusModalButtonClasses(type: 'success' | 'error'): string {
    if (type === 'success') {
      return 'bg-teal-500 hover:bg-teal-600';
    }

    return 'bg-red-600 hover:bg-red-700';
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

  private markItemInactive(target: Record<string, any>): void {
    this.items.update(current =>
      current.map(item =>
        item['id'] === target['id']
          ? { ...item, estado: 'inactivo' }
          : item
      )
    );
  }

  private isFieldValid(field: AdminField, value: unknown): boolean {
    const isEmpty =
      value === null ||
      value === undefined ||
      (typeof value === 'string' && value.trim() === '') ||
      (Array.isArray(value) && value.length === 0);

    if (isEmpty) {
      return !field.required;
    }

    if (field.maxLength !== undefined && typeof value === 'string') {
      if (value.length > field.maxLength) return false;
    }

    if (field.type === 'email') {
      return typeof value === 'string' &&
        /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim());
    }

    if (field.type !== 'number') return true;

    const numberValue = Number(value);

    if (!Number.isFinite(numberValue)) return false;
    if (field.min !== undefined && numberValue < field.min) return false;
    if (field.max !== undefined && numberValue > field.max) return false;

    if (field.step !== undefined) {
      const stepBase = field.min ?? 0;
      const steps = (numberValue - stepBase) / field.step;

      if (Math.abs(steps - Math.round(steps)) > 1e-9) return false;
    }

    return true;
  }

  private showSuccessNotice(operation: 'create' | 'update' | 'delete'): void {
    const message = this.config.successMessages?.[operation];

    if (!message) return;

    this.showStatusNotice(message, 'success');
  }

  private handleRequestError(error: unknown, fallbackMessage: string): void {
    this.showStatusNotice(
      this.extractErrorMessage(error) ?? fallbackMessage,
      'error'
    );
  }

  private extractErrorMessage(error: unknown): string | null {
    if (error instanceof HttpErrorResponse) {
      return this.extractMessageFromPayload(error.error);
    }

    if (error instanceof Error && error.message.trim()) {
      return error.message.trim();
    }

    return this.extractMessageFromPayload(error);
  }

  private extractMessageFromPayload(payload: unknown): string | null {
    if (typeof payload === 'string' && payload.trim()) {
      return payload.trim();
    }

    if (!payload || typeof payload !== 'object') return null;

    const response = payload as Record<string, unknown>;
    const errors = this.extractValidationErrors(response['errors']);

    if (errors) return errors;

    const message = response['message'];
    return typeof message === 'string' && message.trim()
      ? message.trim()
      : null;
  }

  private extractValidationErrors(errors: unknown): string | null {
    if (Array.isArray(errors)) {
      const messages = errors.filter(
        (item): item is string => typeof item === 'string' && !!item.trim()
      );

      return messages.length > 0 ? messages.join(' ') : null;
    }

    if (!errors || typeof errors !== 'object') return null;

    const messages = Object.values(errors)
      .flatMap(value => Array.isArray(value) ? value : [value])
      .filter(
        (item): item is string => typeof item === 'string' && !!item.trim()
      );

    return messages.length > 0 ? messages.join(' ') : null;
  }

  private showStatusNotice(
    message: string,
    type: 'success' | 'error'
  ): void {
    this.clearStatusNotice();
    this.statusNotice.set({ message, type });

    this.noticeTimeout = setTimeout(() => {
      this.statusNotice.set(null);
      this.noticeTimeout = null;
    }, 3500);
  }

  private clearStatusNotice(): void {
    if (this.noticeTimeout) {
      clearTimeout(this.noticeTimeout);
      this.noticeTimeout = null;
    }

    this.statusNotice.set(null);
  }

  private blockUnsupportedAction(
    action: 'create' | 'edit' | 'delete'
  ): boolean {
    const message = this.config.blockedActionsMessage ??
      (action === 'create' ? this.config.blockedCreateMessage : undefined) ??
      (action === 'delete' ? this.config.blockedDeleteMessage : undefined);

    if (!message) return false;

    this.showStatusNotice(message, 'error');
    return true;
  }
}
