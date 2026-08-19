import { Component, inject } from '@angular/core';
import { throwError } from 'rxjs';
import { AcademiasDetalleService } from '../../../../core/services/academias-detalle.service';
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
  private readonly academiasDetalleService = inject(AcademiasDetalleService);

  config: AdminCrudConfig = {
    title: 'Academias',
    subtitle: 'Administra academias con nombre, descripción y relación futura con usuarios/asignaturas.',
    sectionLabel: 'Carga académica',
    addLabel: 'Nueva academia',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'academias',
    initialItems: [],
    dataSource: {
      load: () => this.academiasDetalleService.loadByPeriodo(),
      create: () => throwError(() => new Error('Esta vista solo consulta cargas académicas importadas.')),
      update: () => throwError(() => new Error('Esta vista solo consulta cargas académicas importadas.')),
      delete: () => throwError(() => new Error('Esta vista solo consulta cargas académicas importadas.'))
    },
    successMessages: {
      create: 'Academia creada correctamente.',
      update: 'Academia actualizada correctamente.',
      delete: 'Academia dada de baja correctamente.'
    },
    columns: [{ key:'periodo', label:'Periodo' },{ key:'registros', label:'Registros' },{ key:'estado', label:'Estado', kind:'status' }],
    rowDetails: {
      itemsKey: 'detalles',
      columns: [{ key:'asignatura', label:'Asignatura' },{ key:'cuatrimestre', label:'Cuatrimestre' },{ key:'programaEducativo', label:'P.E.' },{ key:'docente', label:'Docente' }],
      emptyMessage: 'No hay cargas académicas asociadas a este periodo.'
    },
    fields: [{ key:'nombre', label:'Nombre', type:'text', required:true, maxLength:150 },{ key:'descripcion', label:'Descripción', type:'textarea', maxLength:300, span:'full' },{ key:'estado', label:'Estado', type:'select', required:true, options:STATUS_OPTIONS }],
    counters: [{label:'Total',valueKey:'total'},{label:'Activas',valueKey:'activos',tone:'green'},{label:'Inactivas',valueKey:'inactivos'}],
    searchKeys: ['periodo'],
    blockedActionsMessage: 'Esta vista representa cargas académicas importadas por periodo. Administra el catálogo de academias desde su módulo correspondiente.',
    blockedCreateMessage: 'Esta vista representa cargas académicas importadas por periodo. “Nueva academia” no crea una importación.'
  };
}
