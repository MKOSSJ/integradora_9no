import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportarProfesores } from './importar-profesores';

describe('ImportarProfesores', () => {
  let component: ImportarProfesores;
  let fixture: ComponentFixture<ImportarProfesores>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImportarProfesores],
    }).compileComponents();

    fixture = TestBed.createComponent(ImportarProfesores);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
