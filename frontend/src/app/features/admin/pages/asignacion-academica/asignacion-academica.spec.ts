import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AsignacionAcademica } from './asignacion-academica';

describe('AsignacionAcademica', () => {
  let component: AsignacionAcademica;
  let fixture: ComponentFixture<AsignacionAcademica>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AsignacionAcademica],
    }).compileComponents();

    fixture = TestBed.createComponent(AsignacionAcademica);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
