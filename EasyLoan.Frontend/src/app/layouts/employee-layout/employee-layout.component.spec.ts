import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmployeeLayoutComponent } from './employee-layout.component';

describe('EmployeeLayoutComponent', () => {
  let component: EmployeeLayoutComponent;
  let fixture: ComponentFixture<EmployeeLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmployeeLayoutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmployeeLayoutComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
