import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, RouterModule } from '@angular/router';
import { ApplicationService } from '../application.service';
import { LoanApplicationSummary, LoanApplicationStatus } from '../application.models';
import { AuthService } from '../../auth/auth.service';
import { createPaginationParams } from '../../../shared/models/pagination.models';

interface TabPaginationState {
    data: LoanApplicationSummary[];
    pageIndex: number;  // 0-indexed for MatPaginator
    pageSize: number;
    totalCount: number;
    isLoading: boolean;
}

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

    // Pagination state for each tab using signals
    pendingState = signal<TabPaginationState>({
        data: [],
        pageIndex: 0,
        pageSize: 10,
        totalCount: 0,
        isLoading: false
    });

    approvedState = signal<TabPaginationState>({
        data: [],
        pageIndex: 0,
        pageSize: 10,
        totalCount: 0,
        isLoading: false
    });

    rejectedState = signal<TabPaginationState>({
        data: [],
        pageIndex: 0,
        pageSize: 10,
        totalCount: 0,
        isLoading: false
    });

    userRole = '';

    constructor(
        private appService: ApplicationService,
        private authService: AuthService,
        private router: Router
    ) { }

    ngOnInit(): void {
        const user = this.authService.getCurrentUser();
        this.userRole = user?.role || '';
        this.loadAllTabs();
    }

    loadAllTabs(): void {
        this.loadApplications(LoanApplicationStatus.Pending, this.pendingState);
        this.loadApplications(LoanApplicationStatus.Approved, this.approvedState);
        this.loadApplications(LoanApplicationStatus.Rejected, this.rejectedState);
    }

    loadApplications(status: LoanApplicationStatus, stateSignal: ReturnType<typeof signal<TabPaginationState>>): void {
        const currentState = stateSignal();
        stateSignal.set({ ...currentState, isLoading: true });

        const pagination = createPaginationParams(currentState.pageIndex + 1, currentState.pageSize); // Convert to 1-indexed

        this.appService.getApplications(status, pagination).subscribe({
            next: (response) => {
                stateSignal.set({
                    data: Array.isArray(response.items) ? response.items : [],
                    totalCount: response.totalCount || 0,
                    pageIndex: currentState.pageIndex,
                    pageSize: currentState.pageSize,
                    isLoading: false
                });
            },
            error: (err) => {
                console.error('Error loading applications:', err);
                stateSignal.set({
                    data: [],
                    totalCount: 0,
                    pageIndex: currentState.pageIndex,
                    pageSize: currentState.pageSize,
                    isLoading: false
                });
            }
        });
    }

    onPendingPageChange(event: PageEvent): void {
        const currentState = this.pendingState();
        this.pendingState.set({
            ...currentState,
            pageIndex: event.pageIndex,
            pageSize: event.pageSize
        });
        this.loadApplications(LoanApplicationStatus.Pending, this.pendingState);
    }

    onApprovedPageChange(event: PageEvent): void {
        const currentState = this.approvedState();
        this.approvedState.set({
            ...currentState,
            pageIndex: event.pageIndex,
            pageSize: event.pageSize
        });
        this.loadApplications(LoanApplicationStatus.Approved, this.approvedState);
    }

    onRejectedPageChange(event: PageEvent): void {
        const currentState = this.rejectedState();
        this.rejectedState.set({
            ...currentState,
            pageIndex: event.pageIndex,
            pageSize: event.pageSize
        });
        this.loadApplications(LoanApplicationStatus.Rejected, this.rejectedState);
    }

    reviewApplication(applicationNumber: string): void {
        const user = this.authService.getCurrentUser();
        const basePath = user?.role === 'Admin' ? '/admin' : '/employee';
        this.router.navigate([basePath, 'assigned-applications', applicationNumber, 'review']);
    }

    // TrackBy function for better performance
    trackByApplicationNumber(index: number, item: LoanApplicationSummary): string {
        return item.applicationNumber;
    }
}
