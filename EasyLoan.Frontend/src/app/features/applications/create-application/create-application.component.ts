import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormControl, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { LoanTypeService } from '../../../services/loan-type.service';
import { ApplicationService } from '../../../services/application.service';
import { LoanType, EmiScheduleItem } from '../../../models/loan-type.models';
import { EmiPlanPreviewComponent } from './emi-plan-preview/emi-plan-preview.component';
import { createPaginationParams } from '../../../models/pagination.models';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
    selector: 'app-create-application',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        EmiPlanPreviewComponent,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatButtonModule,
        MatCardModule,
        MatIconModule,
        MatProgressSpinnerModule
    ],
    templateUrl: './create-application.component.html',
    styleUrl: './create-application.component.css'
})
export class CreateApplicationComponent implements OnInit {
    loanTypes = signal<LoanType[]>([]);
    selectedLoanType = signal<LoanType | null>(null);
    emiPlan = signal<EmiScheduleItem[]>([]);

    isLoadingLoanTypes = signal(false);
    isLoadingEmiPlan = signal(false);
    isSubmitting = signal(false);

    errorMessage = signal('');
    successMessage = signal('');

    private destroyRef = inject(DestroyRef);

    applicationForm!: FormGroup;

    constructor(
        private fb: FormBuilder,
        private loanTypeService: LoanTypeService,
        private applicationService: ApplicationService,
        private router: Router
    ) {
        this.initializeForm();
    }

    ngOnInit(): void {
        this.loadLoanTypes();
    }

    private initializeForm(): void {
        this.applicationForm = this.fb.group({
            loanTypeId: ['', [Validators.required]],
            requestedAmount: [0, [Validators.required, Validators.min(1), Validators.max(1000000)]],
            requestedTenureInMonths: [0, [Validators.required, Validators.min(1), this.integerValidator]]
        });
    }

