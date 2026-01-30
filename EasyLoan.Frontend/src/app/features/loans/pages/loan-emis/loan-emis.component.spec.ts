import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoanEmisComponent } from './loan-emis.component';

describe('LoanEmisComponent', () => {
  let component: LoanEmisComponent;
  let fixture: ComponentFixture<LoanEmisComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanEmisComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoanEmisComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
