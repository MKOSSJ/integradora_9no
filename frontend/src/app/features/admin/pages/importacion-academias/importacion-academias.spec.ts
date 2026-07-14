import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportacionAcademias } from './importacion-academias';

describe('ImportacionAcademias', () => {
  let component: ImportacionAcademias;
  let fixture: ComponentFixture<ImportacionAcademias>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImportacionAcademias],
    }).compileComponents();

    fixture = TestBed.createComponent(ImportacionAcademias);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
