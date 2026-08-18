import { Component, inject } from '@angular/core';
import { ImportacionesLocalService } from '../../../../core/services/importaciones-local.service';
import { AdminImportPage } from '../../shared/admin-import-page/admin-import-page';
import { AdminImportConfig } from '../../shared/admin-import-page/admin-import.types';

@Component({
  selector: 'app-importar-profesores',
  standalone: true,
  imports: [AdminImportPage],
  templateUrl: './importar-profesores.html',
  styleUrl: './importar-profesores.css'
})
export class ImportarProfesores {
  private readonly importacionesLocalService = inject(ImportacionesLocalService);

  config: AdminImportConfig = {
    title: 'Importar Profesores',
    subtitle: 'Carga usuarios docentes/revisores desde CSV o Excel con roles del sistema y rol dentro de academia.',
    sectionLabel: 'Carga académica',
    importLabel: 'Importar profesores',
    templateLabel: 'Descargar plantilla',
    expectedColumns: ['nombre', 'apellido_paterno', 'apellido_materno', 'email', 'telefono', 'roles', 'academia', 'rol_academia'],
    previewColumns: [
      { key: 'nombre', label: 'Nombre', required: true },
      { key: 'apellidoPaterno', label: 'Apellido paterno', required: true },
      { key: 'apellidoMaterno', label: 'Apellido materno' },
      { key: 'email', label: 'Correo', required: true },
      { key: 'telefono', label: 'Teléfono' },
      { key: 'roles', label: 'Roles', required: true },
      { key: 'academia', label: 'Academia' },
      { key: 'rolAcademia', label: 'Rol academia' }
    ],
    dataSource: this.importacionesLocalService.profesoresDataSource,
    successMessage: 'Profesores importados correctamente.'
  };
}
