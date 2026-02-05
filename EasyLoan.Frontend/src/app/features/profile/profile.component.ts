import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CustomerService } from '../../shared/services/customer.service';
import { EmployeeService } from '../../shared/services/employee.service';
import { TokenService } from '../../shared/services/token.service';
import { CustomerProfile } from '../../shared/models/customer.models';
import { EmployeeProfile } from '../../shared/models/employee.models';

@Component({
    selector: 'app-profile',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './profile.component.html',
    styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
    profile = signal<CustomerProfile | EmployeeProfile | null>(null);
    isLoading = signal(false);
    isEditing = signal(false);
    isSaving = signal(false);
    errorMessage = signal('');
    successMessage = signal('');
    profileForm!: FormGroup;
    userRole = signal<string>('');
    isCustomer = signal(false);
    isEmployee = signal(false);

    constructor(
        private customerService: CustomerService,
        private employeeService: EmployeeService,
        private tokenService: TokenService,
        private fb: FormBuilder
    ) {
        const user = this.tokenService.getCurrentUser();
        console.log('Profile Component - Current User:', user);
        if (user) {
            this.userRole.set(user.role);
            console.log('Profile Component - User Role:', user.role);

            this.isCustomer.set(user.role === 'Customer');
            this.isEmployee.set(user.role === 'Manager' || user.role === 'Admin');

            console.log('Profile Component - isCustomer:', this.isCustomer());
            console.log('Profile Component - isEmployee:', this.isEmployee());
        }
        this.initializeForm();
    }

    ngOnInit(): void {
        this.loadProfile();
    }

    initializeForm(): void {
        if (this.isCustomer()) {
            this.profileForm = this.fb.group({
                name: ['', [Validators.required, Validators.maxLength(100)]],
                phoneNumber: ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]],
                annualSalary: ['', [Validators.required, Validators.min(0)]]
            });
        } else {
            this.profileForm = this.fb.group({
                name: ['', [Validators.required, Validators.maxLength(100)]],
                phoneNumber: ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]]
            });
        }
    }

    loadProfile(): void {
        this.isLoading.set(true);
        this.errorMessage.set('');

        if (this.isCustomer()) {
            this.customerService.getProfile().subscribe({
                next: (data: CustomerProfile) => {
                    this.profile.set(data);
                    this.profileForm.patchValue({
                        name: data.name,
                        phoneNumber: data.phoneNumber,
                        annualSalary: data.annualSalary
                    });
                    this.isLoading.set(false);
                },
                error: (error: Error) => {
                    this.errorMessage.set(error.message || 'Failed to load profile');
                    this.isLoading.set(false);
                }
            });
        } else {
            this.employeeService.getProfile().subscribe({
                next: (data: EmployeeProfile) => {
                    this.profile.set(data);
                    this.profileForm.patchValue({
                        name: data.name,
                        phoneNumber: data.phoneNumber
                    });
                    this.isLoading.set(false);
                },
                error: (error: Error) => {
                    this.errorMessage.set(error.message || 'Failed to load profile');
                    this.isLoading.set(false);
                }
            });
        }
    }

    onEdit(): void {
        this.isEditing.set(true);
        this.successMessage.set('');
        this.errorMessage.set('');
    }

    onCancel(): void {
        this.isEditing.set(false);
        const currentProfile = this.profile();
        if (currentProfile) {
            if (this.isCustomer()) {
                const customerData = currentProfile as CustomerProfile;
                this.profileForm.patchValue({
                    name: customerData.name,
                    phoneNumber: customerData.phoneNumber,
                    annualSalary: customerData.annualSalary
                });
            } else {
                const employeeData = currentProfile as EmployeeProfile;
                this.profileForm.patchValue({
                    name: employeeData.name,
                    phoneNumber: employeeData.phoneNumber
                });
            }
        }
        this.errorMessage.set('');
    }

    onSave(): void {
        if (this.profileForm.invalid) {
            this.errorMessage.set('Please fill in all required fields correctly');
            return;
        }

        this.isSaving.set(true);
        this.errorMessage.set('');
        this.successMessage.set('');

        const updateData = this.profileForm.value;

        if (this.isCustomer()) {
            this.customerService.updateProfile(updateData).subscribe({
                next: (data: CustomerProfile) => {
                    this.profile.set(data);
                    this.isEditing.set(false);
                    this.isSaving.set(false);
                    this.successMessage.set('Profile updated successfully!');
                    setTimeout(() => this.successMessage.set(''), 3000);
                },
                error: (error: Error) => {
                    this.errorMessage.set(error.error?.message || 'Failed to update profile');
                    this.isSaving.set(false);
                }
            });
        } else {
            this.employeeService.updateProfile(updateData).subscribe({
                next: (data: EmployeeProfile) => {
                    this.profile.set(data);
                    this.isEditing.set(false);
                    this.isSaving.set(false);
                    this.successMessage.set('Profile updated successfully!');
                    setTimeout(() => this.successMessage.set(''), 3000);
                },
                error: (error: Error) => {
                    this.errorMessage.set(error.error?.message || 'Failed to update profile');
                    this.isSaving.set(false);
                }
            });
        }
    }

    formatDate(dateString: string): string {
        return new Date(dateString).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }

    getEmployeeRole(): string {
        if (this.isEmployee() && this.profile()) {
            return (this.profile() as EmployeeProfile).role;
        }
        return '';
    }

    getCustomerDateOfBirth(): string {
        if (this.isCustomer() && this.profile()) {
            const customerProfile = this.profile() as CustomerProfile;
            return this.formatDate(customerProfile.dateOfBirth);
        }
        return '';
    }

    getCustomerAnnualSalary(): number {
        if (this.isCustomer() && this.profile()) {
            return (this.profile() as CustomerProfile).annualSalary;
        }
        return 0;
    }

    getCustomerPanNumber(): string {
        if (this.isCustomer() && this.profile()) {
            return (this.profile() as CustomerProfile).panNumber;
        }
        return '';
    }

    getCustomerCreditScore(): number {
        if (this.isCustomer() && this.profile()) {
            return (this.profile() as CustomerProfile).creditScore;
        }
        return 0;
    }
}
