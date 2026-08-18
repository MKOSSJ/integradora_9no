import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of } from 'rxjs';

import {
  Academia,
  Asignatura,
  Grupo,
  Periodo,
  UsuarioBackendListItem
} from '../models/admin-catalogos.model';
import { AcademiasService } from './academias.service';
import { AsignacionesAcademicasService } from './asignaciones-academicas.service';
import { AsignaturasService } from './asignaturas.service';
import { GruposService } from './grupos.service';
import { PeriodosService } from './periodos.service';
import { UsuariosService } from './usuarios.service';

describe('AsignacionesAcademicasService', () => {
  const periodo = {
    publicId: 'periodo-1', nombre: 'Periodo uno', estado: 'activo'
  } as Periodo;
  const grupo = {
    publicId: 'grupo-1', nombre: 'Grupo uno', estado: 'activo'
  } as Grupo;
  const asignatura = {
    publicId: 'asignatura-1', nombre: 'Asignatura uno', estado: 'activo'
  } as Asignatura;
  const academia = {
    publicId: 'academia-1', nombre: 'Academia uno', estado: 'activo'
  } as Academia;
  const docente = user(
    'docente-1', 'Docente', 'Backend', ['DOCENTE']
  );
  const revisor = user(
    'revisor-1', 'Revisor', 'Backend', ['DOCENTE', 'REVISOR']
  );
  const responseItem = {
    publicId: 'carga-1',
    periodoPublicId: periodo.publicId,
    grupoPublicId: grupo.publicId,
    asignaturaPublicId: asignatura.publicId,
    docentePublicId: docente.publicId,
    revisorPublicId: revisor.publicId,
    academiaPublicId: academia.publicId,
    activo: true
  };

  let service: AsignacionesAcademicasService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        AsignacionesAcademicasService,
        { provide: PeriodosService, useValue: { load: () => of([periodo]) } },
        { provide: GruposService, useValue: { load: () => of([grupo]) } },
        {
          provide: AsignaturasService,
          useValue: { load: () => of([asignatura]) }
        },
        { provide: AcademiasService, useValue: { load: () => of([academia]) } },
        {
          provide: UsuariosService,
          useValue: { load: () => of([docente, revisor]) }
        }
      ]
    });

    service = TestBed.inject(AsignacionesAcademicasService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('lista y ejecuta CRUD usando únicamente publicId reales', async () => {
    const loadPromise = firstValueFrom(service.load());
    http.expectOne(request =>
      request.method === 'GET' &&
      request.url.endsWith('/api/CargasAcademicas')
    ).flush(ok([responseItem]));

    const [loaded] = await loadPromise;
    expect(loaded).toMatchObject({
      source: 'backend',
      docenteNombre: 'Docente Backend',
      revisorNombre: 'Revisor Backend'
    });
    expect(service.docenteOptions).toEqual([
      { label: 'Docente Backend', value: docente.publicId },
      { label: 'Revisor Backend', value: revisor.publicId }
    ]);
    expect(service.revisorOptions).toEqual([
      { label: 'Revisor Backend', value: revisor.publicId }
    ]);

    const form = { ...loaded };
    const createPromise = firstValueFrom(service.create(form));
    const createRequest = http.expectOne(request =>
      request.method === 'POST' &&
      request.url.endsWith('/api/CargasAcademicas')
    );
    expect(createRequest.request.body).toEqual({
      periodoPublicId: periodo.publicId,
      grupoPublicId: grupo.publicId,
      asignaturaPublicId: asignatura.publicId,
      docentePublicId: docente.publicId,
      revisorPublicId: revisor.publicId,
      academiaPublicId: academia.publicId
    });
    createRequest.flush(ok({ ...responseItem, publicId: 'carga-2' }));
    expect((await createPromise).source).toBe('backend');

    const updatePromise = firstValueFrom(service.update(form));
    http.expectOne(request =>
      request.method === 'PUT' &&
      request.url.endsWith('/api/CargasAcademicas/carga-1')
    ).flush(ok(responseItem));
    expect((await updatePromise).publicId).toBe('carga-1');

    const deletePromise = firstValueFrom(service.delete(form));
    http.expectOne(request =>
      request.method === 'DELETE' &&
      request.url.endsWith('/api/CargasAcademicas/carga-1')
    ).flush(ok(true));
    expect(await deletePromise).toBe(true);
  });

  function user(
    publicId: string,
    nombre: string,
    apellidoPaterno: string,
    roles: UsuarioBackendListItem['roles']
  ): UsuarioBackendListItem {
    return {
      source: 'backend',
      id: publicId,
      publicId,
      nombre,
      apellidoPaterno,
      apellidoMaterno: '',
      email: '',
      telefono: '',
      ultimoAcceso: '',
      roles,
      academiaNombre: 'Sin información',
      rolEnAcademia: 'Sin información',
      estado: 'Sin información'
    };
  }

  function ok<T>(data: T) {
    return { success: true, data, message: null, errors: null };
  }
});
