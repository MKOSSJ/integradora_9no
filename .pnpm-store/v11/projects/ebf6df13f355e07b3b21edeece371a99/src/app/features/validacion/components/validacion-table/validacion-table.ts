import { DatePipe, NgClass } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

import {
  LucideDynamicIcon,
  LucideEye,
  LucideChevronRight
} from '@lucide/angular';

import {
  RevisionItem,
  RevisionStatus
} from '../../../../core/models/validacion.model';

@Component({
  selector: 'app-validacion-table',
  standalone: true,
  imports: [
    NgClass,
    DatePipe,
    RouterLink,
    LucideDynamicIcon
  ],
  templateUrl: './validacion-table.html',
  styleUrl: './validacion-table.css'
})
export class ValidacionTable {
  @Input({ required: true }) items: RevisionItem[] = [];

  eyeIcon = LucideEye;
  arrowIcon = LucideChevronRight;

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