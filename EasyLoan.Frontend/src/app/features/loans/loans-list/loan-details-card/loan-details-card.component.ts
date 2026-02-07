import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LoanService } from '../../../../services/loan.service';
import { LoanDetails, DueEmiResponse, EmiDueStatus, PaymentHistory } from '../../../../models/loan.models';

@Component({
    selector: 'app-loan-details-card',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './loan-details-card.component.html',
    styleUrl: './loan-details-card.component.css'
})
export class LoanDetailsCardComponent implements OnInit {
    @Input({ required: true }) loanNumber!: string;
    @Output() close = new EventEmitter<void>();
    @Output() paymentSuccess = new EventEmitter<void>();

    loanDetails = signal<LoanDetails | null>(null);
    emis = signal<DueEmiResponse[]>([]);
    paymentHistory = signal<PaymentHistory[]>([]);
    selectedEmiStatus = signal<EmiDueStatus>(EmiDueStatus.Overdue);
    selectedEmi = signal<DueEmiResponse | null>(null);
    paymentAmount = signal<number>(0);
    isLoading = signal(false);
    isLoadingHistory = signal(false);
    isProcessingPayment = signal(false);
    errorMessage = signal('');
    successMessage = signal('');

    EmiDueStatus = EmiDueStatus; // Expose enum to template

    constructor(private loanService: LoanService) { }

    ngOnInit(): void {
        this.loadLoanDetails();
        this.loadEmis();
        this.loadPaymentHistory();
    }

    loadLoanDetails(): void {
        this.isLoading.set(true);
        this.loanService.getLoanDetails(this.loanNumber).subscribe({
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
        this.loanService.getDueEmisForLoan(this.loanNumber, this.selectedEmiStatus()).subscribe({
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
        this.loanService.getPaymentHistory(this.loanNumber).subscribe({
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
        this.paymentAmount.set(this.roundToTwoDecimals(emi.remainingEmiAmount + emi.penaltyAmount));
        this.successMessage.set('');
        this.errorMessage.set('');
    }

    onPaymentAmountChange(value: number): void {
        // Round to 2 decimal places to prevent loss of money
        const rounded = this.roundToTwoDecimals(value);
        this.paymentAmount.set(rounded);
    }

    roundToTwoDecimals(value: number): number {
        return Math.round(value * 100) / 100;
    }

    onMakePayment(): void {
        if (!this.selectedEmi() || this.paymentAmount() <= 0) {
            this.errorMessage.set('Please select an EMI and enter a valid amount');
            return;
        }

        // Ensure payment amount is rounded to 2 decimal places
        const roundedAmount = this.roundToTwoDecimals(this.paymentAmount());
        this.paymentAmount.set(roundedAmount);

        this.isProcessingPayment.set(true);
        this.errorMessage.set('');

        this.loanService.makePayment(this.loanNumber, roundedAmount).subscribe({
            next: (response) => {
                this.successMessage.set(`Payment successful! Transaction ID: ${response.transactionId}`);
                this.isProcessingPayment.set(false);
                this.selectedEmi.set(null);
                this.paymentAmount.set(0);
                this.loadEmis(); // Refresh EMI list
                this.loadLoanDetails(); // Refresh loan details
                this.loadPaymentHistory(); // Refresh payment history
                this.paymentSuccess.emit();
            },
            error: (error) => {
                this.errorMessage.set(error.error?.message || 'Payment failed. Please try again.');
                this.isProcessingPayment.set(false);
            }
        });
    }

    onClose(): void {
        this.close.emit();
    }

    getTotalDue(emi: DueEmiResponse): number {
        return emi.remainingEmiAmount + emi.penaltyAmount;
    }
}
