import { DatePipe, NgClass } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import {
  LucideDynamicIcon,
  LucideArrowLeft,
  LucideFileText,
  LucideBookOpen
} from '@lucide/angular';

import {
  ReviewTab,
  RevisionDetail,
  RevisionStatus
} from '../../../../core/models/validacion.model';

import { ValidacionService } from '../../../../core/services/validacion.service';
import { RevisionSidePanel } from '../../components/revision-side-panel/revision-side-panel';

import { PlaneacionPdfViewer } from '../../../planeaciones/components/planeacion-pdf-viewer/planeacion-pdf-viewer';
import { PlaneacionProgramaView } from '../../../planeaciones/components/planeacion-programa-view/planeacion-programa-view';

@Component({
  selector: 'app-validacion-detail',
  standalone: true,
  imports: [
    NgClass,
    DatePipe,
    RouterLink,
    LucideDynamicIcon,
    PlaneacionPdfViewer,
    PlaneacionProgramaView,
    RevisionSidePanel
  ],
  templateUrl: './validacion-detail.html',
  styleUrl: './validacion-detail.css'
})
export class ValidacionDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly validacionService = inject(ValidacionService);

  revision = signal<RevisionDetail | null>(null);
  activeTab = signal<ReviewTab>('vista-previa');

  statusNotice = signal<{
    message: string;
    type: 'info' | 'success' | 'warning';
  } | null>(null);

  backIcon = LucideArrowLeft;
  previewIcon = LucideFileText;
  programIcon = LucideBookOpen;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.validacionService.getRevisionById(id).subscribe(revision => {
      if (!revision) return;

      if (revision.reviewStatus === 'pendiente') {
        this.validacionService.startRevision(revision.id).subscribe(() => {
          this.validacionService.getRevisionById(revision.id).subscribe(updated => {
            if (!updated) return;

            this.revision.set(updated);

            this.showStatusNotice(
              'La planeación cambió automáticamente a estado: En revisión.',
              'info'
            );
          });
        });

        return;
      }

      this.revision.set(revision);
    });
  }

  setTab(tab: ReviewTab): void {
    this.activeTab.set(tab);
  }

  approveRevision(): void {
    const currentRevision = this.revision();

    if (!currentRevision) return;

    this.validacionService.approveRevision(currentRevision.id).subscribe(() => {
      this.validacionService.getRevisionById(currentRevision.id).subscribe(updated => {
        if (!updated) return;

        this.revision.set(updated);

        this.showStatusNotice(
          'La planeación fue aprobada correctamente.',
          'success'
        );
      });
    });
  }

  requestCorrections(): void {
    const currentRevision = this.revision();

    if (!currentRevision) return;

    this.validacionService.requestCorrections(currentRevision.id).subscribe(() => {
      this.validacionService.getRevisionById(currentRevision.id).subscribe(updated => {
        if (!updated) return;

        this.revision.set(updated);

        this.showStatusNotice(
          'Se solicitaron correcciones al docente.',
          'warning'
        );
      });
    });
  }

  addComment(comment: string): void {
    const currentRevision = this.revision();

    if (!currentRevision || !this.canReviewerEdit(currentRevision.reviewStatus)) return;
    if (!comment.trim()) return;

    this.validacionService.addComment(currentRevision.id, comment).subscribe(() => {
      this.validacionService.getRevisionById(currentRevision.id).subscribe(updated => {
        if (!updated) return;

        this.revision.set(updated);

        this.showStatusNotice(
          'Comentario agregado correctamente.',
          'success'
        );
      });
    });
  }

  canReviewerEdit(status: RevisionStatus): boolean {
    return status === 'pendiente' || status === 'revision';
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

  getStatusNoticeClasses(type: 'info' | 'success' | 'warning'): string {
    if (type === 'success') {
      return 'border-green-200 bg-green-50 text-green-700';
    }

    if (type === 'warning') {
      return 'border-orange-200 bg-orange-50 text-orange-700';
    }

    return 'border-cyan-200 bg-cyan-50 text-cyan-700';
  }

  private showStatusNotice(
    message: string,
    type: 'info' | 'success' | 'warning'
  ): void {
    this.statusNotice.set({
      message,
      type
    });

    setTimeout(() => {
      this.statusNotice.set(null);
    }, 3500);
  }
}