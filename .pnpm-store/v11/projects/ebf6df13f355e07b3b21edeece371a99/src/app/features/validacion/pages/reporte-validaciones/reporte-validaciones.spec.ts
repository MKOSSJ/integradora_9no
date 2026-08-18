import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReporteValidaciones } from './reporte-validaciones';

describe('ReporteValidaciones', () => {
  let component: ReporteValidaciones;
  let fixture: ComponentFixture<ReporteValidaciones>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReporteValidaciones],
    }).compileComponents();

    fixture = TestBed.createComponent(ReporteValidaciones);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
