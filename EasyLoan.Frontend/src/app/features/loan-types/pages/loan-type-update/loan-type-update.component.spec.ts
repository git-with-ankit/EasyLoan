import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoanTypeUpdateComponent } from './loan-type-update.component';

describe('LoanTypeUpdateComponent', () => {
  let component: LoanTypeUpdateComponent;
  let fixture: ComponentFixture<LoanTypeUpdateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanTypeUpdateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoanTypeUpdateComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
