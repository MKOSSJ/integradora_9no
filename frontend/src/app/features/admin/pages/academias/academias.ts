import { Component } from '@angular/core';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';
import { ACADEMIAS, ASIGNATURAS, CARGA_ACADEMICA, CARRERAS, CICLOS, GRUPOS, PERIODOS, ROLE_OPTIONS, ROL_ACADEMIA_OPTIONS, STATUS_OPTIONS, USUARIOS } from '../../shared/admin-data';

@Component({
  selector: 'app-academias',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './academias.html',
  styleUrl: './academias.css'
})
export class Academias {
  config: AdminCrudConfig = {
    title: 'Academias',
    subtitle: 'Administra academias con nombre, descripción y relación futura con usuarios/asignaturas.',
    sectionLabel: 'Carga académica',
    addLabel: 'Nueva academia',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'academias',
    initialItems: ACADEMIAS,
    columns: [{ key:'nombre', label:'Nombre' },{ key:'descripcion', label:'Descripción' },{ key:'totalUsuarios', label:'Usuarios' },{ key:'totalAsignaturas', label:'Asignaturas' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'nombre', label:'Nombre', type:'text', required:true },{ key:'descripcion', label:'Descripción', type:'textarea', required:true, span:'full' },{ key:'estado', label:'Estado', type:'select', required:true, options:STATUS_OPTIONS }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activas',valueKey:'activos',tone:'green'},{label:'Inactivas',valueKey:'inactivos'}],
    searchKeys: ['nombre','descripcion']
  };
}
