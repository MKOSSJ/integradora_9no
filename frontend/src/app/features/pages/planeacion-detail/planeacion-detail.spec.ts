import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaneacionDetail } from './planeacion-detail';

describe('PlaneacionDetail', () => {
  let component: PlaneacionDetail;
  let fixture: ComponentFixture<PlaneacionDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlaneacionDetail],
    }).compileComponents();

    fixture = TestBed.createComponent(PlaneacionDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
