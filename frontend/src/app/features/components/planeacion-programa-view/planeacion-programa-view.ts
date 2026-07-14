import { Component, Input } from '@angular/core';
import { LucideDynamicIcon, LucideBookOpen } from '@lucide/angular';

import { PlaneacionDetail } from '../../../core/models/planeacion.model';

@Component({
  selector: 'app-planeacion-programa-view',
  standalone: true,
  imports: [
    LucideDynamicIcon
  ],
  templateUrl: './planeacion-programa-view.html',
  styleUrl: './planeacion-programa-view.css'
})
export class PlaneacionProgramaView {
  @Input({ required: true }) planeacion!: PlaneacionDetail;

  bookIcon = LucideBookOpen;
}