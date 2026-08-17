import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom, map, Observable } from 'rxjs';

import { environment } from '../../environments/environments';
import { ApiResponseDto } from '../dto/api-response.dto';
import { ImportacionCargaAcademicaResultadoDto } from '../dto/cargas-academicas/importacion-carga-academica.dto';
import { Periodo } from '../models/admin-catalogos.model';
import { PeriodosService } from './periodos.service';

type PreviewRow = Record<string, any>;
type PeriodOption = { label: string; value: string };

@Injectable({ providedIn: 'root' })
export class CargasAcademicasImportService {
  private readonly http = inject(HttpClient);
  private readonly periodosService = inject(PeriodosService);
  private readonly endpoint =
    `${environment.apiUrl}/api/CargasAcademicas/importar`;
  private selectedFile: File | null = null;
  private selectedPeriodoPublicId = '';
  private lastPreview: PreviewRow[] = [];

  readonly dataSource = {
    validate: (file: File) => this.validate(file),
    import: (_items: PreviewRow[]) => this.importSelectedFile(),
    downloadTemplate: () => this.downloadTemplate()
  };

  loadPeriodOptions(): Observable<PeriodOption[]> {
    return this.periodosService.load().pipe(
      map(periodos => periodos
        .filter(periodo => periodo.estado === 'activo')
        .map(periodo => ({
          value: periodo.publicId,
          label: this.periodLabel(periodo)
        })))
    );
  }

  setPeriodoPublicId(publicId: string): void {
    this.selectedPeriodoPublicId = publicId.trim();
  }

  private async validate(file: File): Promise<PreviewRow[]> {
    const extension = this.extension(file.name);

    if (!['csv', 'xls', 'xlsx'].includes(extension)) {
      throw new Error(
        'Formato de archivo no válido. Utiliza .csv, .xls o .xlsx.'
      );
    }

    const rows = extension === 'csv'
      ? this.parseCsv(await file.text())
      : await this.readFirstExcelSheet(file);
    const nonEmptyRows = rows.filter(row =>
      row.some(value => value.trim() !== '')
    );

    if (nonEmptyRows.length === 0) {
      throw new Error('El archivo está vacío o no contiene filas legibles.');
    }

    const preview = [{
      id: 'archivo-seleccionado',
      archivo: file.name,
      formato: extension.toUpperCase(),
      filas: nonEmptyRows.length,
      fila: '',
      campo: '',
      valor: '',
      mensaje: 'Archivo legible y listo para enviar.',
      estado: 'validado',
      observacion: 'El backend realizará la validación definitiva.'
    }];

    this.selectedFile = file;
    this.lastPreview = preview.map(item => ({ ...item }));
    return preview;
  }

  private async importSelectedFile(): Promise<{
    message: string;
    type: 'success' | 'error';
    items: PreviewRow[];
  }> {
    if (!this.selectedFile) {
      throw new Error('Selecciona y valida un archivo antes de importarlo.');
    }

    if (!this.selectedPeriodoPublicId) {
      throw new Error('Selecciona el periodo de la carga académica.');
    }

    const form = new FormData();
    form.append('File', this.selectedFile, this.selectedFile.name);
    form.append('PeriodoPublicId', this.selectedPeriodoPublicId);

    const apiResponse = await firstValueFrom(
      this.http.post<ApiResponseDto<ImportacionCargaAcademicaResultadoDto>>(
        this.endpoint,
        form
      )
    );
    const response = this.unwrap(apiResponse);
    const selected = this.lastPreview[0] ?? {};
    const hasErrors = response.errores.length > 0;
    const items = hasErrors
      ? response.errores.map((error, index) => ({
          id: `backend-error-${error.fila}-${index}`,
          archivo: selected['archivo'] ?? this.selectedFile?.name ?? '',
          formato: selected['formato'] ?? '',
          filas: response.totalFilas,
          fila: error.fila,
          campo: error.campo,
          valor: error.valor ?? '',
          mensaje: error.mensaje,
          estado: 'error',
          observacion: error.mensaje
        }))
      : [{
          ...selected,
          filas: response.totalFilas,
          mensaje: apiResponse.message ?? 'Importación finalizada.',
          estado: 'validado',
          observacion: 'El backend procesó el archivo correctamente.'
        }];

    return {
      type: hasErrors ? 'error' : 'success',
      message: [
        apiResponse.message ?? 'Importación de carga académica finalizada.',
        `Filas: ${response.totalFilas}.`,
        `Procesadas: ${response.procesadas}.`,
        `Insertadas: ${response.insertadas}.`,
        `Omitidas: ${response.omitidas}.`
      ].join(' '),
      items
    };
  }

  private parseCsv(text: string): string[][] {
    const normalizedText = text.replace(/^\uFEFF/, '');
    const delimiter = normalizedText.split(';').length >
      normalizedText.split(',').length ? ';' : ',';
    const rows: string[][] = [];
    let row: string[] = [];
    let value = '';
    let quoted = false;

    for (let index = 0; index < normalizedText.length; index += 1) {
      const character = normalizedText[index];

      if (character === '"') {
        if (quoted && normalizedText[index + 1] === '"') {
          value += '"';
          index += 1;
        } else {
          quoted = !quoted;
        }
      } else if (character === delimiter && !quoted) {
        row.push(value);
        value = '';
      } else if ((character === '\n' || character === '\r') && !quoted) {
        if (character === '\r' && normalizedText[index + 1] === '\n') {
          index += 1;
        }
        row.push(value);
        rows.push(row);
        row = [];
        value = '';
      } else {
        value += character;
      }
    }

    if (quoted) throw new Error('El archivo CSV contiene comillas sin cerrar.');
    if (value.length > 0 || row.length > 0) {
      row.push(value);
      rows.push(row);
    }

    return rows;
  }

  private async readFirstExcelSheet(file: File): Promise<string[][]> {
    try {
      const { read, utils } = await import('xlsx');
      const workbook = read(await file.arrayBuffer(), { type: 'array' });
      const firstSheetName = workbook.SheetNames[0];

      if (!firstSheetName) throw new Error('El libro no contiene hojas.');

      return utils.sheet_to_json<unknown[]>(
        workbook.Sheets[firstSheetName],
        { header: 1, defval: '', raw: false, blankrows: false }
      ).map(row => row.map(value => this.cellToString(value)));
    } catch {
      throw new Error('No fue posible leer el archivo Excel.');
    }
  }

  private downloadTemplate(): void {
    const content = 'Asignatura,Cuatrimestre,P.E.,Docente\r\n';
    const url = URL.createObjectURL(
      new Blob([content], { type: 'text/csv;charset=utf-8' })
    );
    const link = document.createElement('a');
    link.href = url;
    link.download = 'plantilla-carga-academica.csv';
    link.click();
    URL.revokeObjectURL(url);
  }

  private periodLabel(periodo: Periodo): string {
    return periodo.cicloEscolarNombre
      ? `${periodo.nombre} — ${periodo.cicloEscolarNombre}`
      : periodo.nombre;
  }

  private cellToString(value: unknown): string {
    if (value === null || value === undefined) return '';
    if (typeof value === 'number' && Number.isNaN(value)) return '';
    return String(value);
  }

  private extension(fileName: string): string {
    return fileName.split('.').pop()?.toLocaleLowerCase() ?? '';
  }

  private unwrap<T>(response: ApiResponseDto<T>): T {
    if (!response.success || response.data === null) {
      throw new Error(
        response.errors?.join(' ') ||
          response.message ||
          environment.defaultErrorMessage
      );
    }

    return response.data;
  }
}
