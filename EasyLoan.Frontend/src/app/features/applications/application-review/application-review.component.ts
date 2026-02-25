import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ApplicationService } from '../../../services/application.service';
import { LoanApplicationDetailsWithCustomerData, ReviewLoanApplicationRequest } from '../../../models/review.models';
import { LoanApplicationStatus } from '../../../models/application.models';
import { AuthService } from '../../../services/auth.service';
import { finalize } from 'rxjs';

@Component({
    selector: 'app-application-review',
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatCardModule,
        MatButtonModule,
        MatInputModule,
        MatSelectModule,
        MatSnackBarModule,
        RouterModule
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

    private destroyRef = inject(DestroyRef);

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
            remarks: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(1000)]]
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

    loadDetails(): void {
        this.isLoading.set(true);

        this.appService.getApplicationDetailsForReview(this.applicationNumber)
            .pipe(
                finalize(() => this.isLoading.set(false)),
                takeUntilDestroyed(this.destroyRef)
            )
            .subscribe({
                next: (data) => {

                    this.details.set(data);
                    this.isPending.set(data.status === LoanApplicationStatus.Pending);

                    this.reviewForm.patchValue({
                        approvedAmount: data.requestedAmount
                    });

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

                    if (!this.isPending()) {
                        this.reviewForm.disable();
                    }

                },
                error: () => {
                    this.snackBar.open('Failed to load application details ', 'Close', { duration: 5000 });
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

        this.appService.submitReview(this.applicationNumber, request)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
                next: () => {
                    this.isLoading.set(false);
                    this.snackBar.open('Review submitted successfully', 'Close', { duration: 3000 });

                    // Navigate based on user role
                    const user = this.authService.getCurrentUser();
                    const basePath = user?.role === 'Admin' ? '/admin' : '/employee';
                    this.router.navigate([basePath, 'assigned-applications']);
                },
                error: (error) => {
                    this.isLoading.set(false);
                    this.snackBar.open('Failed to submit review ', 'Close', { duration: 5000 });
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

    // Prevent 'e', '+', '-' for approved amount field
    onApprovedAmountKeyDown(event: KeyboardEvent): void {
        // Prevent 'e', 'E', '+', '-'
        if (['e', 'E', '+', '-'].includes(event.key)) {
            event.preventDefault();
            return;
        }

        // Allow control keys (backspace, delete, tab, arrows, etc.)
        const allowedKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];
        if (allowedKeys.includes(event.key)) {
            return;
        }

        // Allow Ctrl/Cmd combinations (copy, paste, select all, etc.)
        if (event.ctrlKey || event.metaKey) {
            return;
        }
    }

    goBack() {
        const basePath = this.userRole === 'Admin' ? '/admin' : '/employee';
        this.router.navigate([basePath, 'assigned-applications']);
    }
}
