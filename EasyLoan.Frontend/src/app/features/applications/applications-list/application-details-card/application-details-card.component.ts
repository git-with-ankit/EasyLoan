import { Component, Inject, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { ApplicationService } from '../../../../services/application.service';
import { LoanApplicationDetails } from '../../../../models/application.models';

@Component({
    selector: 'app-application-details-card',
    standalone: true,
    imports: [
        CommonModule,
        MatDialogModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatChipsModule
    ],
    templateUrl: './application-details-card.component.html',
    styleUrl: './application-details-card.component.css'
})
export class ApplicationDetailsCardComponent implements OnInit {
    applicationDetails = signal<LoanApplicationDetails | null>(null);
    isLoading = signal(false);
    errorMessage = signal('');

    private destroyRef = inject(DestroyRef);
    public data = inject(MAT_DIALOG_DATA) as {applicationNumber : string}
    private applicationService = inject(ApplicationService)
    public dialogRef = inject(MatDialogRef<ApplicationDetailsCardComponent>)

    // constructor(
    //     private applicationService: ApplicationService,
    //     public dialogRef: MatDialogRef<ApplicationDetailsCardComponent>,
    //     @Inject(MAT_DIALOG_DATA) public data: { applicationNumber: string }
    // ) { }

    ngOnInit(): void {
        this.loadApplicationDetails();
    }

    loadApplicationDetails(): void {
        this.isLoading.set(true);
        this.applicationService.getApplicationDetails(this.data.applicationNumber)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
                next: (data) => {
                    this.applicationDetails.set(data);
                    this.isLoading.set(false);
                },
                error: (error) => {
                    this.errorMessage.set('Failed to load application details');
                    this.isLoading.set(false);
                }
            });
    }

    onClose(): void {
        this.dialogRef.close();
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

    getStatusColor(): 'primary' | 'accent' | 'warn' {
        const status = this.applicationDetails()?.status;
        switch (status) {
            case 'Approved': return 'accent';
            case 'Rejected': return 'warn';
            default: return 'primary';
        }
    }
}
