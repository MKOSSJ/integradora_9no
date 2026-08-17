import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SeguimientoPlaneaciones } from './seguimiento-planeaciones';

describe('SeguimientoPlaneaciones', () => {
  let component: SeguimientoPlaneaciones;
  let fixture: ComponentFixture<SeguimientoPlaneaciones>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SeguimientoPlaneaciones],
    }).compileComponents();

    fixture = TestBed.createComponent(SeguimientoPlaneaciones);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
