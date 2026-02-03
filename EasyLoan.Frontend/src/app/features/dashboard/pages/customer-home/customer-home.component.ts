import { Component, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { LoanService } from '../../../../shared/services/loan.service';
import { ApplicationService } from '../../../../shared/services/application.service';
import { AuthService } from '../../../auth/auth.service';
import { LoanStatus, EmiDueStatus } from '../../../../shared/models/loan.models';
import { LoanApplicationStatus } from '../../../../shared/models/application.models';

@Component({
    selector: 'app-customer-home',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        RouterModule,
        CurrencyPipe,
        DatePipe
    ],
    templateUrl: './customer-home.component.html',
    styleUrl: './customer-home.component.css'
})
export class CustomerHomeComponent implements OnInit {
    activeLoansCount = 0;
    totalPrincipalRemaining = 0;
    pendingApplicationsCount = 0;
    nextEmiAmount = 0;
    nextEmiDate: Date | null = null;

    constructor(
        private loanService: LoanService,
        private applicationService: ApplicationService,
        private auth: AuthService
    ) { }

    ngOnInit(): void {
        this.loadDashboardData();
    }

    private loadDashboardData(): void {
        // 1. Active Loans
        this.loanService.getLoans(LoanStatus.Active).subscribe({
            next: (loans) => {
                this.activeLoansCount = loans.length;
                this.totalPrincipalRemaining = loans.reduce((sum, loan) => sum + loan.principalRemaining, 0);
            },
            error: () => console.error('Failed to load active loans')
        });

        // 2. Pending Applications
        this.applicationService.getApplications(LoanApplicationStatus.Pending).subscribe({
            next: (apps) => {
                this.pendingApplicationsCount = apps.length;
            },
            error: () => console.error('Failed to load pending applications')
        });

        // 3. Next EMI (Upcoming)
        this.loanService.getAllDueEmis(EmiDueStatus.Upcoming).subscribe({
            next: (loanGroups) => {
                // Flatten all upcoming EMIs across all loans
                const allUpcomingEmis = loanGroups.flatMap(g => g.emis);

                if (allUpcomingEmis.length > 0) {
                    // Sort by date to find the absolute next one
                    allUpcomingEmis.sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime());

                    const nextEmi = allUpcomingEmis[0];
                    this.nextEmiAmount = nextEmi.emiAmount;
                    this.nextEmiDate = new Date(nextEmi.dueDate);
                }
            },
            error: () => console.error('Failed to load upcoming EMIs')
        });
    }
}
