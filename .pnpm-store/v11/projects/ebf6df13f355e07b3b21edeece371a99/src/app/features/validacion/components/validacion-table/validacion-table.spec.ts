import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ValidacionTable } from './validacion-table';

describe('ValidacionTable', () => {
  let component: ValidacionTable;
  let fixture: ComponentFixture<ValidacionTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ValidacionTable],
    }).compileComponents();

    fixture = TestBed.createComponent(ValidacionTable);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
