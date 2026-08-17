import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of } from 'rxjs';
import { utils, write } from 'xlsx';

import { Periodo } from '../models/admin-catalogos.model';
import { CargasAcademicasImportService } from './cargas-academicas-import.service';
import { PeriodosService } from './periodos.service';

describe('CargasAcademicasImportService', () => {
  const periodo: Periodo = {
    id: 'periodo-1',
    publicId: 'periodo-1',
    cicloEscolarPublicId: 'ciclo-1',
    cicloEscolarNombre: '2026-2027',
    nombre: 'Mayo - Agosto 2026',
    fechaInicio: '2026-05-01',
    fechaFin: '2026-08-31',
    estado: 'activo'
  };

  let service: CargasAcademicasImportService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        CargasAcademicasImportService,
        { provide: PeriodosService, useValue: { load: () => of([periodo]) } }
      ]
    });

    service = TestBed.inject(CargasAcademicasImportService);
    http = TestBed.inject(HttpTestingController);
    service.setPeriodoPublicId(periodo.publicId);
  });

  afterEach(() => http.verify());

  it('habilita CSV y XLSX legibles sin exigir encabezados específicos', async () => {
    const rows = [
      ['ACADEMIAS MAYO - AGOSTO 2026'],
      ['Columna institucional', 'Dato libre'],
      ['Valor uno', 'Valor dos']
    ];
    const csvFile = {
      name: 'carga.csv',
      text: async () => rows.map(row => row.join(',')).join('\r\n')
    } as File;
    const xlsxFile = createExcelFile(rows);

    for (const file of [csvFile, xlsxFile]) {
      const preview = await service.dataSource.validate(file);

      expect(preview).toHaveLength(1);
      expect(preview[0]).toMatchObject({
        archivo: file.name,
        filas: 3,
        estado: 'validado'
      });
    }
  });

  it('envía el archivo original y periodoPublicId mediante FormData', async () => {
    const file = new File(
      [[
        'ACADEMIAS MAYO - AGOSTO 2026',
        'Asignatura,Cuatrimestre,P.E.,Docente',
        'Cálculo Integral,3A,TIID,Mtro. Abel Martínez Reyes'
      ].join('\r\n')],
      'carga.csv',
      { type: 'text/csv' }
    );
    const preview = await service.dataSource.validate(file);
    const importPromise = service.dataSource.import(preview);
    const request = http.expectOne(candidate =>
      candidate.method === 'POST' &&
      candidate.url.endsWith('/api/CargasAcademicas/importar')
    );
    const form = request.request.body as FormData;
    const uploadedFile = form.get('File') as File;

    expect(uploadedFile.name).toBe(file.name);
    expect(uploadedFile.size).toBe(file.size);
    expect(form.get('PeriodoPublicId')).toBe(periodo.publicId);

    request.flush({
      success: true,
      data: {
        totalFilas: 1,
        procesadas: 1,
        insertadas: 1,
        omitidas: 0,
        errores: []
      },
      message: 'Importación finalizada.',
      errors: null
    });

    await expect(importPromise).resolves.toMatchObject({ type: 'success' });
  });

  it('habilita XLS legible y deja la validación definitiva al backend', async () => {
    const file = createExcelFile([
      ['Título institucional'],
      ['Cualquier columna'],
      ['Dato']
    ], 'xls');
    const preview = await service.dataSource.validate(file);

    expect(preview[0]).toMatchObject({
      formato: 'XLS',
      estado: 'validado'
    });
  });

  it('muestra fila, campo, valor y mensaje devueltos por la API', async () => {
    const file = new File(['dato libre'], 'carga.csv', { type: 'text/csv' });
    const preview = await service.dataSource.validate(file);
    const importPromise = service.dataSource.import(preview);
    const request = http.expectOne(candidate =>
      candidate.method === 'POST' &&
      candidate.url.endsWith('/api/CargasAcademicas/importar')
    );

    request.flush({
      success: true,
      data: {
        totalFilas: 1,
        procesadas: 0,
        insertadas: 0,
        omitidas: 1,
        errores: [{
          fila: 3,
          campo: 'Docente',
          valor: '',
          mensaje: 'El campo es obligatorio.'
        }]
      },
      message: 'Importación finalizada.',
      errors: null
    });

    await expect(importPromise).resolves.toMatchObject({
      type: 'error',
      items: [{
        fila: 3,
        campo: 'Docente',
        valor: '',
        mensaje: 'El campo es obligatorio.',
        estado: 'error'
      }]
    });
  });

  it('carga opciones reales de periodo con ciclo escolar', async () => {
    await expect(firstValueFrom(service.loadPeriodOptions())).resolves.toEqual([
      {
        value: periodo.publicId,
        label: 'Mayo - Agosto 2026 — 2026-2027'
      }
    ]);
  });

  function createExcelFile(
    rows: string[][],
    format: 'xls' | 'xlsx' = 'xlsx'
  ): File {
    const workbook = utils.book_new();
    utils.book_append_sheet(
      workbook,
      utils.aoa_to_sheet(rows),
      'Carga académica'
    );
    const bytes = write(workbook, { type: 'array', bookType: format });
    return new File([bytes], `carga.${format}`);
  }
});
