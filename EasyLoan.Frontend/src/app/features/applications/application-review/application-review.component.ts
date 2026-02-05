import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ApplicationService } from '../application.service';
import { LoanApplicationDetailsWithCustomerData, ReviewLoanApplicationRequest } from '../review.models';
import { LoanApplicationStatus } from '../application.models';
import { AuthService } from '../../auth/auth.service';

@Component({
    selector: 'app-application-review',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatCardModule,
        MatButtonModule,
        MatInputModule,
        MatSelectModule,
        MatSnackBarModule,
        RouterModule,
        CurrencyPipe,
        DatePipe
    ],
    templateUrl: './application-review.component.html',
    styleUrl: './application-review.component.css'
})
export class ApplicationReviewComponent implements OnInit {
    applicationNumber: string = '';
    details = signal<LoanApplicationDetailsWithCustomerData | null>(null);
    reviewForm: FormGroup;
    isLoading = signal(false);
    isPending = signal(false);
    userRole: string = '';
    isManager = false;

    statuses = [
        { value: LoanApplicationStatus.Approved, label: 'Approve' },
        { value: LoanApplicationStatus.Rejected, label: 'Reject' }
    ];

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private appService: ApplicationService,
        private fb: FormBuilder,
        private snackBar: MatSnackBar,
        private authService: AuthService
    ) {
        this.reviewForm = this.fb.group({
            status: ['', Validators.required],
            approvedAmount: ['', [Validators.required, Validators.min(1)]],
            remarks: ['', [Validators.required, Validators.minLength(5)]]
        });
    }

    ngOnInit(): void {
        const user = this.authService.getCurrentUser();
        this.userRole = user?.role || '';
        this.isManager = this.userRole === 'Manager';

        this.applicationNumber = this.route.snapshot.paramMap.get('applicationNumber') || '';
        if (this.applicationNumber) {
            this.loadDetails();
        }
    }

    loadDetails() {
        console.log('=== loadDetails called ===');
        console.log('Application number:', this.applicationNumber);
        this.isLoading.set(true);
        console.log('isLoading set to:', this.isLoading());

        this.appService.getApplicationDetailsForReview(this.applicationNumber).subscribe({
            next: (data) => {
                console.log('=== Data received ===');
                console.log('Application details loaded:', data);

                this.details.set(data);
                console.log('this.details set to:', this.details());

                this.isLoading.set(false);
                console.log('isLoading set to:', this.isLoading());

                this.isPending.set(data.status === LoanApplicationStatus.Pending);
                console.log('isPending:', this.isPending());

                // Set approved amount to requested amount by default
                this.reviewForm.patchValue({
                    approvedAmount: data.requestedAmount
                });

                // Add max validator for approved amount
                const approvedAmountControl = this.reviewForm.get('approvedAmount');
                if (approvedAmountControl) {
                    approvedAmountControl.setValidators([
                        Validators.required,
                        Validators.min(1),
                        Validators.max(data.requestedAmount),
                        this.decimalPlacesValidator(2)
                    ]);
                    approvedAmountControl.updateValueAndValidity();
                }

                // Disable form if not pending
                if (!this.isPending()) {
                    this.reviewForm.disable();
                }

                console.log('=== loadDetails complete ===');
            },
            error: (error) => {
                console.error('Error loading application details:', error);
                this.isLoading.set(false);
                this.snackBar.open('Failed to load application details: ' + (error.error?.message || error.message), 'Close', { duration: 5000 });
            }
        });
    }

    submitReview() {
        if (this.reviewForm.invalid) return;

        this.isLoading.set(true);
        const formValue = this.reviewForm.value;

        // Transform the request to match backend DTO expectations
        const request: ReviewLoanApplicationRequest = {
            isApproved: formValue.status === LoanApplicationStatus.Approved,
            approvedAmount: formValue.approvedAmount,
            managerComments: formValue.remarks
        };

        this.appService.submitReview(this.applicationNumber, request).subscribe({
            next: () => {
                this.isLoading.set(false);
                this.snackBar.open('Review submitted successfully', 'Close', { duration: 3000 });

                // Navigate based on user role
                const user = this.authService.getCurrentUser();
                const basePath = user?.role === 'Admin' ? '/admin' : '/employee';
                this.router.navigate([basePath, 'dashboard', 'assigned-applications']);
            },
            error: (error) => {
                console.error('Error submitting review:', error);
                this.isLoading.set(false);
                this.snackBar.open('Failed to submit review: ' + (error.error?.message || error.message), 'Close', { duration: 5000 });
            }
        });
    }

    // Custom validator for decimal places
    decimalPlacesValidator(maxDecimalPlaces: number) {
        return (control: AbstractControl) => {
            if (control.value === null || control.value === undefined || control.value === '') {
                return null;
            }

            const value = control.value.toString();
            const decimalIndex = value.indexOf('.');

            if (decimalIndex === -1) {
                return null; // No decimal point, valid
            }

            const decimalPlaces = value.length - decimalIndex - 1;

            if (decimalPlaces > maxDecimalPlaces) {
                return { decimalPlaces: { max: maxDecimalPlaces, actual: decimalPlaces } };
            }

            return null;
        };
    }

    goBack() {
        const basePath = this.userRole === 'Admin' ? '/admin' : '/employee';
        this.router.navigate([basePath, 'assigned-applications']);
    }
}
