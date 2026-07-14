import { Component } from '@angular/core';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';
import { ACADEMIAS, ASIGNATURAS, CARGA_ACADEMICA, CARRERAS, CICLOS, GRUPOS, PERIODOS, ROLE_OPTIONS, ROL_ACADEMIA_OPTIONS, STATUS_OPTIONS, USUARIOS } from '../../shared/admin-data';

@Component({
  selector: 'app-asignacion-academica',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './asignacion-academica.html',
  styleUrl: './asignacion-academica.css'
})
export class AsignacionAcademica {
  config: AdminCrudConfig = {
    title: 'Asignación Académica',
    subtitle: 'Define periodo, grupo, asignatura, docente, revisor y academia.',
    sectionLabel: 'Carga académica',
    addLabel: 'Nueva asignación',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'asignacion-academica',
    initialItems: CARGA_ACADEMICA,
    columns: [{ key:'periodoNombre', label:'Periodo' },{ key:'grupoNombre', label:'Grupo' },{ key:'asignaturaNombre', label:'Asignatura' },{ key:'docenteNombre', label:'Docente' },{ key:'revisorNombre', label:'Revisor' },{ key:'academiaNombre', label:'Academia' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'periodoNombre', label:'Periodo', type:'select', required:true, options:PERIODOS.map(p=>({label:p.nombre,value:p.nombre})) },{ key:'grupoNombre', label:'Grupo', type:'select', required:true, options:GRUPOS.map(g=>({label:g.nombre,value:g.nombre})) },{ key:'asignaturaNombre', label:'Asignatura', type:'select', required:true, options:ASIGNATURAS.map(a=>({label:a.nombre,value:a.nombre})) },{ key:'docenteNombre', label:'Docente', type:'select', required:true, options:USUARIOS.filter(u=>u.roles.includes('DOCENTE')).map(u=>({label:`${u.nombre} ${u.apellidoPaterno}`,value:`${u.nombre} ${u.apellidoPaterno}`})) },{ key:'revisorNombre', label:'Revisor', type:'select', options:USUARIOS.filter(u=>u.roles.includes('REVISOR')||u.roles.includes('DIRECTIVO')).map(u=>({label:`${u.nombre} ${u.apellidoPaterno}`,value:`${u.nombre} ${u.apellidoPaterno}`})) },{ key:'academiaNombre', label:'Academia', type:'select', options:ACADEMIAS.map(a=>({label:a.nombre,value:a.nombre})) },{ key:'estado', label:'Estado', type:'select', required:true, options:STATUS_OPTIONS }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activas',valueKey:'activos',tone:'green'},{label:'Inactivas',valueKey:'inactivos'}],
    searchKeys: ['periodoNombre','grupoNombre','asignaturaNombre','docenteNombre','revisorNombre','academiaNombre']
  };
}
