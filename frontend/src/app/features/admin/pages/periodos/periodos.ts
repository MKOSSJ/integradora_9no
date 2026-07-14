import { Component } from '@angular/core';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';
import { ACADEMIAS, ASIGNATURAS, CARGA_ACADEMICA, CARRERAS, CICLOS, GRUPOS, PERIODOS, ROLE_OPTIONS, ROL_ACADEMIA_OPTIONS, STATUS_OPTIONS, USUARIOS } from '../../shared/admin-data';

@Component({
  selector: 'app-periodos',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './periodos.html',
  styleUrl: './periodos.css'
})
export class Periodos {
  config: AdminCrudConfig = {
    title: 'Ciclos y Periodos',
    subtitle: 'Administra ciclos escolares y periodos asociados.',
    sectionLabel: 'Administración',
    addLabel: 'Nuevo periodo',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'periodos',
    initialItems: PERIODOS,
    columns: [{ key:'nombre', label:'Periodo' },{ key:'cicloEscolarNombre', label:'Ciclo escolar' },{ key:'fechaInicio', label:'Inicio', kind:'date' },{ key:'fechaFin', label:'Fin', kind:'date' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'cicloEscolarNombre', label:'Ciclo escolar', type:'select', required:true, options:CICLOS.map(c=>({label:c.nombre,value:c.nombre})) },{ key:'nombre', label:'Nombre del periodo', type:'text', required:true },{ key:'fechaInicio', label:'Fecha inicio', type:'date', required:true },{ key:'fechaFin', label:'Fecha fin', type:'date', required:true },{ key:'estado', label:'Estado', type:'select', required:true, options:STATUS_OPTIONS }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activos',valueKey:'activos',tone:'green'},{label:'Inactivos',valueKey:'inactivos'}],
    searchKeys: ['nombre','cicloEscolarNombre']
  };
}
