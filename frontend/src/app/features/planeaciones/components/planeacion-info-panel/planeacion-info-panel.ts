import { DatePipe, NgClass } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

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

interface RevisionComment {
  id: number;
  autor: string;
  rol: 'Docente' | 'Revisor';
  mensaje: string;
  hora: string;
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
export class PlaneacionInfoPanel {
  @Input({ required: true }) planeacion!: PlaneacionDetail;
  @Input() canEdit = true;
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

  feedbackMessage = signal('');
  feedbackType = signal<'success' | 'warning'>('success');

  comments = signal<RevisionComment[]>([
    {
      id: 1,
      autor: 'María González',
      rol: 'Revisor',
      mensaje: 'Revisar que las evidencias de cierre correspondan al resultado de aprendizaje.',
      hora: '10:42'
    },
    {
      id: 2,
      autor: 'María González',
      rol: 'Revisor',
      mensaje: 'La estructura general de la secuencia es clara.',
      hora: '10:50'
    },
    {
      id: 3,
      autor: 'Carlos Pérez',
      rol: 'Docente',
      mensaje: 'Actualizaré la evidencia de cierre y guardaré los cambios.',
      hora: '11:05'
    }
  ]);

  openSections = signal({
    info: true,
    guide: true,
    comments: true
  });

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

    if (!text) return;

    this.comments.update(current => [
      {
        id: Date.now(),
        autor: this.planeacion.autor || 'Docente',
        rol: 'Docente',
        mensaje: text,
        hora: new Date().toLocaleTimeString('es-MX', {
          hour: '2-digit',
          minute: '2-digit'
        })
      },
      ...current
    ]);

    this.commentText = '';
  }

  handleSaveDraft(): void {
    if (!this.canEdit) {
      this.showFeedback('Esta planeación está bloqueada y no se puede guardar.', 'warning');
      return;
    }

    this.saveDraft.emit();
    this.showFeedback('Cambios guardados correctamente.', 'success');
  }

  handleSubmitForApproval(): void {
    if (!this.canEdit) {
      this.showFeedback('Esta planeación está bloqueada y no se puede enviar.', 'warning');
      return;
    }

    this.submitForApproval.emit();
    this.showFeedback('Planeación enviada a revisión correctamente.', 'success');
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
    if (status === 'revision') return 'En revisión';
    if (status === 'pendiente') return 'Pendiente';
    return 'Correcciones';
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
}