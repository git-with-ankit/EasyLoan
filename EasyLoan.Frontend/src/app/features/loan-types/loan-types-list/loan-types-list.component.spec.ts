import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoanTypesListComponent } from './loan-types-list.component';

describe('LoanTypesListComponent', () => {
  let component: LoanTypesListComponent;
  let fixture: ComponentFixture<LoanTypesListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanTypesListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoanTypesListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
