import { Component } from '@angular/core';
import { AdminImportPage } from '../../shared/admin-import-page/admin-import-page';
import { AdminImportConfig } from '../../shared/admin-import-page/admin-import.types';

@Component({
  selector: 'app-importacion-academias',
  standalone: true,
  imports: [AdminImportPage],
  templateUrl: './importacion-academias.html',
  styleUrl: './importacion-academias.css'
})
export class ImportacionAcademias {
  config: AdminImportConfig = {
    title: 'Importación de Academias',
    subtitle: 'Carga academias desde CSV o Excel con las columnas necesarias para el catálogo real.',
    sectionLabel: 'Administración',
    importLabel: 'Importar academias',
    templateLabel: 'Descargar plantilla',
    expectedColumns: ['nombre', 'descripcion'],
    previewColumns: [
      { key: 'nombre', label: 'Nombre', required: true },
      { key: 'descripcion', label: 'Descripción', required: true }
    ],
    initialPreview: [
      { id: 1, nombre: 'Academia de Desarrollo de Software', descripcion: 'Área enfocada en programación.', estado: 'validado', observacion: 'Registro válido' },
      { id: 2, nombre: 'Academia de Bases de Datos', descripcion: 'Área de modelado y administración de datos.', estado: 'validado', observacion: 'Registro válido' },
      { id: 3, nombre: '', descripcion: 'Registro sin nombre.', estado: 'error', observacion: 'Falta nombre de academia' }
    ]
  };
}
