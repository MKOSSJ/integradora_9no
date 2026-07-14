import { DatePipe, NgClass } from '@angular/common';
import { Component, computed, signal } from '@angular/core';

import {
  LucideDynamicIcon,
  LucideUpload,
  LucideFileSpreadsheet,
  LucideDownload,
  LucideCircleCheckBig,
  LucideTriangleAlert,
  LucideClock3,
  LucideX,
  LucideRefreshCcw,
  LucideDatabase,
  LucideEye,
  LucideSave,
  LucideTrash2
} from '@lucide/angular';

type ImportStatus = 'pendiente' | 'validado' | 'error' | 'importado';

interface AcademiaPreview {
  id: number;
  nombre: string;
  clave: string;
  carrera: string;
  coordinador: string;
  docentes: number;
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
  selector: 'app-importacion-academias',
  standalone: true,
  imports: [
    NgClass,
    DatePipe,
    LucideDynamicIcon
  ],
  templateUrl: './importacion-academias.html',
  styleUrl: './importacion-academias.css'
})
export class ImportacionAcademias {
  selectedFile = signal<File | null>(null);
  isDragging = signal(false);
  isValidating = signal(false);
  isImported = signal(false);
  showConfirmModal = signal(false);

  uploadIcon = LucideUpload;
  fileIcon = LucideFileSpreadsheet;
  downloadIcon = LucideDownload;
  checkIcon = LucideCircleCheckBig;
  warningIcon = LucideTriangleAlert;
  clockIcon = LucideClock3;
  closeIcon = LucideX;
  refreshIcon = LucideRefreshCcw;
  databaseIcon = LucideDatabase;
  eyeIcon = LucideEye;
  saveIcon = LucideSave;
  deleteIcon = LucideTrash2;

  previewData = signal<AcademiaPreview[]>([]);

  history = signal<ImportHistory[]>([
    {
      id: 1,
      archivo: 'academias_enero_2026.xlsx',
      fecha: '2026-07-10',
      registros: 8,
      exitosos: 8,
      errores: 0,
      estado: 'importado'
    },
    {
      id: 2,
      archivo: 'carga_academias_ti.csv',
      fecha: '2026-07-08',
      registros: 6,
      exitosos: 5,
      errores: 1,
      estado: 'error'
    },
    {
      id: 3,
      archivo: 'academias_periodo_anterior.xlsx',
      fecha: '2026-07-01',
      registros: 10,
      exitosos: 10,
      errores: 0,
      estado: 'importado'
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
          nombre: 'Academia de Desarrollo de Software',
          clave: 'ADS',
          carrera: 'Ingeniería en Tecnologías de la Información',
          coordinador: 'Carlos Pérez',
          docentes: 8,
          estado: 'validado',
          observacion: 'Registro válido'
        },
        {
          id: 2,
          nombre: 'Academia de Bases de Datos',
          clave: 'ABD',
          carrera: 'Ingeniería en Tecnologías de la Información',
          coordinador: 'María González',
          docentes: 6,
          estado: 'validado',
          observacion: 'Registro válido'
        },
        {
          id: 3,
          nombre: 'Academia de Redes',
          clave: 'ARD',
          carrera: 'Ingeniería en Tecnologías de la Información',
          coordinador: 'Juan Martínez',
          docentes: 5,
          estado: 'validado',
          observacion: 'Registro válido'
        },
        {
          id: 4,
          nombre: 'Academia de Matemáticas',
          clave: '',
          carrera: 'Ingeniería Industrial',
          coordinador: 'Ana López',
          docentes: 4,
          estado: 'error',
          observacion: 'Falta clave de academia'
        }
      ]);

      this.isValidating.set(false);
    }, 900);
  }

  removeFile(): void {
    this.selectedFile.set(null);
    this.previewData.set([]);
    this.isImported.set(false);
    this.showConfirmModal.set(false);
  }

  retryValidation(): void {
    if (!this.selectedFile()) return;

    this.previewData.update(items =>
      items.map(item => ({
        ...item,
        clave: item.clave || 'MAT',
        estado: 'validado',
        observacion: 'Registro válido'
      }))
    );
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

    const newHistory: ImportHistory = {
      id: Date.now(),
      archivo: file.name,
      fecha: new Date().toISOString(),
      registros: items.length,
      exitosos: items.length,
      errores: 0,
      estado: 'importado'
    };

    this.history.update(current => [newHistory, ...current]);
    this.isImported.set(true);
    this.showConfirmModal.set(false);
  }

  downloadTemplate(): void {
    console.log('Descargar plantilla de academias');
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