    loadLoanTypes(): void {
        this.isLoadingLoanTypes.set(true);
        this.errorMessage.set('');

        this.loanTypeService.getLoanTypes()
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
                next: (data) => {
                    this.loanTypes.set(data);
                    this.isLoadingLoanTypes.set(false);
                },
                error: (error) => {
                    this.errorMessage.set(error.message || 'Failed to load loan types');
                    this.isLoadingLoanTypes.set(false);
                }
            });
    }

    onLoanTypeChange(event: any): void {
        const loanTypeId = event.value || event.target?.value;

        const loanType = this.loanTypes().find(lt => lt.id === loanTypeId);
        this.selectedLoanType.set(loanType || null);

        // Reset amount and tenure
        this.applicationForm.patchValue({
            requestedAmount: 0,
            requestedTenureInMonths: 0
        });

        // Clear EMI plan
        this.emiPlan.set([]);
        this.errorMessage.set('');

        // Update validators based on selected loan type
        if (loanType) {
            const amountControl = this.applicationForm.get('requestedAmount');
            const tenureControl = this.applicationForm.get('requestedTenureInMonths');

            amountControl?.setValidators([
                Validators.required,
                Validators.min(loanType.minAmount),
                Validators.max(1000000)
            ]);

            tenureControl?.setValidators([
                Validators.required,
                Validators.min(1),
                Validators.max(loanType.maxTenureInMonths),
                this.integerValidator
            ]);

            amountControl?.updateValueAndValidity();
            tenureControl?.updateValueAndValidity();
        }
    }

    onPreviewEmi(): void {
        if (this.applicationForm.invalid) {
            this.applicationForm.markAllAsTouched();
            this.errorMessage.set('Please fill all required fields correctly');
            return;
        }

        const loanTypeId = this.applicationForm.value.loanTypeId;
        const amount = this.applicationForm.value.requestedAmount;
        const tenure = this.applicationForm.value.requestedTenureInMonths;

        this.isLoadingEmiPlan.set(true);
        this.errorMessage.set('');

        // Fetch all EMI schedule items across all pages
        this.fetchAllEmiPages(loanTypeId, amount, tenure);
    }

    private fetchAllEmiPages(loanTypeId: string, amount: number, tenure: number, pageNumber: number = 1, accumulatedItems: any[] = []): void {
        const pagination = createPaginationParams(pageNumber, 100); // Max allowed by backend

        this.loanTypeService.previewEmiPlan(loanTypeId, amount, tenure, pagination)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
                next: (response) => {
                    // Accumulate items from this page
                    const allItems = [...accumulatedItems, ...response.items];

                    // Check if there are more pages
                    if (pageNumber < response.totalPages) {
                        // Fetch next page
                        this.fetchAllEmiPages(loanTypeId, amount, tenure, pageNumber + 1, allItems);
                    } else {
                        // All pages fetched, set the complete EMI plan
                        this.emiPlan.set(allItems);
                        this.isLoadingEmiPlan.set(false);
                    }
                },
                error: (error) => {
                    // this.errorMessage.set(error.error?.message || 'Failed to generate EMI plan');
                    this.isLoadingEmiPlan.set(false);
                }
            });
    }

    onSubmit(): void {
        if (this.applicationForm.invalid) {
            this.applicationForm.markAllAsTouched();
            this.errorMessage.set('Please fill all required fields correctly');
            return;
        }

        this.isSubmitting.set(true);
        this.errorMessage.set('');
        this.successMessage.set('');

        this.applicationService.createApplication(this.applicationForm.value)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
                next: (response) => {
                    this.successMessage.set(`Application submitted successfully! Application Number: ${response.applicationNumber}`);
                    this.isSubmitting.set(false);

                    // Redirect to applications list after 2 seconds
                    setTimeout(() => {
                        this.router.navigate(['/customer/applications']);
                    }, 2000);
                },
                error: (error) => {
                    this.errorMessage.set(error.error?.message || 'Failed to submit application. Please try again.');
                    this.isSubmitting.set(false);
                }
            });
    }

    resetForm(): void {
        this.applicationForm.reset({
            loanTypeId: '',
            requestedAmount: 0,
            requestedTenureInMonths: 0
        });
        this.selectedLoanType.set(null);
        this.emiPlan.set([]);
        this.errorMessage.set('');
        this.successMessage.set('');
    }

    // Custom validator for integer values
    integerValidator(control: AbstractControl): ValidationErrors | null {
        const value = control.value;
        if (value && !Number.isInteger(Number(value))) {
            return { notInteger: true };
        }
        return null;
    }

    // Prevent 'e', '+', '-' for amount field
    onAmountKeyDown(event: KeyboardEvent): void {
        const input = event.target as HTMLInputElement;
        const currentValue = input.value;

        // Prevent 'e', 'E', '+', '-'
        if (['e', 'E', '+', '-'].includes(event.key)) {
            event.preventDefault();
            return;
        }

        // Allow control keys (backspace, delete, tab, arrows, etc.)
        const allowedKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];
        if (allowedKeys.includes(event.key)) {
            return;
        }

        // Allow Ctrl/Cmd combinations (copy, paste, select all, etc.)
        if (event.ctrlKey || event.metaKey) {
            return;
        }

        // Prevent input if already at 7 characters (1000000 = 1 million)
        if (currentValue.length >= 7 && !input.selectionStart !== !input.selectionEnd) {
            return;
        }

        if (currentValue.length >= 7) {
            event.preventDefault();
        }
    }

    // Prevent letters, special characters, and decimal points for tenure field
    onTenureKeyDown(event: KeyboardEvent): void {
        // Prevent 'e', 'E', '+', '-', '.'
        if (['e', 'E', '+', '-', '.'].includes(event.key)) {
            event.preventDefault();
            return;
        }

        // Allow control keys (backspace, delete, tab, arrows, etc.)
        const allowedKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];
        if (allowedKeys.includes(event.key)) {
            return;
        }

        // Allow Ctrl/Cmd combinations
        if (event.ctrlKey || event.metaKey) {
            return;
        }
    }

    // Getters for form controls
    get loanTypeId() { return this.applicationForm.get('loanTypeId'); }
    get requestedAmount() { return this.applicationForm.get('requestedAmount'); }
    get requestedTenureInMonths() { return this.applicationForm.get('requestedTenureInMonths'); }
}
