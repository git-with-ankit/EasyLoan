import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoanService } from '../../../../shared/services/loan.service';
import { LoanDetailsCardComponent } from '../../components/loan-details-card/loan-details-card.component';
import { EmiDueStatus, LoanEmiGroup } from '../../../../shared/models/loan.models';

@Component({
    selector: 'app-emi-payments',
    standalone: true,
    imports: [CommonModule, LoanDetailsCardComponent],
    templateUrl: './emi-payments.component.html',
    styleUrl: './emi-payments.component.css'
})
export class EmiPaymentsComponent implements OnInit {
    emisByLoan = signal<LoanEmiGroup[]>([]);
    isLoading = signal(false);
    errorMessage = signal('');
    selectedLoanNumber = signal<string | null>(null);

    constructor(private loanService: LoanService) { }

    ngOnInit(): void {
        this.loadDueEmis();
    }

    loadDueEmis(): void {
        this.isLoading.set(true);
        this.errorMessage.set('');

        this.loanService.getAllDueEmis(EmiDueStatus.Overdue).subscribe({
            next: (data) => {
                this.emisByLoan.set(data);
                this.isLoading.set(false);
            },
            error: (error) => {
                this.errorMessage.set(error.message || 'Failed to load EMI data');
                this.isLoading.set(false);
            }
        });
    }

    onLoanClick(loanNumber: string): void {
        this.selectedLoanNumber.set(loanNumber);
    }

    onCloseLoanDetails(): void {
        this.selectedLoanNumber.set(null);
    }

    onPaymentSuccess(): void {
        this.selectedLoanNumber.set(null);
        this.loadDueEmis(); // Refresh the EMI list
    }

    getLoanTotalDue(loanGroup: LoanEmiGroup): number {
        return loanGroup.emis.reduce((sum, emi) => sum + emi.remainingEmiAmount + emi.penaltyAmount, 0);
    }

    get totalDueEmis(): number {
        return this.emisByLoan().reduce((sum, group) => sum + group.emis.length, 0);
    }

    get totalAmountDue(): number {
        return this.emisByLoan().reduce((sum, group) => {
            return sum + group.emis.reduce((emiSum, emi) => emiSum + emi.remainingEmiAmount + emi.penaltyAmount, 0);
        }, 0);
    }
}
