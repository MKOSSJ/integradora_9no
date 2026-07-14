import { Component } from '@angular/core';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';
import { ACADEMIAS, ASIGNATURAS, CARGA_ACADEMICA, CARRERAS, CICLOS, GRUPOS, PERIODOS, ROLE_OPTIONS, ROL_ACADEMIA_OPTIONS, STATUS_OPTIONS, USUARIOS } from '../../shared/admin-data';

@Component({
  selector: 'app-asignaturas',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './asignaturas.html',
  styleUrl: './asignaturas.css'
})
export class Asignaturas {
  config: AdminCrudConfig = {
    title: 'Asignaturas',
    subtitle: 'Administra clave, cuatrimestre, horas, créditos y academia asociada.',
    sectionLabel: 'Administración',
    addLabel: 'Nueva asignatura',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'asignaturas',
    initialItems: ASIGNATURAS,
    columns: [{ key:'nombre', label:'Asignatura' },{ key:'clave', label:'Clave' },{ key:'academiaNombre', label:'Academia' },{ key:'cuatrimestre', label:'Cuatrimestre' },{ key:'horasTotales', label:'Horas totales' },{ key:'horasSemana', label:'Horas semana' },{ key:'creditos', label:'Créditos' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'nombre', label:'Nombre', type:'text', required:true },{ key:'clave', label:'Clave', type:'text', required:true },{ key:'academiaNombre', label:'Academia', type:'select', options:ACADEMIAS.map(a=>({label:a.nombre,value:a.nombre})) },{ key:'cuatrimestre', label:'Cuatrimestre', type:'text', required:true },{ key:'horasTotales', label:'Horas totales', type:'number', required:true },{ key:'horasSemana', label:'Horas semana', type:'number', required:true },{ key:'creditos', label:'Créditos', type:'number', required:true },{ key:'estado', label:'Estado', type:'select', required:true, options:STATUS_OPTIONS }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activas',valueKey:'activos',tone:'green'},{label:'Inactivas',valueKey:'inactivos'}],
    searchKeys: ['nombre','clave','academiaNombre','cuatrimestre']
  };
}
