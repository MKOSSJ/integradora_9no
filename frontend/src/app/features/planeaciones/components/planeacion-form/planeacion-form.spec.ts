import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaneacionForm } from './planeacion-form';

describe('PlaneacionForm', () => {
  let component: PlaneacionForm;
  let fixture: ComponentFixture<PlaneacionForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlaneacionForm],
    }).compileComponents();

    fixture = TestBed.createComponent(PlaneacionForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
