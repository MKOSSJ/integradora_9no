import { HttpErrorResponse } from '@angular/common/http';
import { NgClass } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import {
  LucideCircleCheckBig,
  LucideDynamicIcon,
  LucideRefreshCcw,
  LucideTriangleAlert
} from '@lucide/angular';

import { GeneracionPlaneacionesVisualResultado } from '../../../../core/models/planeacion-directivo.model';
import { CargasAcademicasImportService } from '../../../../core/services/cargas-academicas-import.service';
import { PlaneacionesDirectivoService } from '../../../../core/services/planeaciones-directivo.service';
import { ProgramasAsignaturaImportService } from '../../../../core/services/programas-asignatura-import.service';
import { AdminImportPage } from '../../shared/admin-import-page/admin-import-page';
import { AdminImportConfig } from '../../shared/admin-import-page/admin-import.types';

@Component({
  selector: 'app-importacion-academias',
  standalone: true,
  imports: [NgClass, AdminImportPage, LucideDynamicIcon],
  templateUrl: './importacion-academias.html',
  styleUrl: './importacion-academias.css'
})
export class ImportacionAcademias implements OnInit {
  private readonly cargasImportService = inject(
    CargasAcademicasImportService
  );
  private readonly programasImportService = inject(
    ProgramasAsignaturaImportService
  );
  private readonly planeacionesDirectivoService = inject(
    PlaneacionesDirectivoService
  );

  readonly activeTab = signal<'programas' | 'academias'>('programas');
  readonly generating = signal(false);
  readonly generationResult =
    signal<GeneracionPlaneacionesVisualResultado | null>(null);
  readonly generationError = signal<string | null>(null);
  readonly periodoOptions = signal<Array<{ label: string; value: string }>>([]);
  readonly selectedPeriodoPublicId = signal('');

  readonly generateIcon = LucideRefreshCcw;
  readonly checkIcon = LucideCircleCheckBig;
  readonly warningIcon = LucideTriangleAlert;

  programConfig: AdminImportConfig = {
    title: 'Programa de asignatura',
    subtitle: 'Carga un programa de asignatura en PDF para procesarlo en el backend.',
    sectionLabel: 'Importaciones',
    importLabel: 'Importar programa',
    templateLabel: 'Descargar plantilla',
    expectedColumns: ['Archivo PDF del programa de asignatura'],
    previewColumns: [
      { key: 'archivo', label: 'Archivo', required: true },
      { key: 'asignatura', label: 'Asignatura' },
      { key: 'clave', label: 'Clave' },
      { key: 'unidadesExtraidas', label: 'Unidades' },
      { key: 'datosGuardados', label: 'Guardado' }
    ],
    dataSource: this.programasImportService.dataSource,
    successMessage: 'Importación de programa de asignatura finalizada.',
    acceptedFileTypes: '.pdf,application/pdf',
    formatHint: 'Formato permitido: .pdf',
    showHeader: false,
    showTemplateAction: false
  };

  cargaAcademicaConfig: AdminImportConfig = {
    title: 'Carga académica',
    subtitle: 'Importa asignaturas, cuatrimestres, programas educativos y docentes para un periodo real.',
    sectionLabel: 'Importaciones',
    importLabel: 'Importar carga académica',
    templateLabel: 'Descargar plantilla',
    expectedColumns: [
      'Archivo original CSV, XLS o XLSX',
      'La estructura será validada por el backend'
    ],
    previewColumns: [
      { key: 'archivo', label: 'Archivo', required: true },
      { key: 'formato', label: 'Formato', required: true },
      { key: 'filas', label: 'Filas' },
      { key: 'fila', label: 'Fila' },
      { key: 'campo', label: 'Campo' },
      { key: 'valor', label: 'Valor' },
      { key: 'mensaje', label: 'Mensaje' }
    ],
    dataSource: this.cargasImportService.dataSource,
    successMessage: 'Importación de carga académica finalizada.',
    acceptedFileTypes: '.csv,.xls,.xlsx',
    formatHint: 'Formatos permitidos: .csv, .xls, .xlsx',
    showHeader: false
  };

  ngOnInit(): void {
    this.cargasImportService.loadPeriodOptions().subscribe({
      next: options => this.periodoOptions.set(options),
      error: error => this.generationError.set(
        this.extractErrorMessage(error) ??
          'No fue posible cargar los periodos disponibles.'
      )
    });
  }

  selectPeriodo(event: Event): void {
    const publicId = (event.target as HTMLSelectElement).value;
    this.selectedPeriodoPublicId.set(publicId);
    this.cargasImportService.setPeriodoPublicId(publicId);
  }

  generatePlaneaciones(): void {
    if (this.generating()) return;

    this.generationError.set(null);
    this.generationResult.set(null);
    this.generating.set(true);

    this.planeacionesDirectivoService.generate().pipe(
      finalize(() => this.generating.set(false))
    ).subscribe({
      next: result => this.generationResult.set(result),
      error: error => this.generationError.set(
        this.extractErrorMessage(error) ??
          'No fue posible generar las planeaciones.'
      )
    });
  }

  closeGenerationResult(): void {
    this.generationResult.set(null);
  }

  closeGenerationError(): void {
    this.generationError.set(null);
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
