import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { ApplicationService } from '../../../services/application.service';
import { LoanApplicationSummary, LoanApplicationStatus } from '../../../models/application.models';
import { ApplicationCardComponent } from './application-card/application-card.component';
import { ApplicationDetailsCardComponent } from './application-details-card/application-details-card.component';
import { createPaginationParams } from '../../../models/pagination.models';

@Component({
    selector: 'app-applications-list',
    standalone: true,
    imports: [CommonModule, ApplicationCardComponent],
    templateUrl: './applications-list.component.html',
    styleUrl: './applications-list.component.css'
})
export class ApplicationsListComponent implements OnInit {
    applications = signal<LoanApplicationSummary[]>([]);
    selectedStatus = signal<LoanApplicationStatus>(LoanApplicationStatus.Pending);
    isLoading = signal(false);
    errorMessage = signal('');

    LoanApplicationStatus = LoanApplicationStatus; // Expose enum to template

    constructor(
        private applicationService: ApplicationService,
        private dialog: MatDialog
    ) { }

    ngOnInit(): void {
        this.loadApplications();
    }

    loadApplications(): void {
        this.isLoading.set(true);
        this.errorMessage.set('');

        // Fetch all applications for the selected status (use max allowed page size)
        const pagination = createPaginationParams(1, 100);

        this.applicationService.getApplications(this.selectedStatus(), pagination).subscribe({
            next: (response) => {
                // Extract items from paginated response
                this.applications.set(response.items);
                this.isLoading.set(false);
            },
            error: (error) => {
                this.errorMessage.set(error.message || 'Failed to load applications');
                this.applications.set([]); // Reset to empty array on error
                this.isLoading.set(false);
            }
        });
    }

    onStatusChange(status: LoanApplicationStatus): void {
        this.selectedStatus.set(status);
        this.loadApplications();
    }

    onViewDetails(applicationNumber: string): void {
        this.dialog.open(ApplicationDetailsCardComponent, {
            data: { applicationNumber },
            width: '700px',
            maxWidth: '90vw'
        });
    }
}
