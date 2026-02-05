import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoanApplicationSummary } from '../../application.models';

@Component({
    selector: 'app-application-card',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './application-card.component.html',
    styleUrl: './application-card.component.css'
})
export class ApplicationCardComponent {
    @Input({ required: true }) application!: LoanApplicationSummary;
    @Output() viewDetails = new EventEmitter<string>();

    onViewDetails(): void {
        this.viewDetails.emit(this.application.applicationNumber);
    }

    getStatusClass(): string {
        switch (this.application.status) {
            case 'Pending': return 'status-pending';
            case 'Approved': return 'status-approved';
            case 'Rejected': return 'status-rejected';
            default: return '';
        }
    }
}
