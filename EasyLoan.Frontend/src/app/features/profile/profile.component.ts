import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ProfileService, UserProfile, UpdateUserProfile } from '../../services/profile.service';
import { UserService } from '../../services/user.service';
import { CustomerProfile } from '../../models/customer-profile.models';
import { EmployeeProfile } from '../../models/employee-profile.models';

@Component({
    selector: 'app-profile',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './profile.component.html',
    styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
    profile = signal<UserProfile | null>(null);
    isLoading = signal(false);
    isEditing = signal(false);
    isSaving = signal(false);
    errorMessage = signal('');
    successMessage = signal('');
    profileForm!: FormGroup;
    userRole = signal<string>('');
    isCustomer = signal(false);
    isEmployee = signal(false);

    private readonly MAX_ANNUAL_SALARY = 1000000000000000; // 1000 trillion rupees

    constructor(
        private profileService: ProfileService,
        private userService: UserService,
        private fb: FormBuilder
    ) {
        const user = this.userService.currentUser();
        if (user) {
            this.userRole.set(user.role);
            this.isCustomer.set(user.role === 'Customer');
            this.isEmployee.set(user.role === 'Manager' || user.role === 'Admin');
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
                annualSalary: ['', [Validators.required, Validators.min(0), Validators.max(this.MAX_ANNUAL_SALARY), this.decimalPlacesValidator(2)]]
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

        this.profileService.getProfile().subscribe({
            next: (data: UserProfile) => {
                this.profile.set(data);
                if (this.isCustomer()) {
                    const customerData = data as CustomerProfile;
                    this.profileForm.patchValue({
                        name: customerData.name,
                        phoneNumber: customerData.phoneNumber,
                        annualSalary: customerData.annualSalary
                    });
                } else {
                    const employeeData = data as EmployeeProfile;
                    this.profileForm.patchValue({
                        name: employeeData.name,
                        phoneNumber: employeeData.phoneNumber
                    });
                }
                this.isLoading.set(false);
            },
            error: (error: Error) => {
                this.errorMessage.set(error.message || 'Failed to load profile');
                this.isLoading.set(false);
            }
        });
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

        this.profileService.updateProfile(updateData).subscribe({
            next: (data: UserProfile) => {
                this.profile.set(data);
                this.isEditing.set(false);
                this.isSaving.set(false);
                this.successMessage.set('Profile updated successfully!');
                setTimeout(() => this.successMessage.set(''), 3000);
            },
            error: (error: Error) => {
                this.isSaving.set(false);
                this.errorMessage.set(error.message || 'Failed to update profile');
            }
        });
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

    // Prevent invalid characters for phone number and limit to 10 digits
    onPhoneNumberKeyDown(event: KeyboardEvent): void {
        const input = event.target as HTMLInputElement;
        const currentValue = input.value;

        // Allow control keys (backspace, delete, tab, arrows, etc.)
        const allowedKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];
        if (allowedKeys.includes(event.key)) {
            return;
        }

        // Allow Ctrl/Cmd combinations (copy, paste, select all, etc.)
        if (event.ctrlKey || event.metaKey) {
            return;
        }

        // Only allow numeric keys (0-9)
        if (event.key < '0' || event.key > '9') {
            event.preventDefault();
            return;
        }

        // Prevent input if already at 10 digits
        if (currentValue.length >= 10) {
            const selectionStart = input.selectionStart || 0;
            const selectionEnd = input.selectionEnd || 0;
            // Allow if there's a selection (user is replacing text)
            if (selectionStart === selectionEnd) {
                event.preventDefault();
            }
        }
    }

    // Prevent 'e', '+', '-' for annual salary and limit input to 16 digits and 2 decimal places
    onSalaryKeyDown(event: KeyboardEvent): void {
        const input = event.target as HTMLInputElement;
        const currentValue = input.value;

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

        // Prevent multiple decimal points
        if (event.key === '.' && currentValue.includes('.')) {
            event.preventDefault();
            return;
        }

        // Limit to 2 decimal places if value contains a decimal point
        if (currentValue.includes('.')) {
            const selectionStart = input.selectionStart || 0;
            const selectionEnd = input.selectionEnd || 0;
            const decimalIndex = currentValue.indexOf('.');
            const decimalPart = currentValue.substring(decimalIndex + 1);

            // Only check for numeric keys (0-9)
            const isNumericKey = event.key >= '0' && event.key <= '9';

            // If cursor is after decimal point, we already have 2 decimal places, and user is typing a number
            if (isNumericKey && selectionStart > decimalIndex && decimalPart.length >= 2 && selectionStart === selectionEnd) {
                event.preventDefault();
                return;
            }
        }

        // Prevent input if already at 16 characters (1000 trillion = 1,000,000,000,000,000)
        // Don't count the decimal point in the character limit
        const valueWithoutDecimal = currentValue.replace('.', '');
        if (valueWithoutDecimal.length >= 16 && event.key >= '0' && event.key <= '9') {
            const selectionStart = input.selectionStart || 0;
            const selectionEnd = input.selectionEnd || 0;
            // Allow if there's a selection (user is replacing text)
            if (selectionStart === selectionEnd) {
                event.preventDefault();
            }
        }
    }

    // Custom validator for decimal places
    decimalPlacesValidator(maxDecimalPlaces: number) {
        return (control: AbstractControl): ValidationErrors | null => {
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
}
