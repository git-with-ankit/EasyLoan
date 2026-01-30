import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoanApplicationsListComponent } from './loan-applications-list.component';

describe('LoanApplicationsListComponent', () => {
  let component: LoanApplicationsListComponent;
  let fixture: ComponentFixture<LoanApplicationsListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanApplicationsListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoanApplicationsListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
