import { Component, Inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormControl, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { LoanService } from '../../../../services/loan.service';
import { LoanDetails, DueEmiResponse, EmiDueStatus, PaymentHistory } from '../../../../models/loan.models';

@Component({
    selector: 'app-loan-details-card',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatTabsModule,
        MatFormFieldModule,
        MatInputModule,
        MatTableModule,
        MatChipsModule
    ],
    templateUrl: './loan-details-card.component.html',
    styleUrl: './loan-details-card.component.css'
})
export class LoanDetailsCardComponent implements OnInit {
    loanDetails = signal<LoanDetails | null>(null);
    emis = signal<DueEmiResponse[]>([]);
    paymentHistory = signal<PaymentHistory[]>([]);
    selectedEmiStatus = signal<EmiDueStatus>(EmiDueStatus.Overdue);
    selectedEmi = signal<DueEmiResponse | null>(null);
    paymentAmount: FormControl;
    isLoading = signal(false);
    isLoadingHistory = signal(false);
    isProcessingPayment = signal(false);
    errorMessage = signal('');
    successMessage = signal('');

    EmiDueStatus = EmiDueStatus; // Expose enum to template
    displayedColumns: string[] = ['date', 'amount', 'status'];
    private readonly MAX_PAYMENT_AMOUNT = 1000000000000000; // 1000 trillion

    constructor(
        private loanService: LoanService,
        public dialogRef: MatDialogRef<LoanDetailsCardComponent>,
        @Inject(MAT_DIALOG_DATA) public data: { loanNumber: string }
    ) {
        this.paymentAmount = new FormControl(0, [
            Validators.required,
            Validators.min(0.01),
            Validators.max(this.MAX_PAYMENT_AMOUNT),
            this.decimalPlacesValidator(2)
        ]);
    }

    ngOnInit(): void {
        this.loadLoanDetails();
        this.loadEmis();
        this.loadPaymentHistory();
    }

    loadLoanDetails(): void {
        this.isLoading.set(true);
        this.loanService.getLoanDetails(this.data.loanNumber).subscribe({
            next: (data) => {
                this.loanDetails.set(data);
                this.isLoading.set(false);
            },
            error: (error) => {
                this.errorMessage.set(error.message || 'Failed to load loan details');
                this.isLoading.set(false);
            }
        });
    }

    loadEmis(): void {
        this.loanService.getDueEmisForLoan(this.data.loanNumber, this.selectedEmiStatus()).subscribe({
            next: (data) => {
                this.emis.set(data);
            },
            error: (error) => {
                console.error('Failed to load EMIs:', error);
            }
        });
    }

    loadPaymentHistory(): void {
        this.isLoadingHistory.set(true);
        this.loanService.getPaymentHistory(this.data.loanNumber).subscribe({
            next: (data) => {
                this.paymentHistory.set(data);
                this.isLoadingHistory.set(false);
            },
            error: (error) => {
                console.error('Failed to load payment history:', error);
                this.isLoadingHistory.set(false);
            }
        });
    }

    onEmiStatusChange(status: EmiDueStatus): void {
        this.selectedEmiStatus.set(status);
        this.selectedEmi.set(null);
        this.loadEmis();
    }

    onSelectEmi(emi: DueEmiResponse): void {
        this.selectedEmi.set(emi);
        this.paymentAmount.setValue(this.roundToTwoDecimals(emi.remainingEmiAmount + emi.penaltyAmount));
        this.successMessage.set('');
        this.errorMessage.set('');
    }



    onPaymentAmountKeyDown(event: KeyboardEvent): void {
        const input = event.target as HTMLInputElement;
        const currentValue = input.value;

        // Prevent 'e', 'E', '+', '-'
        if (['e', 'E', '+', '-'].includes(event.key)) {
            event.preventDefault();
            return;
        }

        // Allow: backspace, delete, tab, escape, enter, decimal point
        if (['Backspace', 'Delete', 'Tab', 'Escape', 'Enter', 'ArrowLeft', 'ArrowRight'].includes(event.key)) {
            return;
        }

        // Allow Ctrl/Cmd combinations (copy, paste, select all, etc.)
        if (event.ctrlKey || event.metaKey) {
            return;
        }

        // Prevent multiple decimal points
        if (event.key === '.' && currentValue.includes('.')) {
            event.preventDefault();
            return;
        }

        // Limit to 2 decimal places
        if (currentValue.includes('.')) {
            const parts = currentValue.split('.');
            const decimalPart = parts[1] || '';
            const selectionStart = input.selectionStart || 0;
            const selectionEnd = input.selectionEnd || 0;
            const decimalIndex = currentValue.indexOf('.');

            // If we already have 2 decimal places and cursor is after decimal point
            if (decimalPart.length >= 2 && selectionStart > decimalIndex && selectionStart === selectionEnd && event.key >= '0' && event.key <= '9') {
                event.preventDefault();
                return;
            }
        }

        // Prevent input if already at 16 characters (1000 trillion = 1,000,000,000,000,000)
        // Don't count the decimal point in the character limit
        const valueWithoutDecimal = currentValue.replace('.', '');
        if (valueWithoutDecimal.length >= 16 && event.key >= '0' && event.key <= '9') {
            const selectionStart = input.selectionStart || 0;
            const selectionEnd = input.selectionEnd || 0;
            // Allow if there's a selection (user is replacing text)
            if (selectionStart === selectionEnd) {
                event.preventDefault();
            }
        }
    }

    roundToTwoDecimals(value: number): number {
        return Math.round(value * 100) / 100;
    }

    onMakePayment(): void {
        // Validate EMI selection
        if (!this.selectedEmi()) {
            this.errorMessage.set('Please select an EMI to make a payment');
            return;
        }

        // Mark as touched to show validation errors
        this.paymentAmount.markAsTouched();

        // Check if form is valid
        if (this.paymentAmount.invalid) {
            return;
        }

        // Get and validate payment amount
        const amount = this.paymentAmount.value;

        // Validate decimal places
        const amountStr = amount.toString();
        if (amountStr.includes('.')) {
            const decimalPlaces = amountStr.split('.')[1]?.length || 0;
            if (decimalPlaces > 2) {
                this.errorMessage.set('Payment amount can have at most 2 decimal places');
                return;
            }
        }

        // Ensure payment amount is rounded to 2 decimal places
        const roundedAmount = this.roundToTwoDecimals(amount);
        this.paymentAmount.setValue(roundedAmount);

        this.isProcessingPayment.set(true);
        this.errorMessage.set('');

        this.loanService.makePayment(this.data.loanNumber, roundedAmount).subscribe({
            next: (response) => {
                this.successMessage.set(`Payment successful! Transaction ID: ${response.transactionId}`);
                this.isProcessingPayment.set(false);
                this.selectedEmi.set(null);
                this.paymentAmount.reset(0);
                this.loadEmis(); // Refresh EMI list
                this.loadLoanDetails(); // Refresh loan details
                this.loadPaymentHistory(); // Refresh payment history
            },
            error: (error) => {
                this.errorMessage.set(error.error?.message || 'Payment failed. Please try again.');
                this.isProcessingPayment.set(false);
            }
        });
    }

    onClose(): void {
        this.dialogRef.close();
    }

    getTotalDue(emi: DueEmiResponse): number {
        return emi.remainingEmiAmount + emi.penaltyAmount;
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
}
