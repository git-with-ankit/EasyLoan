import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CustomerService } from '../../../../shared/services/customer.service';
import { CustomerProfile } from '../../../../shared/models/customer.models';

@Component({
    selector: 'app-profile',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './profile.component.html',
    styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
    profile = signal<CustomerProfile | null>(null);
    isLoading = signal(false);
    isEditing = signal(false);
    isSaving = signal(false);
    errorMessage = signal('');
    successMessage = signal('');
    profileForm!: FormGroup;

    constructor(
        private customerService: CustomerService,
        private fb: FormBuilder
    ) {
        this.initializeForm();
    }

    ngOnInit(): void {
        this.loadProfile();
    }

    initializeForm(): void {
        this.profileForm = this.fb.group({
            name: ['', [Validators.required, Validators.maxLength(100)]],
            phoneNumber: ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]],
            annualSalary: ['', [Validators.required, Validators.min(0)]]
        });
    }

    loadProfile(): void {
        this.isLoading.set(true);
        this.errorMessage.set('');

        this.customerService.getProfile().subscribe({
            next: (data) => {
                this.profile.set(data);
                this.profileForm.patchValue({
                    name: data.name,
                    phoneNumber: data.phoneNumber,
                    annualSalary: data.annualSalary
                });
                this.isLoading.set(false);
            },
            error: (error) => {
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
            this.profileForm.patchValue({
                name: currentProfile.name,
                phoneNumber: currentProfile.phoneNumber,
                annualSalary: currentProfile.annualSalary
            });
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

        this.customerService.updateProfile(updateData).subscribe({
            next: (data) => {
                this.profile.set(data);
                this.isEditing.set(false);
                this.isSaving.set(false);
                this.successMessage.set('Profile updated successfully!');
                setTimeout(() => this.successMessage.set(''), 3000);
            },
            error: (error) => {
                this.errorMessage.set(error.error?.message || 'Failed to update profile');
                this.isSaving.set(false);
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
}
