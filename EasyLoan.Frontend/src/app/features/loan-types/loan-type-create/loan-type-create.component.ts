import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LoanTypeService } from '../../../shared/services/loan-type.service';

@Component({
  selector: 'app-loan-type-create',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './loan-type-create.component.html',
  styleUrl: './loan-type-create.component.css',
})
export class LoanTypeCreateComponent implements OnInit {
  createForm!: FormGroup;
  submitting = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private loanTypeService: LoanTypeService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.createForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      interestRate: ['', [Validators.required, Validators.min(0.01), Validators.max(100)]],
      minAmount: ['', [Validators.required, Validators.min(1)]],
      maxTenureInMonths: ['', [Validators.required, Validators.min(1), Validators.max(480)]]
    });
  }

  onSubmit(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.error = null;

    this.loanTypeService.createLoanType(this.createForm.value).subscribe({
      next: () => {
        this.router.navigate(['/admin/dashboard/loan-types']);
      },
      error: (err) => {
        this.submitting = false;
        this.error = err.error?.title || 'Failed to create loan type. Please try again.';
        console.error('Error creating loan type:', err);
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/admin/dashboard/loan-types']);
  }

  getFieldError(fieldName: string): string | null {
    const field = this.createForm.get(fieldName);
    if (field?.touched && field?.errors) {
      if (field.errors['required']) return `${this.getFieldLabel(fieldName)} is required`;
      if (field.errors['maxLength']) return `${this.getFieldLabel(fieldName)} cannot exceed ${field.errors['maxLength'].requiredLength} characters`;
      if (field.errors['min']) return `${this.getFieldLabel(fieldName)} must be at least ${field.errors['min'].min}`;
      if (field.errors['max']) return `${this.getFieldLabel(fieldName)} cannot exceed ${field.errors['max'].max}`;
    }
    return null;
  }

  getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      name: 'Name',
      interestRate: 'Interest Rate',
      minAmount: 'Minimum Amount',
      maxTenureInMonths: 'Maximum Tenure'
    };
    return labels[fieldName] || fieldName;
  }
}
