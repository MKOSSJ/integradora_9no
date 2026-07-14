import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaneacionesList } from './planeaciones-list';

describe('PlaneacionesList', () => {
  let component: PlaneacionesList;
  let fixture: ComponentFixture<PlaneacionesList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlaneacionesList],
    }).compileComponents();

    fixture = TestBed.createComponent(PlaneacionesList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
