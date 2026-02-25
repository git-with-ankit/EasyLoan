import { Component, signal, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'app-change-password',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule
    ],
    templateUrl: './change-password.component.html',
    styleUrl: './change-password.component.css'
})
export class ChangePasswordComponent {
    form: FormGroup;
    isLoading = signal<boolean>(false);
    errorMessage = signal<string>('');
    successMessage = signal<string>('');
    hideOldPassword = true;
    hideNewPassword = true;

    private destroyRef = inject(DestroyRef);

    private readonly PASSWORD_REGEX =
        /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$/;

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private dialogRef: MatDialogRef<ChangePasswordComponent>
    ) {
        this.form = this.fb.group({
            oldPassword: ['', [Validators.required, Validators.pattern(this.PASSWORD_REGEX)]],
            newPassword: ['', [Validators.required, Validators.pattern(this.PASSWORD_REGEX)]]
        });

        // Add custom validator to newPassword that checks against oldPassword
        const newPasswordControl = this.form.get('newPassword');
        newPasswordControl?.addValidators(this.samePasswordValidator());

        // Re-validate newPassword when oldPassword changes
        this.form.get('oldPassword')?.valueChanges
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(() => {
                newPasswordControl?.updateValueAndValidity();
            });
    }

    // Custom validator that checks if new password matches old password
    samePasswordValidator() {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.value) return null;

            const oldPassword = this.form?.get('oldPassword')?.value;
            if (!oldPassword) return null;

            return control.value === oldPassword ? { samePassword: true } : null;
        };
    }

    submit(): void {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        this.isLoading.set(true);
        this.errorMessage.set('');
        this.successMessage.set('');

        this.authService.changePassword(this.form.value)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
                next: () => {
                    this.isLoading.set(false);
                    this.successMessage.set('Password changed successfully!');
                    setTimeout(() => {
                        this.dialogRef.close(true);
                    }, 1500);
                },
                error: (error: Error) => {
                    this.isLoading.set(false);
                    this.errorMessage.set(error.message || 'Failed to change password.');
                }
            });
    }

    toggleOldPasswordVisibility(): void {
        this.hideOldPassword = !this.hideOldPassword;
    }

    toggleNewPasswordVisibility(): void {
        this.hideNewPassword = !this.hideNewPassword;
    }

    cancel(): void {
        this.dialogRef.close(false);
    }
}
