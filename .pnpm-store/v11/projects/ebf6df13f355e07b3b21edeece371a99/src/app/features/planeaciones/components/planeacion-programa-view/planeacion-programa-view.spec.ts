import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaneacionProgramaView } from './planeacion-programa-view';

describe('PlaneacionProgramaView', () => {
  let component: PlaneacionProgramaView;
  let fixture: ComponentFixture<PlaneacionProgramaView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlaneacionProgramaView],
    }).compileComponents();

    fixture = TestBed.createComponent(PlaneacionProgramaView);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
