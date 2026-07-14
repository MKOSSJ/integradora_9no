import { DatePipe, NgClass } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import {
  LucideDynamicIcon,
  LucideFileText,
  LucideSave,
  LucideSend,
  LucideChevronDown,
  LucideMessageSquare,
  LucideCircleHelp
} from '@lucide/angular';

import {
  PlaneacionDetail,
  PlaneacionStatus,
  PlaneacionTutorial
} from '../../../core/models/planeacion.model';

@Component({
  selector: 'app-planeacion-info-panel',
  standalone: true,
  imports: [
    NgClass,
    DatePipe,
    LucideDynamicIcon
  ],
  templateUrl: './planeacion-info-panel.html',
  styleUrl: './planeacion-info-panel.css'
})
export class PlaneacionInfoPanel {
  @Input({ required: true }) planeacion!: PlaneacionDetail;
  @Input() formTutorial: PlaneacionTutorial | null = null;
  @Input() canEdit = true;

  @Output() saveDraft = new EventEmitter<void>();
  @Output() submitForApproval = new EventEmitter<void>();

  infoIcon = LucideFileText;
  saveIcon = LucideSave;
  sendIcon = LucideSend;
  chevronIcon = LucideChevronDown;
  commentsIcon = LucideMessageSquare;
  tutorialIcon = LucideCircleHelp;

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
}