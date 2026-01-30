import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoanPaymentsHistory } from './loan-payments-history';

describe('LoanPaymentsHistory', () => {
  let component: LoanPaymentsHistory;
  let fixture: ComponentFixture<LoanPaymentsHistory>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanPaymentsHistory]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoanPaymentsHistory);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
