import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApplicationService } from '../../application.service';
import { LoanApplicationDetails } from '../../application.models';

@Component({
    selector: 'app-application-details-card',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './application-details-card.component.html',
    styleUrl: './application-details-card.component.css'
})
export class ApplicationDetailsCardComponent implements OnInit {
    @Input({ required: true }) applicationNumber!: string;
    @Output() close = new EventEmitter<void>();

    applicationDetails = signal<LoanApplicationDetails | null>(null);
    isLoading = signal(false);
    errorMessage = signal('');

    constructor(private applicationService: ApplicationService) { }

    ngOnInit(): void {
        this.loadApplicationDetails();
    }

    loadApplicationDetails(): void {
        this.isLoading.set(true);
        this.applicationService.getApplicationDetails(this.applicationNumber).subscribe({
            next: (data) => {
                this.applicationDetails.set(data);
                this.isLoading.set(false);
            },
            error: (error) => {
                // this.errorMessage.set(error.message || 'Failed to load application details');
                this.isLoading.set(false);
            }
        });
    }

    onClose(): void {
        this.close.emit();
    }

    getStatusClass(): string {
        const status = this.applicationDetails()?.status;
        switch (status) {
            case 'Pending': return 'status-pending';
            case 'Approved': return 'status-approved';
            case 'Rejected': return 'status-rejected';
            default: return '';
        }
    }
}
