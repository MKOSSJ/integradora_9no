import { NgClass } from '@angular/common';
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
  isImported = signal(false);
  showConfirmModal = signal(false);
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
    return items.length > 0 && items.every(item => item['estado'] === 'validado') && !this.isImported();
  });

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

  validateFile(): void {
    if (!this.selectedFile()) return;
    this.isValidating.set(true);
    setTimeout(() => {
      this.previewData.set(this.config.initialPreview.map(item => ({ ...item })));
      this.isValidating.set(false);
    }, 800);
  }

  retryValidation(): void {
    this.previewData.update(items => items.map(item => ({ ...item, estado: 'validado', observacion: 'Registro válido' })));
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

  confirmImport(): void {
    this.isImported.set(true);
    this.showConfirmModal.set(false);
  }

  downloadTemplate(): void {
    console.log(this.config.templateLabel);
  }

  getStatusClasses(status: string): string {
    if (status === 'validado') return 'bg-green-100 text-green-700 ring-green-200';
    if (status === 'error') return 'bg-red-100 text-red-700 ring-red-200';
    return 'bg-amber-100 text-amber-700 ring-amber-200';
  }
}
