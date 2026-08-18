import { EnvironmentInjector, runInInjectionContext } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of } from 'rxjs';
import { utils, write } from 'xlsx';

import {
  Academia,
  Asignatura,
  Grupo,
  Periodo
} from '../models/admin-catalogos.model';
import { AcademiasService } from './academias.service';
import { AsignacionesAcademicasLocalService } from './asignaciones-academicas-local.service';
import { AsignaturasService } from './asignaturas.service';
import { GruposService } from './grupos.service';
import { ImportacionesLocalService } from './importaciones-local.service';
import { PeriodosService } from './periodos.service';
import { UsuariosLocalService } from './usuarios-local.service';

describe('Flujo local temporal de Directivo', () => {
  const academia: Academia = {
    id: 'academia-1',
    publicId: 'academia-1',
    nombre: 'Academia de Desarrollo de Software',
    descripcion: '',
    estado: 'activo',
    totalUsuarios: 0,
    totalAsignaturas: 1
  };
  const periodo: Periodo = {
    id: 'periodo-1',
    publicId: 'periodo-1',
    cicloEscolarPublicId: 'ciclo-1',
    cicloEscolarNombre: '2026-2027',
    nombre: 'Agosto - Diciembre 2026',
    fechaInicio: '2026-08-01',
    fechaFin: '2026-12-31',
    estado: 'activo'
  };
  const grupo: Grupo = {
    id: 'grupo-1',
    publicId: 'grupo-1',
    nombre: 'TI-501',
    cuatrimestre: 5,
    carreraPublicId: 'carrera-1',
    carreraNombre: 'Ingeniería en TI',
    periodoPublicId: periodo.publicId,
    periodoNombre: periodo.nombre,
    estado: 'activo'
  };
  const asignatura: Asignatura = {
    id: 'asignatura-1',
    publicId: 'asignatura-1',
    academiaPublicId: academia.publicId,
    academiaNombre: academia.nombre,
    nombre: 'Programación Web',
    clave: 'PW-501',
    cuatrimestre: 5,
    horasTotales: 90,
    horasSemana: 6,
    creditos: 6,
    estado: 'activo'
  };

  beforeEach(() => {
    localStorage.removeItem(UsuariosLocalService.storageKey);
    localStorage.removeItem(AsignacionesAcademicasLocalService.storageKey);
    localStorage.removeItem(ImportacionesLocalService.storageKey);

    TestBed.configureTestingModule({
      providers: [
        UsuariosLocalService,
        AsignacionesAcademicasLocalService,
        ImportacionesLocalService,
        { provide: AcademiasService, useValue: { load: () => of([academia]) } },
        { provide: PeriodosService, useValue: { load: () => of([periodo]) } },
        { provide: GruposService, useValue: { load: () => of([grupo]) } },
        { provide: AsignaturasService, useValue: { load: () => of([asignatura]) } }
      ]
    });
  });

  afterEach(() => {
    localStorage.removeItem(UsuariosLocalService.storageKey);
    localStorage.removeItem(AsignacionesAcademicasLocalService.storageKey);
    localStorage.removeItem(ImportacionesLocalService.storageKey);
  });

  it('crea, edita, da de baja, importa y conserva usuarios/asignaciones al recrear services', async () => {
    const usersService = TestBed.inject(UsuariosLocalService);
    await firstValueFrom(usersService.load());

    const createdUser = await firstValueFrom(usersService.create({
      nombre: 'Usuario',
      apellidoPaterno: 'Temporal',
      apellidoMaterno: '',
      email: 'temporal@uth.edu.mx',
      telefono: '7711234567',
      roles: ['DOCENTE'],
      academiaPublicId: academia.publicId,
      rolEnAcademia: 'Docente',
      estado: 'activo'
    }));
    expect(createdUser.publicId).toMatch(/[0-9a-f-]{36}/i);
    expect(createdUser.source).toBe('local');

    const editedUser = await firstValueFrom(usersService.update({
      ...createdUser,
      nombre: 'Usuario Editado'
    }));
    expect(editedUser.nombre).toBe('Usuario Editado');
    await firstValueFrom(usersService.delete({ ...editedUser }));
    expect(usersService.getSnapshot().find(
      user => user.publicId === createdUser.publicId
    )?.estado).toBe('inactivo');

    const csv = [
      'nombre,apellido_paterno,apellido_materno,email,telefono,roles,academia,rol_academia',
      'Juan,Pérez,López,juan.importado@uth.edu.mx,7710001111,"DOCENTE,REVISOR",Academia de Desarrollo de Software,Revisor',
      'Duplicado,Archivo,,juan.importado@uth.edu.mx,,DOCENTE,Academia de Desarrollo de Software,Docente'
    ].join('\r\n');
    const file = {
      name: 'profesores.csv',
      text: async () => csv
    } as File;
    const importsService = TestBed.inject(ImportacionesLocalService);
    const preview = await importsService.profesoresDataSource.validate(file);
    expect(preview).toHaveLength(2);
    expect(preview[0]['estado']).toBe('validado');
    expect(preview[1]['estado']).toBe('error');
    await importsService.profesoresDataSource.import(
      preview.filter(item => item['estado'] === 'validado')
    );

    const usersAfterImport = await firstValueFrom(usersService.load());
    const imported = usersAfterImport.find(
      user => user.email === 'juan.importado@uth.edu.mx'
    );
    expect(imported?.roles).toEqual(['DOCENTE', 'REVISOR']);

    const assignmentsService = TestBed.inject(
      AsignacionesAcademicasLocalService
    );
    await firstValueFrom(assignmentsService.load());
    const createdAssignment = await firstValueFrom(assignmentsService.create({
      periodoPublicId: periodo.publicId,
      grupoPublicId: grupo.publicId,
      asignaturaPublicId: asignatura.publicId,
      docentePublicId: imported?.publicId,
      revisorPublicId: imported?.publicId,
      academiaPublicId: academia.publicId,
      estado: 'activo'
    }));
    expect(createdAssignment.docenteNombre).toBe('Juan Pérez López');
    expect(createdAssignment.source).toBe('local');

    const editedAssignment = await firstValueFrom(assignmentsService.update({
      ...createdAssignment,
      revisorPublicId: ''
    }));
    expect(editedAssignment.revisorNombre).toBe('');
    await firstValueFrom(assignmentsService.delete({ ...editedAssignment }));

    await firstValueFrom(usersService.update({
      ...imported,
      nombre: 'Juan Carlos'
    }));

    const injector = TestBed.inject(EnvironmentInjector);
    const freshUsers = runInInjectionContext(
      injector,
      () => new UsuariosLocalService()
    );
    const freshAssignments = runInInjectionContext(
      injector,
      () => new AsignacionesAcademicasLocalService()
    );
    const reloadedUsers = await firstValueFrom(freshUsers.load());
    const reloadedAssignments = await firstValueFrom(freshAssignments.load());

    expect(reloadedUsers.find(
      user => user.publicId === imported?.publicId
    )?.nombre).toBe('Juan Carlos');
    expect(reloadedAssignments.find(
      item => item.publicId === createdAssignment.publicId
    )).toMatchObject({
      estado: 'inactivo',
      docenteNombre: 'Juan Carlos Pérez López'
    });
    expect(localStorage.getItem(ImportacionesLocalService.storageKey)).toContain(
      'profesores'
    );
  });

  it('procesa Academias desde CSV, XLS y XLSX con el mismo preview y almacenamiento local', async () => {
    const importsService = TestBed.inject(ImportacionesLocalService);
    const formats = ['csv', 'xls', 'xlsx'] as const;

    for (const format of formats) {
      const suffix = format.toUpperCase();
      const rows = [
        ['nombre', 'descripcion'],
        [`Academia ${suffix}`, `Descripción ${suffix}`],
        [`Academia inválida ${suffix}`, '']
      ];
      const file = format === 'csv'
        ? ({
            name: `academias.${format}`,
            text: async () => rows.map(row => row.join(',')).join('\r\n')
          } as File)
        : createExcelFile(rows, format);

      const preview = await importsService.academiasDataSource.validate(file);

      expect(preview).toHaveLength(2);
      expect(preview[0]).toMatchObject({
        nombre: `Academia ${suffix}`,
        descripcion: `Descripción ${suffix}`,
        estado: 'validado'
      });
      expect(preview[1]['estado']).toBe('error');
      expect(preview[1]['observacion']).toContain('Falta descripción');

      await importsService.academiasDataSource.import(
        preview.filter(item => item['estado'] === 'validado')
      );
    }

    const stored = localStorage.getItem(ImportacionesLocalService.storageKey);
    expect(stored).toContain('Academia CSV');
    expect(stored).toContain('Academia XLS');
    expect(stored).toContain('Academia XLSX');
  });

  function createExcelFile(
    rows: string[][],
    format: 'xls' | 'xlsx'
  ): File {
    const workbook = utils.book_new();
    const sheet = utils.aoa_to_sheet(rows);
    utils.book_append_sheet(workbook, sheet, 'Academias');
    const bytes = write(workbook, { type: 'array', bookType: format });

    return new File([bytes], `academias.${format}`);
  }
});
