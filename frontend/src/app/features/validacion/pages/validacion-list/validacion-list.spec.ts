import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ValidacionList } from './validacion-list';

describe('ValidacionList', () => {
  let component: ValidacionList;
  let fixture: ComponentFixture<ValidacionList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ValidacionList],
    }).compileComponents();

    fixture = TestBed.createComponent(ValidacionList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
