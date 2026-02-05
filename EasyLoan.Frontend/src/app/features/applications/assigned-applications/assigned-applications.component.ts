import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, RouterModule } from '@angular/router';
import { ApplicationService } from '../../../shared/services/application.service';
import { LoanApplicationSummary, LoanApplicationStatus } from '../../../shared/models/application.models';
import { AuthService } from '../../auth/auth.service';

@Component({
    selector: 'app-assigned-applications',
    standalone: true,
    imports: [
        CommonModule,
        MatTabsModule,
        MatTableModule,
        MatPaginatorModule,
        MatButtonModule,
        MatIconModule,
        MatTooltipModule,
        RouterModule,
        DatePipe,
        CurrencyPipe
    ],
    templateUrl: './assigned-applications.component.html',
    styleUrl: './assigned-applications.component.css'
})
export class AssignedApplicationsComponent implements OnInit {
    displayedColumns: string[] = ['applicationNumber', 'loanType', 'amount', 'appliedOn', 'status', 'actions'];

    // Data Sources for each tab
    pendingDataSource = new MatTableDataSource<LoanApplicationSummary>([]);
    approvedDataSource = new MatTableDataSource<LoanApplicationSummary>([]);
    rejectedDataSource = new MatTableDataSource<LoanApplicationSummary>([]);

    // Paginators for each tab
    @ViewChild('pendingPaginator') pendingPaginator!: MatPaginator;
    @ViewChild('approvedPaginator') approvedPaginator!: MatPaginator;
    @ViewChild('rejectedPaginator') rejectedPaginator!: MatPaginator;

    userRole: string = '';

    constructor(
        private appService: ApplicationService,
        private authService: AuthService,
        private router: Router
    ) { }

    ngOnInit(): void {
        const user = this.authService.getCurrentUser();
        this.userRole = user?.role || '';
        this.loadApplications();
    }

    loadApplications() {
        // Load Pending
        this.appService.getApplications(LoanApplicationStatus.Pending).subscribe(data => {
            this.pendingDataSource.data = data;
            this.pendingDataSource.paginator = this.pendingPaginator;
        });

        // Load Approved
        this.appService.getApplications(LoanApplicationStatus.Approved).subscribe(data => {
            this.approvedDataSource.data = data;
            this.approvedDataSource.paginator = this.approvedPaginator;
        });

        // Load Rejected
        this.appService.getApplications(LoanApplicationStatus.Rejected).subscribe(data => {
            this.rejectedDataSource.data = data;
            this.rejectedDataSource.paginator = this.rejectedPaginator;
        });
    }

    reviewApplication(applicationNumber: string) {
        const user = this.authService.getCurrentUser();
        const basePath = user?.role === 'Admin' ? '/admin' : '/employee';
        this.router.navigate([basePath, 'dashboard', 'assigned-applications', applicationNumber, 'review']);
    }
}
