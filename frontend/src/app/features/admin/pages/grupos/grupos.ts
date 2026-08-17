import { Component } from '@angular/core';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';
import { ACADEMIAS, ASIGNATURAS, CARGA_ACADEMICA, CARRERAS, CICLOS, GRUPOS, PERIODOS, ROLE_OPTIONS, ROL_ACADEMIA_OPTIONS, STATUS_OPTIONS, USUARIOS } from '../../shared/admin-data';

@Component({
  selector: 'app-grupos',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './grupos.html',
  styleUrl: './grupos.css'
})
export class Grupos {
  config: AdminCrudConfig = {
    title: 'Grupos',
    subtitle: 'Administra grupos vinculados a carrera y periodo.',
    sectionLabel: 'Carga académica',
    addLabel: 'Nuevo grupo',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'grupos',
    initialItems: GRUPOS,
    columns: [{ key:'nombre', label:'Grupo' },{ key:'carreraNombre', label:'Carrera' },{ key:'periodoNombre', label:'Periodo' },{ key:'cuatrimestre', label:'Cuatrimestre' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'nombre', label:'Nombre del grupo', type:'text', required:true },{ key:'cuatrimestre', label:'Cuatrimestre', type:'text', required:true },{ key:'carreraNombre', label:'Carrera', type:'select', required:true, options:CARRERAS.map(c=>({label:c.nombre,value:c.nombre})) },{ key:'periodoNombre', label:'Periodo', type:'select', required:true, options:PERIODOS.map(p=>({label:p.nombre,value:p.nombre})) },{ key:'estado', label:'Estado', type:'select', required:true, options:STATUS_OPTIONS }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activos',valueKey:'activos',tone:'green'},{label:'Inactivos',valueKey:'inactivos'}],
    searchKeys: ['nombre','carreraNombre','periodoNombre','cuatrimestre']
  };
}
