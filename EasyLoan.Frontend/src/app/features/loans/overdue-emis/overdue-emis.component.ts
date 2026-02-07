import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { LoanService } from '../../../services/loan.service';
import { LoanDetailsCardComponent } from '../loans-list/loan-details-card/loan-details-card.component';
import { EmiDueStatus, LoanEmiGroup } from '../../../models/loan.models';

@Component({
    selector: 'app-overdue-emis',
    standalone: true,
    imports: [CommonModule],
    templateUrl: 'overdue-emis.component.html',
    styleUrl: './overdue-emis.component.css'
})
export class OverdueEmis implements OnInit {
    emisByLoan = signal<LoanEmiGroup[]>([]);
    isLoading = signal(false);
    errorMessage = signal('');

    constructor(
        private loanService: LoanService,
        private dialog: MatDialog
    ) { }

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
        const dialogRef = this.dialog.open(LoanDetailsCardComponent, {
            data: { loanNumber },
            width: '900px',
            maxWidth: '95vw',
            maxHeight: '90vh'
        });

        dialogRef.afterClosed().subscribe(() => {
            this.loadDueEmis(); // Refresh EMI list after dialog closes (in case payment was made)
        });
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
