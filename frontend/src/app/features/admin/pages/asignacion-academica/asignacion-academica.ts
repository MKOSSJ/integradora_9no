import { Component, inject } from '@angular/core';
import { PlaneacionesDirectivoService } from '../../../../core/services/planeaciones-directivo.service';
import { AdminCrudPage } from '../../shared/admin-crud-page/admin-crud-page';
import { AdminCrudConfig } from '../../shared/admin-crud-page/admin-crud.types';

@Component({
  selector: 'app-asignacion-academica',
  standalone: true,
  imports: [AdminCrudPage],
  templateUrl: './asignacion-academica.html',
  styleUrl: './asignacion-academica.css'
})
export class AsignacionAcademica {
  private readonly planeacionesService = inject(
    PlaneacionesDirectivoService
  );

  config: AdminCrudConfig = {
    title: 'Asignación de Revisores',
    subtitle: 'Asigna o cambia el revisor global de las planeaciones generadas durante la sesión actual.',
    sectionLabel: 'Carga académica',
    addLabel: 'Nueva asignación',
    searchPlaceholder: 'Buscar...',
    entityLabel: 'asignacion-revisores',
    initialItems: [],
    dataSource: this.planeacionesService,
    blockedCreateMessage: 'Las planeaciones se crean desde Importaciones mediante Generar planeaciones.',
    blockedDeleteMessage: 'El backend no expone una baja de planeación dentro de este flujo.',
    successMessages: {
      create: '',
      update: 'Revisor asignado correctamente.',
      delete: ''
    },
    columns: [
      { key: 'asignatura', label: 'Asignatura' },
      { key: 'docente', label: 'Docente' },
      { key: 'estado', label: 'Estado', kind: 'status' },
      { key: 'revisorNombre', label: 'Revisor asignado' }
    ],
    fields: [
      {
        key: 'asignatura', label: 'Asignatura', type: 'text',
        readonlyWhen: () => true
      },
      {
        key: 'docente', label: 'Docente', type: 'text',
        readonlyWhen: () => true
      },
      {
        key: 'estado', label: 'Estado', type: 'text',
        readonlyWhen: () => true
      },
      {
        key: 'revisorPublicId', label: 'Revisor', type: 'select',
        required: true, options: this.planeacionesService.reviewerOptions,
        span: 'full'
      }
    ],
    counters: [{ label: 'Planeaciones disponibles', valueKey: 'total' }],
    searchKeys: ['asignatura', 'docente', 'estado', 'revisorNombre']
  };
}
