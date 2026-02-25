import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { ApplicationService } from '../../../services/application.service';
import { LoanApplicationSummary, LoanApplicationStatus } from '../../../models/application.models';
import { ApplicationCardComponent } from './application-card/application-card.component';
import { ApplicationDetailsCardComponent } from './application-details-card/application-details-card.component';
import { createPaginationParams } from '../../../models/pagination.models';

@Component({
    selector: 'app-applications-list',
    imports: [CommonModule, ApplicationCardComponent],
    templateUrl: './applications-list.component.html',
    styleUrl: './applications-list.component.css'
})
export class ApplicationsListComponent implements OnInit {
    applications = signal<LoanApplicationSummary[]>([]);
    selectedStatus = signal<LoanApplicationStatus>(LoanApplicationStatus.Pending);
    isLoading = signal(false);
    errorMessage = signal('');

    private destroyRef = inject(DestroyRef);

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

        const pagination = createPaginationParams(1, 100);

        this.applicationService.getApplications(this.selectedStatus(), pagination)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
                next: (response) => {
                    this.applications.set(response.items);
                    this.isLoading.set(false);
                },
                error: () => {
                    this.errorMessage.set('Failed to load applications');
                    this.applications.set([]); 
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
