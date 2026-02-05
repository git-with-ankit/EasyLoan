import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApplicationService } from '../../../shared/services/application.service';
import { LoanApplicationSummary, LoanApplicationStatus } from '../../../shared/models/application.models';
import { ApplicationCardComponent } from './application-card/application-card.component';
import { ApplicationDetailsCardComponent } from './application-details-card/application-details-card.component';

@Component({
    selector: 'app-applications-list',
    standalone: true,
    imports: [CommonModule, ApplicationCardComponent, ApplicationDetailsCardComponent],
    templateUrl: './applications-list.component.html',
    styleUrl: './applications-list.component.css'
})
export class ApplicationsListComponent implements OnInit {
    applications = signal<LoanApplicationSummary[]>([]);
    selectedStatus = signal<LoanApplicationStatus>(LoanApplicationStatus.Pending);
    selectedApplicationNumber = signal<string | null>(null);
    isLoading = signal(false);
    errorMessage = signal('');

    LoanApplicationStatus = LoanApplicationStatus; // Expose enum to template

    constructor(private applicationService: ApplicationService) { }

    ngOnInit(): void {
        this.loadApplications();
    }

    loadApplications(): void {
        this.isLoading.set(true);
        this.errorMessage.set('');

        this.applicationService.getApplications(this.selectedStatus()).subscribe({
            next: (data) => {
                this.applications.set(data);
                this.isLoading.set(false);
            },
            error: (error) => {
                this.errorMessage.set(error.message || 'Failed to load applications');
                this.isLoading.set(false);
            }
        });
    }

    onStatusChange(status: LoanApplicationStatus): void {
        this.selectedStatus.set(status);
        this.loadApplications();
    }

    onViewDetails(applicationNumber: string): void {
        this.selectedApplicationNumber.set(applicationNumber);
    }

    onCloseDetails(): void {
        this.selectedApplicationNumber.set(null);
    }
}
