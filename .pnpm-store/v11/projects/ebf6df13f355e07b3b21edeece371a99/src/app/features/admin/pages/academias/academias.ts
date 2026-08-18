import { Component, inject } from '@angular/core';
import { AcademiasService } from '../../../../core/services/academias.service';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';
import { STATUS_OPTIONS } from '../../shared/admin-data';

@Component({
  selector: 'app-academias',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './academias.html',
  styleUrl: './academias.css'
})
export class Academias {
  private readonly academiasService = inject(AcademiasService);

  config: AdminCrudConfig = {
    title: 'Academias',
    subtitle: 'Administra academias con nombre, descripción y relación futura con usuarios/asignaturas.',
    sectionLabel: 'Carga académica',
    addLabel: 'Nueva academia',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'academias',
    initialItems: [],
    dataSource: this.academiasService,
    successMessages: {
      create: 'Academia creada correctamente.',
      update: 'Academia actualizada correctamente.',
      delete: 'Academia dada de baja correctamente.'
    },
    columns: [{ key:'nombre', label:'Nombre' },{ key:'descripcion', label:'Descripción' },{ key:'totalUsuarios', label:'Usuarios' },{ key:'totalAsignaturas', label:'Asignaturas' },{ key:'estado', label:'Estado', kind:'status' }],
    fields: [{ key:'nombre', label:'Nombre', type:'text', required:true, maxLength:150 },{ key:'descripcion', label:'Descripción', type:'textarea', maxLength:300, span:'full' },{ key:'estado', label:'Estado', type:'select', required:true, options:STATUS_OPTIONS }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activas',valueKey:'activos',tone:'green'},{label:'Inactivas',valueKey:'inactivos'}],
    searchKeys: ['nombre','descripcion']
  };
}
