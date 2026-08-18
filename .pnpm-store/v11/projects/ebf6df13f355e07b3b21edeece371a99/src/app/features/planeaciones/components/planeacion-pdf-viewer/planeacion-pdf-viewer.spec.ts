import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaneacionPdfViewer } from './planeacion-pdf-viewer';

describe('PlaneacionPdfViewer', () => {
  let component: PlaneacionPdfViewer;
  let fixture: ComponentFixture<PlaneacionPdfViewer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlaneacionPdfViewer],
    }).compileComponents();

    fixture = TestBed.createComponent(PlaneacionPdfViewer);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
