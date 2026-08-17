import { NgClass } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import {
  LucideDynamicIcon,
  LucideArrowLeft,
  LucideFileText,
  LucideBookOpen,
  LucidePenLine
} from '@lucide/angular';

import { PlaneacionesService } from '../../../../core/services/planeaciones.service';

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

  planeacion = signal<PlaneacionDetail | null>(null);
  activeTab = signal<PlaneacionTab>('vista-previa');
  formTutorial = signal<PlaneacionTutorial | null>(null);

  backIcon = LucideArrowLeft;
  previewIcon = LucideFileText;
  programaIcon = LucideBookOpen;
  formIcon = LucidePenLine;

  isEditable = computed(() => {
    const status = this.planeacion()?.status;

    return status === 'borrador' || status === 'correcciones';
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.planeacionesService.getPlaneacionById(id).subscribe(data => {
      if (data) {
        this.planeacion.set(data);
      }
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

    this.planeacionesService.saveDraft(current.id).subscribe();
  }

  submitForApproval(): void {
    const current = this.planeacion();

    if (!current || !this.isEditable()) return;

    this.planeacionesService.submitForApproval(current.id).subscribe(() => {
      this.planeacion.set({
        ...current,
        status: 'pendiente'
      });

      this.activeTab.set('vista-previa');
    });
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
}