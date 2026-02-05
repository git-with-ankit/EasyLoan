import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoanTypeCreateComponent } from './loan-type-create.component';

describe('LoanTypeCreateComponent', () => {
  let component: LoanTypeCreateComponent;
  let fixture: ComponentFixture<LoanTypeCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanTypeCreateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoanTypeCreateComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
