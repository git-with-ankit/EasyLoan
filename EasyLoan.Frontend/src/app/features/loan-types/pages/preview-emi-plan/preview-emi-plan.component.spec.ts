import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PreviewEmiPlanComponent } from './preview-emi-plan.component';

describe('PreviewEmiPlanComponent', () => {
  let component: PreviewEmiPlanComponent;
  let fixture: ComponentFixture<PreviewEmiPlanComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PreviewEmiPlanComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PreviewEmiPlanComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
