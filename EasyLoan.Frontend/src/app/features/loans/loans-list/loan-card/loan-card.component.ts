import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoanSummary } from '../../../../shared/models/loan.models';

@Component({
    selector: 'app-loan-card',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './loan-card.component.html',
    styleUrl: './loan-card.component.css'
})
export class LoanCardComponent {
    @Input({ required: true }) loan!: LoanSummary;
    @Output() viewDetails = new EventEmitter<string>();

    onViewDetails(): void {
        this.viewDetails.emit(this.loan.loanNumber);
    }

    getStatusClass(): string {
        switch (this.loan.status) {
            case 'Active': return 'status-active';
            case 'Closed': return 'status-closed';
            default: return '';
        }
    }
}
