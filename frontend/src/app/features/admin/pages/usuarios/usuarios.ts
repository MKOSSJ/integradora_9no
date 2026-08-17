import { Component, inject } from '@angular/core';
import { UsuariosService } from '../../../../core/services/usuarios.service';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';

const NEUTRAL_OPTIONS = [
  { label: 'Sin información', value: 'Sin información' }
];

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './usuarios.html',
  styleUrl: './usuarios.css'
})
export class Usuarios {
  private readonly usuariosService = inject(UsuariosService);

  config: AdminCrudConfig = {
    title: 'Usuarios',
    subtitle: 'Consulta los usuarios registrados actualmente en el sistema.',
    sectionLabel: 'Administración',
    addLabel: 'Nuevo usuario',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'usuarios',
    initialItems: [],
    dataSource: this.usuariosService,
    blockedCreateMessage: 'La creación de perfiles continúa local y separada porque el backend no expone un endpoint compatible.',
    blockedDeleteMessage: 'La baja de perfiles está bloqueada porque el backend no expone un endpoint compatible.',
    successMessages: {
      create: '',
      update: 'Roles del usuario actualizados correctamente.',
      delete: ''
    },
    columns: [{ key:'nombre', label:'Nombre' },{ key:'apellidoPaterno', label:'Apellido paterno' },{ key:'email', label:'Correo' },{ key:'telefono', label:'Teléfono' },{ key:'roles', label:'Roles', kind:'chips' },{ key:'academiaNombre', label:'Academia' },{ key:'rolEnAcademia', label:'Rol academia' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [
      { key:'nombre', label:'Nombre', type:'text', readonlyWhen:() => true },
      { key:'apellidoPaterno', label:'Apellido paterno', type:'text', readonlyWhen:() => true },
      { key:'apellidoMaterno', label:'Apellido materno', type:'text', readonlyWhen:() => true },
      { key:'email', label:'Correo electrónico', type:'email', readonlyWhen:() => true },
      { key:'telefono', label:'Teléfono', type:'tel', readonlyWhen:() => true },
      { key:'roles', label:'Roles del sistema', type:'multiselect', span:'full', options:this.usuariosService.roleOptions },
      { key:'academiaNombre', label:'Academia', type:'select', options:NEUTRAL_OPTIONS, readonlyWhen:() => true },
      { key:'rolEnAcademia', label:'Rol dentro de academia', type:'select', options:NEUTRAL_OPTIONS, readonlyWhen:() => true },
      { key:'estado', label:'Estado', type:'select', options:NEUTRAL_OPTIONS, readonlyWhen:() => true }
    ],
    counters: [{label:'Total',valueKey:'total'},{label:'Docentes',valueKey:'docentes',tone:'green'},{label:'Revisores',valueKey:'revisores',tone:'cyan'},{label:'Directivos',valueKey:'directivos',tone:'amber'}],
    searchKeys: ['nombre','apellidoPaterno','apellidoMaterno','email','telefono','academiaNombre']
  };
}
