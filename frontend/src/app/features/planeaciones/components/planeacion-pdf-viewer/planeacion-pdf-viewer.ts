import { NgClass } from '@angular/common';
import { Component, Input, signal } from '@angular/core';

import {
  LucideDynamicIcon,
  LucideChevronLeft,
  LucideChevronRight,
  LucideZoomIn,
  LucideZoomOut,
  LucidePrinter,
  LucideDownload
} from '@lucide/angular';

import { PlaneacionDetail } from '../../../../core/models/planeacion.model';

@Component({
  selector: 'app-planeacion-pdf-viewer',
  standalone: true,
  imports: [
    NgClass,
    LucideDynamicIcon
  ],
  templateUrl: './planeacion-pdf-viewer.html',
  styleUrl: './planeacion-pdf-viewer.css'
})
export class PlaneacionPdfViewer {
  @Input({ required: true }) planeacion!: PlaneacionDetail;
  @Input() mode: 'preview' | 'programa' = 'preview';

  currentPage = signal(1);
  zoom = signal(100);

  prevIcon = LucideChevronLeft;
  nextIcon = LucideChevronRight;
  zoomInIcon = LucideZoomIn;
  zoomOutIcon = LucideZoomOut;
  printIcon = LucidePrinter;
  downloadIcon = LucideDownload;

  pages(): number[] {
    const total = this.planeacion?.pdfPages || 1;
    return Array.from({ length: total }, (_, index) => index + 1);
  }

  previousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.update(value => value - 1);
    }
  }

  nextPage(): void {
    if (this.currentPage() < this.pages().length) {
      this.currentPage.update(value => value + 1);
    }
  }

  zoomIn(): void {
    this.zoom.update(value => Math.min(value + 10, 140));
  }

  zoomOut(): void {
    this.zoom.update(value => Math.max(value - 10, 70));
  }
}