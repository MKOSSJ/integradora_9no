import { Component } from '@angular/core';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';
import { ACADEMIAS, ASIGNATURAS, CARGA_ACADEMICA, CARRERAS, CICLOS, GRUPOS, PERIODOS, ROLE_OPTIONS, ROL_ACADEMIA_OPTIONS, STATUS_OPTIONS, USUARIOS } from '../../shared/admin-data';

@Component({
  selector: 'app-carreras',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './carreras.html',
  styleUrl: './carreras.css'
})
export class Carreras {
  config: AdminCrudConfig = {
    title: 'Carreras',
    subtitle: 'Administra los programas educativos utilizados en grupos y carga académica.',
    sectionLabel: 'Administración',
    addLabel: 'Nueva carrera',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'carreras',
    initialItems: CARRERAS,
    columns: [{ key:'nombre', label:'Nombre' },{ key:'clave', label:'Clave' },{ key:'nivel', label:'Nivel' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'nombre', label:'Nombre', type:'text', required:true },{ key:'clave', label:'Clave', type:'text', required:true },{ key:'nivel', label:'Nivel', type:'select', required:true, options:[{label:'TSU',value:'TSU'},{label:'Ingeniería',value:'Ingeniería'},{label:'Licenciatura',value:'Licenciatura'}] },{ key:'estado', label:'Estado', type:'select', required:true, options:STATUS_OPTIONS }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activas',valueKey:'activos',tone:'green'},{label:'Inactivas',valueKey:'inactivos'}],
    searchKeys: ['nombre','clave','nivel']
  };
}
