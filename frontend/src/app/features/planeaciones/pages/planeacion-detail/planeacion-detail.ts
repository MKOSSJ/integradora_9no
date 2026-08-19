import { NgClass } from '@angular/common';
import {
  Component,
  computed,
  inject,
  OnInit,
  signal,
  ViewChild
} from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize, map, switchMap } from 'rxjs';

import {
  LucideDynamicIcon,
  LucideArrowLeft,
  LucideFileText,
  LucideBookOpen,
  LucidePenLine
} from '@lucide/angular';

import { PlaneacionesService } from '../../../../core/services/planeaciones.service';
import { AuthService } from '../../../../core/services/auth.service';

import {
  PlaneacionDetail,
  PlaneacionStatus,
  PlaneacionTab,
  PlaneacionTutorial
} from '../../../../core/models/planeacion.model';

import { PlaneacionInfoPanel } from '../../components/planeacion-info-panel/planeacion-info-panel';
import { PlaneacionPdfViewer } from '../../components/planeacion-pdf-viewer/planeacion-pdf-viewer';
import { PlaneacionProgramaView } from '../../components/planeacion-programa-view/planeacion-programa-view';
import { PlaneacionForm } from '../../components/planeacion-form/planeacion-form';

@Component({
  selector: 'app-planeacion-detail',
  standalone: true,
  imports: [
    NgClass,
    RouterLink,
    LucideDynamicIcon,
    PlaneacionInfoPanel,
    PlaneacionPdfViewer,
    PlaneacionProgramaView,
    PlaneacionForm
  ],
  templateUrl: './planeacion-detail.html',
  styleUrl: './planeacion-detail.css'
})
export class PlaneacionDetailPage implements OnInit {
  private route = inject(ActivatedRoute);
  private planeacionesService = inject(PlaneacionesService);
  private authService = inject(AuthService);

  planeacion = signal<PlaneacionDetail<string> | null>(null);
  activeTab = signal<PlaneacionTab>('vista-previa');
  formTutorial = signal<PlaneacionTutorial | null>(null);
  loadError = signal('');
  saving = signal(false);
  submitting = signal(false);
  pdfRefreshKey = signal(0);

  @ViewChild(PlaneacionInfoPanel) private infoPanel?: PlaneacionInfoPanel;

  backIcon = LucideArrowLeft;
  previewIcon = LucideFileText;
  programaIcon = LucideBookOpen;
  formIcon = LucidePenLine;

  isEditable = computed(() => {
    if (this.isDirector()) return false;
    const status = this.planeacion()?.status;

    return status === 'borrador' ||
      status === 'en-proceso' ||
      status === 'correcciones' ||
      status === 'reabierta';
  });
  isDirector = computed(() => this.authService.currentUser()?.roles.includes('DIRECTIVO') ?? false);

  ngOnInit(): void {
    const publicId = this.route.snapshot.paramMap.get('id')?.trim() ?? '';

    if (!publicId) {
      this.loadError.set('La planeación solicitada no tiene un identificador válido.');
      return;
    }

    const request = this.isDirector()
      ? this.planeacionesService.getPlaneacionAdministrativaById(publicId)
      : this.planeacionesService.getPlaneacionById(publicId);

    request.subscribe({
      next: data => this.planeacion.set(data),
      error: error => this.loadError.set(this.errorMessage(error))
    });
  }

  setTab(tab: PlaneacionTab): void {
    if (tab === 'formulario' && !this.isEditable()) {
      return;
    }

    this.activeTab.set(tab);
  }

  updateFormTutorial(tutorial: PlaneacionTutorial): void {
    this.formTutorial.set(tutorial);
  }

  saveDraft(): void {
    const current = this.planeacion();

    if (!current || !this.isEditable()) return;

    if (this.saving()) return;

    this.saving.set(true);
    this.planeacionesService.saveDraft(current).pipe(
      finalize(() => this.saving.set(false))
    ).subscribe({
      next: updated => {
        this.planeacion.set(updated);
        this.pdfRefreshKey.update(value => value + 1);
        this.infoPanel?.showFeedback(
          'Planeación guardada correctamente.',
          'success'
        );
      },
      error: error => this.infoPanel?.showFeedback(
        this.errorMessage(error),
        'warning'
      )
    });
  }

  submitForApproval(): void {
    const current = this.planeacion();

    if (!current || !this.isEditable()) return;

    if (this.submitting()) return;

    this.submitting.set(true);
    this.planeacionesService.saveDraft(current).pipe(
      switchMap(updated => {
        this.planeacion.set(updated);
        this.pdfRefreshKey.update(value => value + 1);
        return this.planeacionesService.submitForApproval(String(updated.id)).pipe(
          map(status => ({ updated, status }))
        );
      }),
      finalize(() => this.submitting.set(false))
    ).subscribe({
      next: ({ updated, status }) => {
        this.planeacion.set({ ...updated, status });
        this.pdfRefreshKey.update(value => value + 1);
        this.activeTab.set('vista-previa');
        this.infoPanel?.showFeedback(
          'Planeación enviada a revisión correctamente.',
          'success'
        );
      },
      error: error => this.infoPanel?.showFeedback(
        this.errorMessage(error),
        'warning'
      )
    });
  }

  getStatusLabel(status: PlaneacionStatus): string {
    if (status === 'aprobado') return 'Aprobado';
    if (status === 'borrador') return 'Borrador';
    if (status === 'en-proceso') return 'En proceso';
    if (status === 'revision') return 'En revisión';
    if (status === 'pendiente') return 'Pendiente';
    if (status === 'correcciones') return 'Correcciones';
    if (status === 'rechazada') return 'Rechazada';
    if (status === 'finalizada') return 'Finalizada';
    return 'Reabierta';
  }

  getStatusClasses(status: PlaneacionStatus): string {
    if (status === 'aprobado') {
      return 'bg-green-100 text-green-700 ring-green-200';
    }

    if (status === 'borrador') {
      return 'bg-slate-100 text-slate-700 ring-slate-200';
    }

    if (status === 'revision') {
      return 'bg-cyan-100 text-cyan-700 ring-cyan-200';
    }

    if (status === 'finalizada') {
      return 'bg-green-100 text-green-700 ring-green-200';
    }

    if (status === 'pendiente') {
      return 'bg-amber-100 text-amber-700 ring-amber-200';
    }

    return 'bg-orange-100 text-orange-700 ring-orange-200';
  }

  private errorMessage(error: unknown): string {
    if (error && typeof error === 'object') {
      const payload = (error as { error?: unknown }).error;
      if (payload && typeof payload === 'object') {
        const response = payload as Record<string, unknown>;
        const errors = response['errors'];
        if (Array.isArray(errors)) {
          const message = errors.filter(item => typeof item === 'string').join(' ');
          if (message) return message;
        }
        const message = response['message'];
        if (typeof message === 'string' && message.trim()) return message.trim();
      }
    }

    if (error instanceof Error && error.message.trim()) {
      return error.message.trim();
    }

    return 'No fue posible completar la operación.';
  }
}
