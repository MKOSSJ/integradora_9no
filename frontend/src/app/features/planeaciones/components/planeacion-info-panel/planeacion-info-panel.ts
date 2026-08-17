import { DatePipe, NgClass } from '@angular/common';
import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  Output,
  signal,
  SimpleChanges
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import {
  LucideDynamicIcon,
  LucideInfo,
  LucideChevronDown,
  LucideSave,
  LucideSend,
  LucideGraduationCap,
  LucideFileText,
  LucideMessageSquare
} from '@lucide/angular';

import {
  PlaneacionDetail,
  PlaneacionStatus,
  PlaneacionTutorial
} from '../../../../core/models/planeacion.model';
import { PlaneacionesService } from '../../../../core/services/planeaciones.service';

interface RevisionComment {
  id: string;
  autor: string;
  rol: string;
  mensaje: string;
  fecha: string;
}

@Component({
  selector: 'app-planeacion-info-panel',
  standalone: true,
  imports: [
    NgClass,
    DatePipe,
    FormsModule,
    LucideDynamicIcon
  ],
  templateUrl: './planeacion-info-panel.html',
  styleUrl: './planeacion-info-panel.css'
})
export class PlaneacionInfoPanel implements OnChanges {
  private readonly planeacionesService = inject(PlaneacionesService);

  @Input({ required: true }) planeacion!: PlaneacionDetail<string | number>;
  @Input() canEdit = true;
  @Input() saving = false;
  @Input() submitting = false;
  @Input() formTutorial: PlaneacionTutorial | null = null;

  @Output() saveDraft = new EventEmitter<void>();
  @Output() submitForApproval = new EventEmitter<void>();

  infoIcon = LucideInfo;
  chevronIcon = LucideChevronDown;
  saveIcon = LucideSave;
  sendIcon = LucideSend;
  guideIcon = LucideGraduationCap;
  documentIcon = LucideFileText;
  commentIcon = LucideMessageSquare;

  commentText = '';
  commentsLoading = signal(false);
  commentSaving = signal(false);

  feedbackMessage = signal('');
  feedbackType = signal<'success' | 'warning'>('success');

  comments = signal<RevisionComment[]>([]);

  openSections = signal({
    info: true,
    guide: true,
    comments: true
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['planeacion'] && this.planeacion?.id) {
      this.loadComments();
    }
  }

  toggleSection(section: 'info' | 'guide' | 'comments'): void {
    this.openSections.update(current => ({
      ...current,
      [section]: !current[section]
    }));
  }

  isOpen(section: 'info' | 'guide' | 'comments'): boolean {
    return this.openSections()[section];
  }

  addComment(): void {
    const text = this.commentText.trim();

    if (!text || this.commentSaving()) return;

    this.commentSaving.set(true);
    this.planeacionesService.addComment(String(this.planeacion.id), text).pipe(
      finalize(() => this.commentSaving.set(false))
    ).subscribe({
      next: comment => {
        this.comments.update(current => [...current, {
          id: comment.publicId,
          autor: comment.usuario,
          rol: comment.rolEnChat,
          mensaje: comment.mensaje,
          fecha: comment.fecha
        }]);
        this.commentText = '';
      },
      error: error => this.showFeedback(this.errorMessage(error), 'warning')
    });
  }

  handleSaveDraft(): void {
    if (!this.canEdit) {
      this.showFeedback('Esta planeación está bloqueada y no se puede guardar.', 'warning');
      return;
    }

    if (this.saving || this.submitting) return;
    this.saveDraft.emit();
  }

  handleSubmitForApproval(): void {
    if (!this.canEdit) {
      this.showFeedback('Esta planeación está bloqueada y no se puede enviar.', 'warning');
      return;
    }

    if (this.saving || this.submitting) return;
    this.submitForApproval.emit();
  }

  showFeedback(message: string, type: 'success' | 'warning'): void {
    this.feedbackMessage.set(message);
    this.feedbackType.set(type);

    setTimeout(() => {
      this.feedbackMessage.set('');
    }, 3500);
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

  getGuideTitle(): string {
    return this.formTutorial?.title ?? 'Guía de llenado';
  }

  getGuideText(): string {
    return this.formTutorial?.text ??
      'Completa cada sección de la planeación didáctica. Revisa que la información sea clara, coherente y corresponda con el programa de asignatura.';
  }

  getGuideOptions(): string[] {
    return this.formTutorial?.options ?? [
      'Verifica que la carátula tenga periodo, asignatura, docentes y grupos.',
      'Comprueba que cada unidad incluya temas, saberes y resultado de aprendizaje.',
      'Agrega evidencias, instrumentos de evaluación y ponderaciones.',
      'Guarda tus cambios antes de enviar la planeación a revisión.'
    ];
  }

  private loadComments(): void {
    this.commentsLoading.set(true);
    this.planeacionesService.getComments(String(this.planeacion.id)).pipe(
      finalize(() => this.commentsLoading.set(false))
    ).subscribe({
      next: response => this.comments.set(response.comentarios.map(comment => ({
        id: comment.publicId,
        autor: comment.usuario,
        rol: comment.rolEnChat,
        mensaje: comment.mensaje,
        fecha: comment.fecha
      }))),
      error: error => this.showFeedback(this.errorMessage(error), 'warning')
    });
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

    if (error instanceof Error && error.message.trim()) return error.message.trim();
    return 'No fue posible completar la operación.';
  }
}
