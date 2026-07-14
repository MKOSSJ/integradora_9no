import { DatePipe, NgClass } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  LucideDynamicIcon,
  LucideInfo,
  LucideChevronDown,
  LucideCircleCheckBig,
  LucideTriangleAlert,
  LucideMessageSquare,
  LucideGraduationCap,
  LucidePlayCircle
} from '@lucide/angular';

import {
  RevisionDetail,
  RevisionStatus
} from '../../../../core/models/validacion.model';

@Component({
  selector: 'app-revision-side-panel',
  standalone: true,
  imports: [
    NgClass,
    DatePipe,
    FormsModule,
    LucideDynamicIcon
  ],
  templateUrl: './revision-side-panel.html',
  styleUrl: './revision-side-panel.css'
})
export class RevisionSidePanel {
  @Input({ required: true }) revision!: RevisionDetail;

  @Output() startRevision = new EventEmitter<void>();
  @Output() approve = new EventEmitter<void>();
  @Output() requestCorrections = new EventEmitter<void>();
  @Output() addComment = new EventEmitter<string>();

  commentText = '';

  infoIcon = LucideInfo;
  chevronIcon = LucideChevronDown;
  approveIcon = LucideCircleCheckBig;
  correctionsIcon = LucideTriangleAlert;
  commentsIcon = LucideMessageSquare;
  guideIcon = LucideGraduationCap;
  startIcon = LucidePlayCircle;

  openSections = signal({
    info: true,
    guide: true,
    comments: true
  });

  canReviewerEdit(): boolean {
    return this.revision.reviewStatus === 'pendiente' ||
      this.revision.reviewStatus === 'revision';
  }

  canStartRevision(): boolean {
    return this.revision.reviewStatus === 'pendiente';
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

  sendComment(): void {
    if (!this.canReviewerEdit()) return;
    if (!this.commentText.trim()) return;

    this.addComment.emit(this.commentText.trim());
    this.commentText = '';
  }

  getStatusLabel(status: RevisionStatus): string {
    if (status === 'aprobado') return 'Aprobado';
    if (status === 'borrador') return 'Borrador';
    if (status === 'revision') return 'En revisión';
    if (status === 'pendiente') return 'Pendiente';
    return 'Correcciones';
  }

  getStatusClasses(status: RevisionStatus): string {
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
}