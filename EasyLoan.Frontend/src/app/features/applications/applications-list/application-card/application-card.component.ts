import { Component, Input, Output, EventEmitter, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoanApplicationSummary } from '../../../../models/application.models';

@Component({
    selector: 'app-application-card',
    imports: [CommonModule],
    templateUrl: './application-card.component.html',
    styleUrl: './application-card.component.css'
})
export class ApplicationCardComponent {
    // application = input.required<LoanApplicationSummary>();
    // viewDetails = output<string>();
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
