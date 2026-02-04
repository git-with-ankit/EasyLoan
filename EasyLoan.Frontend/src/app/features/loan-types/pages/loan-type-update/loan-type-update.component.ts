import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { LoanTypeService } from '../../../../shared/services/loan-type.service';
import { LoanType } from '../../../../shared/models/loan-type.models';

@Component({
  selector: 'app-loan-type-update',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './loan-type-update.component.html',
  styleUrl: './loan-type-update.component.css',
})
export class LoanTypeUpdateComponent implements OnInit {
  updateForm!: FormGroup;
  loanType = signal<LoanType | null>(null);
  loading = signal<boolean>(true);
  submitting = signal<boolean>(false);
  error = signal<string | null>(null);
  loanTypeId: string = '';

  constructor(
    private fb: FormBuilder,
    private loanTypeService: LoanTypeService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.loanTypeId = this.route.snapshot.paramMap.get('id') || '';

    this.updateForm = this.fb.group({
      interestRate: ['', [Validators.required, Validators.min(0.01), Validators.max(100)]],
      minAmount: ['', [Validators.required, Validators.min(1)]],
      maxTenureInMonths: ['', [Validators.required, Validators.min(1), Validators.max(480)]]
    });

    this.loadLoanType();
  }

  loadLoanType(): void {
    this.loading.set(true);
    this.error.set(null);

    this.loanTypeService.getLoanTypeById(this.loanTypeId).subscribe({
      next: (data) => {
        this.loanType.set(data);
        this.updateForm.patchValue({
          interestRate: data.interestRate,
          minAmount: data.minAmount,
          maxTenureInMonths: data.maxTenureInMonths
        });
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set('Failed to load loan type. Please try again.');
        console.error('Error loading loan type:', err);
      }
    });
  }

  onSubmit(): void {
    if (this.updateForm.invalid) {
      this.updateForm.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    this.loanTypeService.updateLoanType(this.loanTypeId, this.updateForm.value).subscribe({
      next: () => {
        this.router.navigate(['/admin/dashboard/loan-types']);
      },
      error: (err) => {
        this.submitting.set(false);
        this.error.set(err.error?.title || 'Failed to update loan type. Please try again.');
        console.error('Error updating loan type:', err);
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/admin/dashboard/loan-types']);
  }

  getFieldError(fieldName: string): string | null {
    const field = this.updateForm.get(fieldName);
    if (field?.touched && field?.errors) {
      if (field.errors['required']) return `${this.getFieldLabel(fieldName)} is required`;
      if (field.errors['min']) return `${this.getFieldLabel(fieldName)} must be at least ${field.errors['min'].min}`;
      if (field.errors['max']) return `${this.getFieldLabel(fieldName)} cannot exceed ${field.errors['max'].max}`;
    }
    return null;
  }

  getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      interestRate: 'Interest Rate',
      minAmount: 'Minimum Amount',
      maxTenureInMonths: 'Maximum Tenure'
    };
    return labels[fieldName] || fieldName;
  }
}
