import { Component, inject } from '@angular/core';
import { AsignaturasService } from '../../../../core/services/asignaturas.service';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';
import { STATUS_OPTIONS } from '../../shared/admin-data';

@Component({
  selector: 'app-asignaturas',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './asignaturas.html',
  styleUrl: './asignaturas.css'
})
export class Asignaturas {
  private readonly asignaturasService = inject(AsignaturasService);

  config: AdminCrudConfig = {
    title: 'Asignaturas',
    subtitle: 'Administra clave, cuatrimestre, horas, créditos y academia asociada.',
    sectionLabel: 'Administración',
    addLabel: 'Nueva asignatura',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'asignaturas',
    initialItems: [],
    dataSource: this.asignaturasService,
    successMessages: {
      create: 'Asignatura creada correctamente.',
      update: 'Asignatura actualizada correctamente.',
      delete: 'Asignatura dada de baja correctamente.'
    },
    columns: [{ key:'nombre', label:'Asignatura' },{ key:'clave', label:'Clave' },{ key:'academiaNombre', label:'Academia' },{ key:'cuatrimestre', label:'Cuatrimestre' },{ key:'horasTotales', label:'Horas totales' },{ key:'horasSemana', label:'Horas semana' },{ key:'creditos', label:'Créditos' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'nombre', label:'Nombre', type:'text', required:true, maxLength:200 },{ key:'clave', label:'Clave', type:'text', required:true, maxLength:50 },{ key:'academiaPublicId', label:'Academia', type:'select', options:this.asignaturasService.academiaOptions },{ key:'cuatrimestre', label:'Cuatrimestre', type:'number', required:true, min:1, max:2147483647, step:1 },{ key:'horasTotales', label:'Horas totales', type:'number', required:true, min:1, max:2147483647, step:1 },{ key:'horasSemana', label:'Horas semana', type:'number', required:true, min:1, max:2147483647, step:1 },{ key:'creditos', label:'Créditos', type:'number', min:0, max:999.99, step:0.01 },{ key:'estado', label:'Estado', type:'select', required:true, options:STATUS_OPTIONS }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activas',valueKey:'activos',tone:'green'},{label:'Inactivas',valueKey:'inactivos'}],
    searchKeys: ['nombre','clave','academiaNombre','cuatrimestre']
  };
}
