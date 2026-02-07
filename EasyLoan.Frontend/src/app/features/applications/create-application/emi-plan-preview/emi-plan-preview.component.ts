import { Component, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { EmiScheduleItem, LoanType } from '../../../../models/loan-type.models';

@Component({
    selector: 'app-emi-plan-preview',
    standalone: true,
    imports: [CommonModule, MatPaginatorModule],
    templateUrl: './emi-plan-preview.component.html',
    styleUrl: './emi-plan-preview.component.css'
})
export class EmiPlanPreviewComponent {

    @Input({ required: true }) emiPlan!: EmiScheduleItem[];
    @Input({ required: true }) loanType!: LoanType;
    @Input({ required: true }) requestedAmount!: number;
    @Input({ required: true }) tenure!: number;

    @ViewChild(MatPaginator) paginator!: MatPaginator;

    paginatedEmiPlan: EmiScheduleItem[] = [];
    pageSize = 10;
    pageSizeOptions = [5, 10, 25, 50];
    pageIndex = 0; // 0-indexed for MatPaginator

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['emiPlan']) {
            this.pageIndex = 0; // Reset to first page on new plan
            if (this.paginator) {
                this.paginator.firstPage();
            }
            this.updatePaginatedPlan();
        }
    }

    onPageChange(event: PageEvent): void {
        this.pageIndex = event.pageIndex;
        this.pageSize = event.pageSize;
        this.updatePaginatedPlan();
    }

    private updatePaginatedPlan(): void {
        if (!this.emiPlan) {
            this.paginatedEmiPlan = [];
            return;
        }
        const startIndex = this.pageIndex * this.pageSize;
        const endIndex = startIndex + this.pageSize;
        this.paginatedEmiPlan = this.emiPlan.slice(startIndex, endIndex);
    }

    get monthlyEmi(): number {
        return this.emiPlan.length > 0 ? this.emiPlan[0].totalEmiAmount : 0;
    }

    get totalPrincipal(): number {
        return this.emiPlan.reduce((sum, emi) => sum + emi.principalComponent, 0);
    }

    get totalInterest(): number {
        return this.emiPlan.reduce((sum, emi) => sum + emi.interestComponent, 0);
    }

    get totalAmount(): number {
        return this.totalPrincipal + this.totalInterest;
    }
}
