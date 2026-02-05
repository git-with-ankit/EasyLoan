import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormControl, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { LoanTypeService } from '../../loan-types/loan-type.service';
import { ApplicationService } from '../application.service';
import { LoanType, EmiScheduleItem } from '../../loan-types/loan-type.models';
import { EmiPlanPreviewComponent } from './emi-plan-preview/emi-plan-preview.component';

@Component({
    selector: 'app-create-application',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, EmiPlanPreviewComponent],
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
            requestedAmount: [0, [Validators.required, Validators.min(1)]],
            requestedTenureInMonths: [0, [Validators.required, Validators.min(1)]]
        });
    }

    loadLoanTypes(): void {
        this.isLoadingLoanTypes.set(true);
        this.errorMessage.set('');

        this.loanTypeService.getLoanTypes().subscribe({
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

    onLoanTypeChange(event: Event): void {
        const selectElement = event.target as HTMLSelectElement;
        const loanTypeId = selectElement.value;

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
                Validators.min(loanType.minAmount)
            ]);

            tenureControl?.setValidators([
                Validators.required,
                Validators.min(1),
                Validators.max(loanType.maxTenureInMonths)
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

        this.loanTypeService.previewEmiPlan(loanTypeId, amount, tenure).subscribe({
            next: (data) => {
                this.emiPlan.set(data);
                this.isLoadingEmiPlan.set(false);
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

        this.applicationService.createApplication(this.applicationForm.value).subscribe({
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

    // Getters for form controls
    get loanTypeId() { return this.applicationForm.get('loanTypeId'); }
    get requestedAmount() { return this.applicationForm.get('requestedAmount'); }
    get requestedTenureInMonths() { return this.applicationForm.get('requestedTenureInMonths'); }
}
