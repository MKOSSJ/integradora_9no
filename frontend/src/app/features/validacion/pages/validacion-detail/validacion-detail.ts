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

import { PlaneacionPdfViewer } from '../../../components/planeacion-pdf-viewer/planeacion-pdf-viewer';
import { PlaneacionProgramaView } from '../../../components/planeacion-programa-view/planeacion-programa-view';

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
  private route = inject(ActivatedRoute);
  private validacionService = inject(ValidacionService);

  revision = signal<RevisionDetail | null>(null);
  activeTab = signal<ReviewTab>('vista-previa');

  backIcon = LucideArrowLeft;
  previewIcon = LucideFileText;
  programIcon = LucideBookOpen;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.validacionService.getRevisionById(id).subscribe(data => {
      if (data) {
        this.revision.set(data);
      }
    });
  }

  setTab(tab: ReviewTab): void {
    this.activeTab.set(tab);
  }

  startRevision(): void {
    const current = this.revision();

    if (!current || current.reviewStatus !== 'pendiente') return;

    this.validacionService.startRevision(current.id).subscribe(() => {
      this.revision.set({
        ...current,
        reviewStatus: 'revision',
        status: 'revision'
      });
    });
  }

  approveRevision(): void {
    const current = this.revision();

    if (!current || !this.canReviewerEdit(current.reviewStatus)) return;

    this.validacionService.approveRevision(current.id).subscribe(() => {
      this.revision.set({
        ...current,
        reviewStatus: 'aprobado',
        status: 'aprobado'
      });
    });
  }

  requestCorrections(): void {
    const current = this.revision();

    if (!current || !this.canReviewerEdit(current.reviewStatus)) return;

    this.validacionService.requestCorrections(current.id).subscribe(() => {
      this.revision.set({
        ...current,
        reviewStatus: 'correcciones',
        status: 'correcciones'
      });
    });
  }

  addComment(comment: string): void {
    const current = this.revision();

    if (!current || !this.canReviewerEdit(current.reviewStatus)) return;
    if (!comment.trim()) return;

    this.validacionService.addComment(current.id, comment).subscribe(() => {
      this.revision.set({
        ...current,
        comentariosRevision: [...current.comentariosRevision, comment.trim()]
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
}