import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { SeguimientoPlaneaciones } from './seguimiento-planeaciones';

describe('SeguimientoPlaneaciones', () => {
  let component: SeguimientoPlaneaciones;
  let fixture: ComponentFixture<SeguimientoPlaneaciones>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SeguimientoPlaneaciones],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(SeguimientoPlaneaciones);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('lista las planeaciones disponibles sin inventar fechas límite', () => {
    expect(component.items().length).toBeGreaterThan(0);

    for (const item of component.items()) {
      if (!item.fechaLimiteCaptura) {
        expect(item.diasRestantes).toBeUndefined();
        expect(item.estadoSeguimiento).toBe('sin-fecha');
      }
    }
  });
});
