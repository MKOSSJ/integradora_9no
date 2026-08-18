import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { UsuariosService } from './usuarios.service';

describe('UsuariosService', () => {
  let service: UsuariosService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        UsuariosService
      ]
    });

    service = TestBed.inject(UsuariosService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('carga identidad y roles reales sin inventar estado', async () => {
    const resultPromise = firstValueFrom(service.load());
    const usersRequest = http.expectOne(value =>
      value.url.endsWith('/api/Usuario/GetAll')
    );
    const catalogRequest = http.expectOne(value =>
      value.url.endsWith('/api/usuarios-roles/catalogo')
    );

    usersRequest.flush([{
      publicId: 'usuario-1',
      nombre: 'Ada',
      apellidoPaterno: 'Lovelace',
      apellidoMaterno: ' STRING ',
      email: 'ada@example.edu',
      telefono: ' string ',
      ultimoAcceso: null
    }]);
    catalogRequest.flush({
      success: true,
      data: [
        {
          publicId: 'rol-docente',
          nombre: 'Docente',
          descripcion: null
        },
        {
          publicId: 'rol-revisor',
          nombre: 'Revisor',
          descripcion: null
        }
      ],
      message: null,
      errors: null
    });
    http.expectOne(value =>
      value.url.endsWith('/api/usuarios-roles/usuario-1')
    ).flush({
      success: true,
      data: {
        usuarioPublicId: 'usuario-1',
        usuario: 'Ada Lovelace',
        roles: [{
          publicId: 'rol-docente',
          nombre: 'Docente',
          descripcion: null
        }]
      },
      message: null,
      errors: null
    });

    const [user] = await resultPromise;
    expect(user).toMatchObject({
      source: 'backend',
      id: 'usuario-1',
      publicId: 'usuario-1',
      nombre: 'Ada',
      apellidoPaterno: 'Lovelace',
      apellidoMaterno: '',
      email: 'ada@example.edu',
      telefono: '',
      roles: ['DOCENTE'],
      estado: 'Sin información'
    });
    expect(service.roleOptions).toEqual([
      { label: 'Docente', value: 'DOCENTE' },
      { label: 'Revisor', value: 'REVISOR' }
    ]);

    const updatePromise = firstValueFrom(service.update({
      ...user,
      roles: ['DOCENTE', 'REVISOR']
    }));
    http.expectOne(value =>
      value.method === 'GET' &&
      value.url.endsWith('/api/usuarios-roles/usuario-1')
    ).flush({
      success: true,
      data: {
        usuarioPublicId: 'usuario-1',
        usuario: 'Ada Lovelace',
        roles: [
          { publicId: 'rol-docente', nombre: 'Docente', descripcion: null }
        ]
      },
      message: null,
      errors: null
    });
    const assignRequest = http.expectOne(value =>
      value.method === 'POST' &&
      value.url.endsWith('/api/usuarios-roles/usuario-1')
    );
    expect(assignRequest.request.body).toEqual({
      rolPublicId: 'rol-revisor'
    });
    assignRequest.flush({
      success: true,
      data: {
        usuarioPublicId: 'usuario-1',
        usuario: 'Ada Lovelace',
        roles: [
          { publicId: 'rol-docente', nombre: 'Docente', descripcion: null },
          { publicId: 'rol-revisor', nombre: 'Revisor', descripcion: null }
        ]
      },
      message: 'Rol asignado.',
      errors: null
    });
    http.expectOne(value =>
      value.method === 'GET' &&
      value.url.endsWith('/api/usuarios-roles/usuario-1')
    ).flush({
      success: true,
      data: {
        usuarioPublicId: 'usuario-1',
        usuario: 'Ada Lovelace',
        roles: [
          { publicId: 'rol-docente', nombre: 'Docente', descripcion: null },
          { publicId: 'rol-revisor', nombre: 'Revisor', descripcion: null }
        ]
      },
      message: null,
      errors: null
    });
    expect((await updatePromise).roles).toEqual(['DOCENTE', 'REVISOR']);

    const removePromise = firstValueFrom(service.update({
      ...user,
      roles: ['DOCENTE']
    }));
    http.expectOne(value =>
      value.method === 'GET' &&
      value.url.endsWith('/api/usuarios-roles/usuario-1')
    ).flush({
      success: true,
      data: {
        usuarioPublicId: 'usuario-1',
        usuario: 'Ada Lovelace',
        roles: [
          { publicId: 'rol-docente', nombre: 'Docente', descripcion: null },
          { publicId: 'rol-revisor', nombre: 'Revisor', descripcion: null }
        ]
      },
      message: null,
      errors: null
    });
    const removeRequest = http.expectOne(value =>
      value.method === 'DELETE' &&
      value.url.endsWith(
        '/api/usuarios-roles/usuario-1/rol-revisor'
      )
    );
    removeRequest.flush({
      success: true,
      data: {
        usuarioPublicId: 'usuario-1',
        usuario: 'Ada Lovelace',
        roles: [
          { publicId: 'rol-docente', nombre: 'Docente', descripcion: null }
        ]
      },
      message: 'Rol retirado.',
      errors: null
    });
    http.expectOne(value =>
      value.method === 'GET' &&
      value.url.endsWith('/api/usuarios-roles/usuario-1')
    ).flush({
      success: true,
      data: {
        usuarioPublicId: 'usuario-1',
        usuario: 'Ada Lovelace',
        roles: [
          { publicId: 'rol-docente', nombre: 'Docente', descripcion: null }
        ]
      },
      message: null,
      errors: null
    });
    expect((await removePromise).roles).toEqual(['DOCENTE']);

    const unchangedPromise = firstValueFrom(service.update({
      ...user,
      roles: ['DOCENTE']
    }));
    http.expectOne(value =>
      value.method === 'GET' &&
      value.url.endsWith('/api/usuarios-roles/usuario-1')
    ).flush({
      success: true,
      data: {
        usuarioPublicId: 'usuario-1',
        usuario: 'Ada Lovelace',
        roles: [
          { publicId: 'rol-docente', nombre: 'Docente', descripcion: null }
        ]
      },
      message: null,
      errors: null
    });
    await expect(unchangedPromise).rejects.toThrow(
      'No se detectaron cambios en los roles del usuario.'
    );
  });
});
