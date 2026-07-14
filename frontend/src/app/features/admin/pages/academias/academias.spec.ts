import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Academias } from './academias';

describe('Academias', () => {
  let component: Academias;
  let fixture: ComponentFixture<Academias>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Academias],
    }).compileComponents();

    fixture = TestBed.createComponent(Academias);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
