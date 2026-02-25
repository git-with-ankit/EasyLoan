import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { LoanService } from '../../../services/loan.service';
import { LoanSummary, LoanStatus } from '../../../models/loan.models';
import { LoanCardComponent } from './loan-card/loan-card.component';
import { LoanDetailsCardComponent } from './loan-details-card/loan-details-card.component';

@Component({
    selector: 'app-loans-list',
    standalone: true,
    imports: [CommonModule, LoanCardComponent],
    templateUrl: './loans-list.component.html',
    styleUrl: './loans-list.component.css'
})
export class LoansListComponent implements OnInit {
    loans = signal<LoanSummary[]>([]);
    selectedStatus = signal<LoanStatus>(LoanStatus.Active);
    isLoading = signal(false);
    errorMessage = signal('');

    private destroyRef = inject(DestroyRef);

    LoanStatus = LoanStatus;

    constructor(
        private loanService: LoanService,
        private dialog: MatDialog
    ) { }

    ngOnInit(): void {
        this.loadLoans();
    }

    loadLoans(): void {
        this.isLoading.set(true);
        this.errorMessage.set('');

        this.loanService.getLoans(this.selectedStatus())
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
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
        const dialogRef = this.dialog.open(LoanDetailsCardComponent, {
            data: { loanNumber },
            width: '900px',
            maxWidth: '95vw',
            maxHeight: '90vh'
        });

        dialogRef.afterClosed()
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(() => {
                this.loadLoans(); // Refresh loans after dialog closes (in case payment was made)
            });
    }
}
