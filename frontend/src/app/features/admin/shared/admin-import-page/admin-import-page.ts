import { NgClass } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, computed, signal } from '@angular/core';

import {
  LucideDynamicIcon,
  LucideUpload,
  LucideFileSpreadsheet,
  LucideDownload,
  LucideCircleCheckBig,
  LucideTriangleAlert,
  LucideRefreshCcw,
  LucideSave,
  LucideTrash2
} from '@lucide/angular';

import { AdminImportConfig } from './admin-import.types';

@Component({
  selector: 'app-admin-import-page',
  standalone: true,
  imports: [NgClass, LucideDynamicIcon],
  templateUrl: './admin-import-page.html',
  styleUrl: './admin-import-page.css'
})
export class AdminImportPage {
  @Input({ required: true }) config!: AdminImportConfig;

  selectedFile = signal<File | null>(null);
  isDragging = signal(false);
  isValidating = signal(false);
  isImporting = signal(false);
  isImported = signal(false);
  showConfirmModal = signal(false);
  statusNotice = signal<{ message: string; type: 'success' | 'error' } | null>(null);
  previewData = signal<Record<string, any>[]>([]);

  uploadIcon = LucideUpload;
  fileIcon = LucideFileSpreadsheet;
  downloadIcon = LucideDownload;
  checkIcon = LucideCircleCheckBig;
  warningIcon = LucideTriangleAlert;
  refreshIcon = LucideRefreshCcw;
  saveIcon = LucideSave;
  deleteIcon = LucideTrash2;

  counters = computed(() => {
    const items = this.previewData();
    return {
      total: items.length,
      validos: items.filter(item => item['estado'] === 'validado').length,
      errores: items.filter(item => item['estado'] === 'error').length
    };
  });

  canImport = computed(() => {
    const items = this.previewData();
    return items.some(item => item['estado'] === 'validado') &&
      !this.isImported() &&
      !this.isImporting();
  });

  validRowsCount = computed(() =>
    this.previewData().filter(item => item['estado'] === 'validado').length
  );

  hasErrors = computed(() => this.previewData().some(item => item['estado'] === 'error'));

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
    const file = event.dataTransfer?.files?.[0];
    if (file) this.handleFile(file);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) this.handleFile(file);
    input.value = '';
  }

  handleFile(file: File): void {
    this.selectedFile.set(file);
    this.isImported.set(false);
    this.previewData.set([]);
    this.validateFile();
  }

  async validateFile(): Promise<void> {
    const file = this.selectedFile();
    if (!file) return;

    this.isValidating.set(true);
    this.isImported.set(false);

    try {
      const preview = await this.config.dataSource.validate(file);
      this.previewData.set(preview.map(item => ({ ...item })));
    } catch (error) {
      this.previewData.set([{
        id: 'archivo-error',
        estado: 'error',
        observacion: this.errorMessage(error, 'No fue posible validar el archivo.')
      }]);
    } finally {
      this.isValidating.set(false);
    }
  }

  retryValidation(): void {
    void this.validateFile();
  }

  removeFile(): void {
    this.selectedFile.set(null);
    this.previewData.set([]);
    this.isImported.set(false);
    this.showConfirmModal.set(false);
  }

  openConfirmModal(): void {
    if (!this.canImport()) return;
    this.showConfirmModal.set(true);
  }

  closeConfirmModal(): void {
    this.showConfirmModal.set(false);
  }

  async confirmImport(): Promise<void> {
    if (this.isImporting()) return;

    const validItems = this.previewData().filter(
      item => item['estado'] === 'validado'
    );
    this.isImporting.set(true);

    try {
      const outcome = await this.config.dataSource.import(validItems);

      if (outcome?.items) {
        this.previewData.set(outcome.items.map(item => ({ ...item })));
      }

      this.isImported.set(outcome?.type !== 'error');
      this.showConfirmModal.set(false);
      this.statusNotice.set({
        type: outcome?.type ?? 'success',
        message: outcome?.message ?? this.config.successMessage
      });
    } catch (error) {
      this.showConfirmModal.set(false);
      this.statusNotice.set({
        type: 'error',
        message: this.errorMessage(error, 'No fue posible realizar la importación.')
      });
    } finally {
      this.isImporting.set(false);
    }
  }

  downloadTemplate(): void {
    this.config.dataSource.downloadTemplate();
  }

  closeStatusNotice(): void {
    this.statusNotice.set(null);
  }

  getStatusClasses(status: string): string {
    if (status === 'validado') return 'bg-green-100 text-green-700 ring-green-200';
    if (status === 'error') return 'bg-red-100 text-red-700 ring-red-200';
    return 'bg-amber-100 text-amber-700 ring-amber-200';
  }

  getStatusModalIconClasses(type: 'success' | 'error'): string {
    return type === 'success'
      ? 'bg-green-100 text-green-600'
      : 'bg-red-100 text-red-600';
  }

  getStatusModalButtonClasses(type: 'success' | 'error'): string {
    return type === 'success'
      ? 'bg-teal-500 hover:bg-teal-600'
      : 'bg-red-600 hover:bg-red-700';
  }

  private errorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      return this.messageFromPayload(error.error) ?? fallback;
    }

    if (error instanceof Error && error.message.trim()) {
      return error.message.trim();
    }

    return this.messageFromPayload(error) ?? fallback;
  }

  private messageFromPayload(payload: unknown): string | null {
    if (typeof payload === 'string' && payload.trim()) return payload.trim();
    if (!payload || typeof payload !== 'object') return null;

    const response = payload as Record<string, unknown>;
    const errors = response['errors'];
    const messages = Array.isArray(errors)
      ? errors
      : errors && typeof errors === 'object'
        ? Object.values(errors).flatMap(value =>
            Array.isArray(value) ? value : [value]
          )
        : [];
    const normalized = messages.filter(
      (item): item is string => typeof item === 'string' && !!item.trim()
    );

    if (normalized.length > 0) return normalized.join(' ');

    const message = response['message'];
    return typeof message === 'string' && message.trim()
      ? message.trim()
      : null;
  }
}
