import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoanApplicationReviewComponent } from './loan-application-review.component';

describe('LoanApplicationReviewComponent', () => {
  let component: LoanApplicationReviewComponent;
  let fixture: ComponentFixture<LoanApplicationReviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanApplicationReviewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoanApplicationReviewComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
