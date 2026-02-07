import { Component, OnInit, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoanTypeService } from '../loan-type.service';
import { LoanType } from '../loan-type.models';

@Component({
    selector: 'app-loan-type-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatCardModule,
        MatIconModule,
        MatProgressSpinnerModule
    ],
    templateUrl: './loan-type-form.component.html',
    styleUrl: './loan-type-form.component.css'
})
export class LoanTypeFormComponent implements OnInit {
    @Input() mode: 'create' | 'update' = 'create';
    @Input() loanTypeId?: string;
    @Output() formSubmit = new EventEmitter<any>();
    @Output() formCancel = new EventEmitter<void>();

    loanTypeForm!: FormGroup;
    loanType = signal<LoanType | null>(null);
    loading = signal<boolean>(false);
    submitting = signal<boolean>(false);
    error = signal<string | null>(null);

    constructor(
        private fb: FormBuilder,
        private loanTypeService: LoanTypeService
    ) { }

    ngOnInit(): void {
        this.initializeForm();

        if (this.mode === 'update' && this.loanTypeId) {
            this.loadLoanType();
        }
    }

    initializeForm(): void {
        const formConfig: any = {
            interestRate: ['', [Validators.required, Validators.min(0.01), Validators.max(100), this.decimalPlacesValidator(2)]],
            minAmount: ['', [Validators.required, Validators.min(1), Validators.max(1000000), this.decimalPlacesValidator(2)]],
            maxTenureInMonths: ['', [Validators.required, Validators.min(1), Validators.max(480), this.integerValidator]]
        };

        // Add name field only for create mode
        if (this.mode === 'create') {
            formConfig.name = ['', [Validators.required, Validators.maxLength(100)]];
        }

        this.loanTypeForm = this.fb.group(formConfig);
    }

    loadLoanType(): void {
        if (!this.loanTypeId) return;

        this.loading.set(true);
        this.error.set(null);

        this.loanTypeService.getLoanTypeById(this.loanTypeId).subscribe({
            next: (data) => {
                this.loanType.set(data);
                this.loanTypeForm.patchValue({
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
        if (this.loanTypeForm.invalid) {
            this.loanTypeForm.markAllAsTouched();
            return;
        }

        this.submitting.set(true);
        this.formSubmit.emit(this.loanTypeForm.value);
    }

    onCancel(): void {
        this.formCancel.emit();
    }

    // Custom validator for integer values
    integerValidator(control: AbstractControl): ValidationErrors | null {
        const value = control.value;
        if (value && !Number.isInteger(Number(value))) {
            return { notInteger: true };
        }
        return null;
    }

    // Custom validator for decimal places
    decimalPlacesValidator(maxDecimalPlaces: number) {
        return (control: AbstractControl): ValidationErrors | null => {
            if (control.value === null || control.value === undefined || control.value === '') {
                return null;
            }

            const value = control.value.toString();
            const decimalIndex = value.indexOf('.');

            if (decimalIndex === -1) {
                return null; // No decimal point, valid
            }

            const decimalPlaces = value.length - decimalIndex - 1;

            if (decimalPlaces > maxDecimalPlaces) {
                return { decimalPlaces: { max: maxDecimalPlaces, actual: decimalPlaces } };
            }

            return null;
        };
    }

    // Prevent 'e', '+', '-' for interest rate field
    onInterestRateKeyDown(event: KeyboardEvent): void {
        if (['e', 'E', '+', '-'].includes(event.key)) {
            event.preventDefault();
            return;
        }

        const allowedKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];
        if (allowedKeys.includes(event.key)) {
            return;
        }

        if (event.ctrlKey || event.metaKey) {
            return;
        }
    }

    // Prevent 'e', '+', '-' for min amount field
    onMinAmountKeyDown(event: KeyboardEvent): void {
        if (['e', 'E', '+', '-'].includes(event.key)) {
            event.preventDefault();
            return;
        }

        const allowedKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];
        if (allowedKeys.includes(event.key)) {
            return;
        }

        if (event.ctrlKey || event.metaKey) {
            return;
        }
    }

    // Prevent 'e', '+', '-', '.' for max tenure field (integers only)
    onMaxTenureKeyDown(event: KeyboardEvent): void {
        if (['e', 'E', '+', '-', '.'].includes(event.key)) {
            event.preventDefault();
            return;
        }

        const allowedKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];
        if (allowedKeys.includes(event.key)) {
            return;
        }

        if (event.ctrlKey || event.metaKey) {
            return;
        }
    }

    get isCreateMode(): boolean {
        return this.mode === 'create';
    }

    get isUpdateMode(): boolean {
        return this.mode === 'update';
    }
}
