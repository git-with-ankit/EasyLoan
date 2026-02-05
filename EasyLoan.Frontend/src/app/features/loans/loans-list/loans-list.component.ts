import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoanService } from '../loan.service';
import { LoanSummary, LoanStatus } from '../loan.models';
import { LoanCardComponent } from './loan-card/loan-card.component';
import { LoanDetailsCardComponent } from './loan-details-card/loan-details-card.component';

@Component({
    selector: 'app-loans-list',
    standalone: true,
    imports: [CommonModule, LoanCardComponent, LoanDetailsCardComponent],
    templateUrl: './loans-list.component.html',
    styleUrl: './loans-list.component.css'
})
export class LoansListComponent implements OnInit {
    loans = signal<LoanSummary[]>([]);
    selectedStatus = signal<LoanStatus>(LoanStatus.Active);
    selectedLoanNumber = signal<string | null>(null);
    isLoading = signal(false);
    errorMessage = signal('');

    LoanStatus = LoanStatus; 

    constructor(private loanService: LoanService) { }

    ngOnInit(): void {
        this.loadLoans();
    }

    loadLoans(): void {
        this.isLoading.set(true);
        this.errorMessage.set('');

        this.loanService.getLoans(this.selectedStatus()).subscribe({
            next: (data) => {
                this.loans.set(data);
                this.isLoading.set(false);
            },
            error: (error) => {
                this.errorMessage.set(error.message || 'Failed to load loans');
                this.isLoading.set(false);
            }
        });
    }

    onStatusChange(status: LoanStatus): void {
        this.selectedStatus.set(status);
        this.loadLoans();
    }

    onViewDetails(loanNumber: string): void {
        this.selectedLoanNumber.set(loanNumber);
    }

    onCloseDetails(): void {
        this.selectedLoanNumber.set(null);
    }

    onPaymentSuccess(): void {
        this.loadLoans(); // Refresh loans after payment
    }
}
