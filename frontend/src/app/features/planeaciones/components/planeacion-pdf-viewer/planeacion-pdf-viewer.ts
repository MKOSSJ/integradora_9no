import { NgClass } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  Input,
  OnDestroy,
  ViewChild,
  signal,
} from '@angular/core';

import {
  LucideDynamicIcon,
  LucideChevronLeft,
  LucideChevronRight,
  LucideZoomIn,
  LucideZoomOut,
  LucidePrinter,
  LucideDownload,
} from '@lucide/angular';

import * as pdfjsLib from 'pdfjs-dist';

import { PlaneacionDetail } from '../../../../core/models/planeacion.model';

(pdfjsLib as any).GlobalWorkerOptions.workerSrc = '/assets/pdf/pdf.worker.min.mjs';

@Component({
  selector: 'app-planeacion-pdf-viewer',
  standalone: true,
  imports: [NgClass, LucideDynamicIcon],
  templateUrl: './planeacion-pdf-viewer.html',
  styleUrl: './planeacion-pdf-viewer.css',
})
export class PlaneacionPdfViewer implements AfterViewInit, OnDestroy {
  @Input({ required: true })
  planeacion!: PlaneacionDetail;

  @Input()
  mode: 'preview' | 'programa' = 'preview';

  @ViewChild('pdfContainer')
  pdfContainer!: ElementRef<HTMLDivElement>;

  currentPage = signal(1);
  totalPages = signal(0);
  zoom = signal(100);
  loading = signal(true);
  error = signal('');

  prevIcon = LucideChevronLeft;
  nextIcon = LucideChevronRight;
  zoomInIcon = LucideZoomIn;
  zoomOutIcon = LucideZoomOut;
  printIcon = LucidePrinter;
  downloadIcon = LucideDownload;

  private pdfDocument: any = null;
  private renderTask: any = null;
  private destroyed = false;

  private readonly pdfUrl = '/assets/pdf/Planeacion.pdf';

  async ngAfterViewInit(): Promise<void> {
    this.destroyed = false;

    await new Promise<void>((resolve) => {
      requestAnimationFrame(() => resolve());
    });

    if (this.destroyed) {
      return;
    }

    await this.loadPdf();
  }

  ngOnDestroy(): void {
    this.destroyed = true;

    if (this.renderTask) {
      try {
        this.renderTask.cancel();
      } catch {
        // No hacer nada
      }

      this.renderTask = null;
    }

    this.pdfDocument = null;
  }

  pages(): number[] {
    return Array.from({ length: this.totalPages() }, (_, index) => index + 1);
  }

  async goToPage(page: number): Promise<void> {
    if (this.destroyed || !this.pdfDocument) {
      return;
    }

    if (page < 1 || page > this.totalPages()) {
      return;
    }

    this.currentPage.set(page);

    await this.renderPage(page);
  }

  async previousPage(): Promise<void> {
    if (this.currentPage() <= 1) {
      return;
    }

    await this.goToPage(this.currentPage() - 1);
  }

  async nextPage(): Promise<void> {
    if (this.currentPage() >= this.totalPages()) {
      return;
    }

    await this.goToPage(this.currentPage() + 1);
  }

  async zoomIn(): Promise<void> {
    if (this.destroyed) {
      return;
    }

    const nextZoom = Math.min(this.zoom() + 10, 150);

    this.zoom.set(nextZoom);

    await this.renderPage(this.currentPage());
  }

  async zoomOut(): Promise<void> {
    if (this.destroyed) {
      return;
    }

    const nextZoom = Math.max(this.zoom() - 10, 50);

    this.zoom.set(nextZoom);

    await this.renderPage(this.currentPage());
  }

  print(): void {
    window.open(this.pdfUrl, '_blank');
  }

  download(): void {
    const link = document.createElement('a');

    link.href = this.pdfUrl;
    link.download = 'planeacion.pdf';
    link.target = '_blank';

    document.body.appendChild(link);

    link.click();

    link.remove();
  }

  private async loadPdf(): Promise<void> {
    if (this.destroyed) {
      return;
    }

    try {
      this.loading.set(true);
      this.error.set('');

      this.pdfDocument = await pdfjsLib.getDocument({
        url: this.pdfUrl,
      }).promise;

      if (this.destroyed) {
        return;
      }

      this.totalPages.set(this.pdfDocument.numPages);

      this.currentPage.set(1);

      await this.renderPage(1);

      if (!this.destroyed) {
        this.loading.set(false);
      }
    } catch (error) {
      if (this.destroyed) {
        return;
      }

      console.error('Error al cargar PDF:', error);

      this.loading.set(false);

      this.error.set('No fue posible cargar el PDF.');
    }
  }

  private async renderPage(pageNumber: number): Promise<void> {
    if (this.destroyed) {
      return;
    }

    if (!this.pdfDocument) {
      return;
    }

    if (!this.pdfContainer) {
      return;
    }

    if (!this.pdfContainer.nativeElement) {
      return;
    }

    try {
      if (this.renderTask) {
        try {
          this.renderTask.cancel();
        } catch {
          // No hacer nada
        }

        this.renderTask = null;
      }

      const page = await this.pdfDocument.getPage(pageNumber);

      if (this.destroyed) {
        return;
      }

      const scale = this.zoom() / 100;

      const viewport = page.getViewport({
        scale,
      });

      if (this.destroyed) {
        return;
      }

      const container = this.pdfContainer.nativeElement;

      container.innerHTML = '';

      const canvas = document.createElement('canvas');

      const context = canvas.getContext('2d');

      if (!context) {
        throw new Error('No fue posible obtener el contexto del canvas.');
      }

      const devicePixelRatio = window.devicePixelRatio || 1;

      canvas.width = Math.floor(viewport.width * devicePixelRatio);

      canvas.height = Math.floor(viewport.height * devicePixelRatio);

      canvas.style.width = `${viewport.width}px`;

      canvas.style.height = `${viewport.height}px`;

      canvas.className = 'block max-w-none bg-white shadow-xl shadow-slate-300/60';

      context.setTransform(devicePixelRatio, 0, 0, devicePixelRatio, 0, 0);

      if (this.destroyed) {
        return;
      }

      container.appendChild(canvas);

      this.renderTask = page.render({
        canvasContext: context,
        viewport,
      });

      await this.renderTask.promise;

      if (!this.destroyed) {
        this.renderTask = null;
      }
    } catch (error: any) {
      if (error?.name === 'RenderingCancelledException') {
        return;
      }

      if (this.destroyed) {
        return;
      }

      console.error('Error al renderizar página:', error);
    }
  }
}
