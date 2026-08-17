import { Component, inject } from '@angular/core';
import { GruposService } from '../../../../core/services/grupos.service';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';
import { STATUS_OPTIONS } from '../../shared/admin-data';

@Component({
  selector: 'app-grupos',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './grupos.html',
  styleUrl: './grupos.css'
})
export class Grupos {
  private readonly gruposService = inject(GruposService);

  config: AdminCrudConfig = {
    title: 'Grupos',
    subtitle: 'Administra grupos vinculados a carrera y periodo.',
    sectionLabel: 'Carga académica',
    addLabel: 'Nuevo grupo',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'grupos',
    initialItems: [],
    dataSource: this.gruposService,
    successMessages: {
      create: 'Grupo creado correctamente.',
      update: 'Grupo actualizado correctamente.',
      delete: 'Grupo dado de baja correctamente.'
    },
    columns: [{ key:'nombre', label:'Grupo' },{ key:'carreraNombre', label:'Carrera' },{ key:'periodoNombre', label:'Periodo' },{ key:'cuatrimestre', label:'Cuatrimestre' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'nombre', label:'Nombre del grupo', type:'text', required:true, maxLength:50 },{ key:'cuatrimestre', label:'Cuatrimestre', type:'number', required:true, min:1, max:2147483647, step:1 },{ key:'carreraPublicId', label:'Carrera', type:'select', required:true, options:this.gruposService.carreraOptions },{ key:'periodoPublicId', label:'Periodo', type:'select', required:true, options:this.gruposService.periodoOptions },{ key:'estado', label:'Estado', type:'select', required:true, options:STATUS_OPTIONS }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activos',valueKey:'activos',tone:'green'},{label:'Inactivos',valueKey:'inactivos'}],
    searchKeys: ['nombre','carreraNombre','periodoNombre','cuatrimestre']
  };
}
