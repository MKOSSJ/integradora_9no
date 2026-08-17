import { Component, inject } from '@angular/core';
import { CiclosService } from '../../../../core/services/ciclos.service';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';

@Component({
  selector: 'app-ciclos',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './ciclos.html',
  styleUrl: './ciclos.css'
})
export class Ciclos {
  private readonly ciclosService = inject(CiclosService);

  config: AdminCrudConfig = {
    title: 'Ciclos Escolares',
    subtitle: 'Administra los ciclos escolares utilizados en la gestión académica.',
    sectionLabel: 'Administración',
    addLabel: 'Nuevo ciclo escolar',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'ciclos escolares',
    initialItems: [],
    dataSource: this.ciclosService,
    successMessages: {
      create: 'Ciclo escolar creado correctamente.',
      update: 'Ciclo escolar actualizado correctamente.',
      delete: 'Ciclo escolar dado de baja correctamente.'
    },
    columns: [{ key:'nombre', label:'Ciclo escolar' },{ key:'fechaInicio', label:'Inicio', kind:'date' },{ key:'fechaFin', label:'Fin', kind:'date' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'nombre', label:'Nombre', type:'text', required:true, maxLength:50 },{ key:'fechaInicio', label:'Fecha inicio', type:'date', required:true },{ key:'fechaFin', label:'Fecha fin', type:'date', required:true }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activos',valueKey:'activos',tone:'green'},{label:'Inactivos',valueKey:'inactivos'}],
    searchKeys: ['nombre']
  };
}
