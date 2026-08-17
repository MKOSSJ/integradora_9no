import { Component } from '@angular/core';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';
import { ACADEMIAS, ASIGNATURAS, CARGA_ACADEMICA, CARRERAS, CICLOS, GRUPOS, PERIODOS, ROLE_OPTIONS, ROL_ACADEMIA_OPTIONS, STATUS_OPTIONS, USUARIOS } from '../../shared/admin-data';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './usuarios.html',
  styleUrl: './usuarios.css'
})
export class Usuarios {
  config: AdminCrudConfig = {
    title: 'Usuarios',
    subtitle: 'Gestiona usuarios con múltiples roles y relación con academias.',
    sectionLabel: 'Administración',
    addLabel: 'Nuevo usuario',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'usuarios',
    initialItems: USUARIOS,
    columns: [{ key:'nombre', label:'Nombre' },{ key:'apellidoPaterno', label:'Apellido paterno' },{ key:'email', label:'Correo' },{ key:'telefono', label:'Teléfono' },{ key:'roles', label:'Roles', kind:'chips' },{ key:'academiaNombre', label:'Academia' },{ key:'rolEnAcademia', label:'Rol academia' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'nombre', label:'Nombre', type:'text', required:true },{ key:'apellidoPaterno', label:'Apellido paterno', type:'text', required:true },{ key:'apellidoMaterno', label:'Apellido materno', type:'text' },{ key:'email', label:'Correo electrónico', type:'email', required:true },{ key:'telefono', label:'Teléfono', type:'tel' },{ key:'roles', label:'Roles del sistema', type:'multiselect', required:true, span:'full', options:ROLE_OPTIONS },{ key:'academiaNombre', label:'Academia', type:'select', options:ACADEMIAS.map(a=>({label:a.nombre,value:a.nombre})) },{ key:'rolEnAcademia', label:'Rol dentro de academia', type:'select', options:ROL_ACADEMIA_OPTIONS },{ key:'estado', label:'Estado', type:'select', required:true, options:STATUS_OPTIONS }],
    counters: [{label:'Total',valueKey:'total'},{label:'Docentes',valueKey:'docentes',tone:'green'},{label:'Revisores',valueKey:'revisores',tone:'cyan'},{label:'Directivos',valueKey:'directivos',tone:'amber'}],
    searchKeys: ['nombre','apellidoPaterno','apellidoMaterno','email','telefono','academiaNombre']
  };
}
