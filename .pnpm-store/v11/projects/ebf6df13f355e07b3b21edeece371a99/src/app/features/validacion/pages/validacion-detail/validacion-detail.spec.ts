import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ValidacionDetail } from './validacion-detail';

describe('ValidacionDetail', () => {
  let component: ValidacionDetail;
  let fixture: ComponentFixture<ValidacionDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ValidacionDetail],
    }).compileComponents();

    fixture = TestBed.createComponent(ValidacionDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
