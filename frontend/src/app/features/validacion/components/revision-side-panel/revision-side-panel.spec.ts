import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RevisionSidePanel } from './revision-side-panel';

describe('RevisionSidePanel', () => {
  let component: RevisionSidePanel;
  let fixture: ComponentFixture<RevisionSidePanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RevisionSidePanel],
    }).compileComponents();

    fixture = TestBed.createComponent(RevisionSidePanel);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
