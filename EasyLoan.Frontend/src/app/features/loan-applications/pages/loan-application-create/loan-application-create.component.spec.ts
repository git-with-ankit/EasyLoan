import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoanApplicationCreateComponent } from './loan-application-create.component';

describe('LoanApplicationCreateComponent', () => {
  let component: LoanApplicationCreateComponent;
  let fixture: ComponentFixture<LoanApplicationCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanApplicationCreateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoanApplicationCreateComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
