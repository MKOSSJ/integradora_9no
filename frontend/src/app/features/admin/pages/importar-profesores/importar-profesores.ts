import { DatePipe, NgClass } from '@angular/common';
import { Component, computed, signal } from '@angular/core';

import {
  LucideDynamicIcon,
  LucideUserPlus,
  LucideUpload,
  LucideFileSpreadsheet,
  LucideDownload,
  LucideCircleCheckBig,
  LucideTriangleAlert,
  LucideClock3,
  LucideRefreshCcw,
  LucideDatabase,
  LucideSave,
  LucideTrash2
} from '@lucide/angular';

type ImportStatus = 'pendiente' | 'validado' | 'error' | 'importado';

interface ProfesorPreview {
  id: number;
  nombre: string;
  email: string;
  telefono: string;
  academia: string;
  rol: string;
  estado: ImportStatus;
  observacion: string;
}

interface ImportHistory {
  id: number;
  archivo: string;
  fecha: string;
  registros: number;
  exitosos: number;
  errores: number;
  estado: ImportStatus;
}

@Component({
  selector: 'app-importar-profesores',
  standalone: true,
  imports: [NgClass, DatePipe, LucideDynamicIcon],
  templateUrl: './importar-profesores.html',
  styleUrl: './importar-profesores.css'
})
export class ImportarProfesores {
  selectedFile = signal<File | null>(null);
  isDragging = signal(false);
  isValidating = signal(false);
  isImported = signal(false);
  showConfirmModal = signal(false);

  pageIcon = LucideUserPlus;
  uploadIcon = LucideUpload;
  fileIcon = LucideFileSpreadsheet;
  downloadIcon = LucideDownload;
  checkIcon = LucideCircleCheckBig;
  warningIcon = LucideTriangleAlert;
  clockIcon = LucideClock3;
  refreshIcon = LucideRefreshCcw;
  databaseIcon = LucideDatabase;
  saveIcon = LucideSave;
  deleteIcon = LucideTrash2;

  previewData = signal<ProfesorPreview[]>([]);

  history = signal<ImportHistory[]>([
    {
      id: 1,
      archivo: 'profesores_enero_2026.xlsx',
      fecha: '2026-07-10',
      registros: 25,
      exitosos: 25,
      errores: 0,
      estado: 'importado'
    },
    {
      id: 2,
      archivo: 'docentes_ti.csv',
      fecha: '2026-07-08',
      registros: 18,
      exitosos: 16,
      errores: 2,
      estado: 'error'
    }
  ]);

  counters = computed(() => {
    const items = this.previewData();

    return {
      total: items.length,
      validos: items.filter(item => item.estado === 'validado').length,
      errores: items.filter(item => item.estado === 'error').length,
      pendientes: items.filter(item => item.estado === 'pendiente').length
    };
  });

  canImport = computed(() => {
    const items = this.previewData();

    return (
      items.length > 0 &&
      items.every(item => item.estado === 'validado') &&
      !this.isImported()
    );
  });

  hasErrors = computed(() => {
    return this.previewData().some(item => item.estado === 'error');
  });

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

    if (file) {
      this.handleFile(file);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (file) {
      this.handleFile(file);
    }

    input.value = '';
  }

  handleFile(file: File): void {
    this.selectedFile.set(file);
    this.isImported.set(false);
    this.previewData.set([]);
    this.validateFile();
  }

  validateFile(): void {
    const file = this.selectedFile();

    if (!file) return;

    this.isValidating.set(true);

    setTimeout(() => {
      this.previewData.set([
        {
          id: 1,
          nombre: 'Carlos Pérez',
          email: 'carlos.perez@uth.edu.mx',
          telefono: '7711234567',
          academia: 'Desarrollo de Software',
          rol: 'DOCENTE',
          estado: 'validado',
          observacion: 'Registro válido'
        },
        {
          id: 2,
          nombre: 'María González',
          email: 'maria.gonzalez@uth.edu.mx',
          telefono: '7714567890',
          academia: 'Bases de Datos',
          rol: 'REVISOR',
          estado: 'validado',
          observacion: 'Registro válido'
        },
        {
          id: 3,
          nombre: 'Juan Martínez',
          email: '',
          telefono: '7719999999',
          academia: 'Redes',
          rol: 'DOCENTE',
          estado: 'error',
          observacion: 'Falta correo electrónico'
        }
      ]);

      this.isValidating.set(false);
    }, 900);
  }

  retryValidation(): void {
    this.previewData.update(items =>
      items.map(item => ({
        ...item,
        email: item.email || 'juan.martinez@uth.edu.mx',
        estado: 'validado',
        observacion: 'Registro válido'
      }))
    );
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
    const file = this.selectedFile();
    const items = this.previewData();

    if (!file || items.length === 0) return;

    this.history.update(current => [
      {
        id: Date.now(),
        archivo: file.name,
        fecha: new Date().toISOString(),
        registros: items.length,
        exitosos: items.length,
        errores: 0,
        estado: 'importado'
      },
      ...current
    ]);

    this.isImported.set(true);
    this.showConfirmModal.set(false);
  }

  downloadTemplate(): void {
    console.log('Descargar plantilla de profesores');
  }

  getStatusLabel(status: ImportStatus): string {
    if (status === 'validado') return 'Validado';
    if (status === 'error') return 'Error';
    if (status === 'importado') return 'Importado';
    return 'Pendiente';
  }

  getStatusClasses(status: ImportStatus): string {
    if (status === 'validado' || status === 'importado') {
      return 'bg-green-100 text-green-700 ring-green-200';
    }

    if (status === 'error') {
      return 'bg-red-100 text-red-700 ring-red-200';
    }

    return 'bg-amber-100 text-amber-700 ring-amber-200';
  }

  getStatusIcon(status: ImportStatus): any {
    if (status === 'validado' || status === 'importado') return this.checkIcon;
    if (status === 'error') return this.warningIcon;
    return this.clockIcon;
  }
}