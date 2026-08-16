import { Component, inject } from '@angular/core';
import { CarrerasService } from '../../../../core/services/carreras.service';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';
import { STATUS_OPTIONS } from '../../shared/admin-data';

@Component({
  selector: 'app-carreras',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './carreras.html',
  styleUrl: './carreras.css'
})
export class Carreras {
  private readonly carrerasService = inject(CarrerasService);

  config: AdminCrudConfig = {
    title: 'Carreras',
    subtitle: 'Administra los programas educativos utilizados en grupos y carga académica.',
    sectionLabel: 'Administración',
    addLabel: 'Nueva carrera',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'carreras',
    initialItems: [],
    dataSource: this.carrerasService,
    successMessages: {
      create: 'Carrera creada correctamente.',
      update: 'Carrera actualizada correctamente.',
      delete: 'Carrera dada de baja correctamente.'
    },
    columns: [{ key:'nombre', label:'Nombre' },{ key:'clave', label:'Clave' },{ key:'nivel', label:'Nivel' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'nombre', label:'Nombre', type:'text', required:true, maxLength:200 },{ key:'clave', label:'Clave', type:'text', required:true, maxLength:50 },{ key:'nivel', label:'Nivel', type:'select', required:true, options:[{label:'TSU',value:'TSU'},{label:'Ingeniería',value:'Ingeniería'},{label:'Licenciatura',value:'Licenciatura'}] },{ key:'estado', label:'Estado', type:'select', required:true, options:STATUS_OPTIONS }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activas',valueKey:'activos',tone:'green'},{label:'Inactivas',valueKey:'inactivos'}],
    searchKeys: ['nombre','clave','nivel']
  };
}
