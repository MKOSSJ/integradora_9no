import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaneacionInfoPanel } from './planeacion-info-panel';

describe('PlaneacionInfoPanel', () => {
  let component: PlaneacionInfoPanel;
  let fixture: ComponentFixture<PlaneacionInfoPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlaneacionInfoPanel],
    }).compileComponents();

    fixture = TestBed.createComponent(PlaneacionInfoPanel);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
