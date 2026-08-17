import { Component } from '@angular/core';
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
    initialPreview: [
      { id: 1, nombre: 'Carlos', apellidoPaterno: 'Pérez', apellidoMaterno: 'López', email: 'carlos.perez@uth.edu.mx', telefono: '7711234567', roles: 'DOCENTE', academia: 'Academia de Desarrollo de Software', rolAcademia: 'Docente', estado: 'validado', observacion: 'Registro válido' },
      { id: 2, nombre: 'María', apellidoPaterno: 'González', apellidoMaterno: 'Ruiz', email: 'maria.gonzalez@uth.edu.mx', telefono: '7714567890', roles: 'DOCENTE,REVISOR', academia: 'Academia de Bases de Datos', rolAcademia: 'Revisor', estado: 'validado', observacion: 'Registro válido' },
      { id: 3, nombre: 'Juan', apellidoPaterno: 'Martínez', apellidoMaterno: '', email: '', telefono: '7719999999', roles: 'DOCENTE', academia: 'Academia de Redes', rolAcademia: 'Docente', estado: 'error', observacion: 'Falta correo electrónico' }
    ]
  };
}
