import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideDownload, LucideDynamicIcon, LucideEye, LucideFileText, LucideSearch } from '@lucide/angular';
import { ProgramaAsignaturaResumen, ProgramasAsignaturaService } from '../../../../core/services/programas-asignatura.service';

@Component({
  selector: 'app-programas-asignatura', standalone: true,
  imports: [DatePipe, FormsModule, LucideDynamicIcon],
  templateUrl: './programas-asignatura.html'
})
export class ProgramasAsignatura {
  private readonly service = inject(ProgramasAsignaturaService);
  items = signal<ProgramaAsignaturaResumen[]>([]);
  search = signal('');
  error = signal('');
  searchIcon = LucideSearch; fileIcon = LucideFileText; eyeIcon = LucideEye; downloadIcon = LucideDownload;
  filtered = computed(() => {
    const query = this.search().trim().toLowerCase();
    return query ? this.items().filter(item => [item.asignatura, item.claveAsignatura, item.carrera, item.nombreArchivo, item.subidoPor].some(value => value?.toLowerCase().includes(query))) : this.items();
  });

  ngOnInit(): void { this.service.load().subscribe({ next: items => this.items.set(items), error: () => this.error.set('No fue posible cargar los programas de asignatura.') }); }
  view(item: ProgramaAsignaturaResumen): void { this.service.view(item.publicId).subscribe({ next: blob => window.open(URL.createObjectURL(blob), '_blank', 'noopener'), error: () => this.error.set('No fue posible abrir el archivo.') }); }
  download(item: ProgramaAsignaturaResumen): void { this.service.download(item.publicId).subscribe({ next: blob => { const link = document.createElement('a'); link.href = URL.createObjectURL(blob); link.download = item.nombreArchivo; link.click(); URL.revokeObjectURL(link.href); }, error: () => this.error.set('No fue posible descargar el archivo.') }); }
}
