import { Component, OnInit, signal } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
  AbstractControl,
  ValidationErrors
} from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { AuthService } from '../auth.service';

type RegisterRole = 'Customer' | 'Manager';

@Component({
  standalone: true,
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
  imports: [
    ReactiveFormsModule,
    CommonModule,
    RouterLink,
    MatCardModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule
  ]
})
export class RegisterComponent implements OnInit {
  form!: FormGroup;
  isLoading = signal<boolean>(false);
  errorMessage = '';
  successMessage = '';
  hidePassword = true;
  hideConfirmPassword = true;
  maxDate: Date;
  minDate: Date;
  registrationType: RegisterRole = 'Customer';
  isManagerRegistration = false;

  private readonly PASSWORD_REGEX =
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$/;
  private readonly PHONE_REGEX = /^[6-9]\d{9}$/;
  private readonly PAN_REGEX = /^[A-Z]{5}[0-9]{4}[A-Z]$/;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    // Set date constraints (18-150 years old)
    const today = new Date();
    this.maxDate = new Date(today.getFullYear() - 18, today.getMonth(), today.getDate());
    this.minDate = new Date(today.getFullYear() - 150, today.getMonth(), today.getDate());
  }

  ngOnInit(): void {
    this.isManagerRegistration = this.router.url.includes('/admin/dashboard/create-manager');
    this.registrationType = this.isManagerRegistration ? 'Manager' : 'Customer';

    if (this.isManagerRegistration) {
      this.form = this.fb.group({
        role: ['Manager', Validators.required],
        name: ['', [Validators.required, Validators.maxLength(100)]],
        email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
        phoneNumber: ['', [Validators.required, Validators.pattern(this.PHONE_REGEX)]],
        password: ['', [Validators.required, Validators.pattern(this.PASSWORD_REGEX)]],
        confirmPassword: ['', Validators.required]
      }, {
        validators: this.passwordMatchValidator
      });
    } else {
      this.form = this.fb.group({
        role: ['Customer', Validators.required],
        name: ['', [Validators.required, Validators.maxLength(100)]],
        email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
        phoneNumber: ['', [Validators.required, Validators.pattern(this.PHONE_REGEX)]],
        dateOfBirth: ['', Validators.required],
        annualSalary: ['', [Validators.required, Validators.min(0)]],
        panNumber: ['', [Validators.required, Validators.pattern(this.PAN_REGEX)]],
        password: ['', [Validators.required, Validators.pattern(this.PASSWORD_REGEX)]],
        confirmPassword: ['', Validators.required]
      }, {
        validators: this.passwordMatchValidator
      });
    }
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (!password || !confirmPassword) {
      return null;
    }

    return password.value === confirmPassword.value ? null : { passwordMismatch: true };
  }

  getPasswordStrength(): { strength: string; color: string; width: string } {
    const password = this.form.get('password')?.value || '';

    if (password.length === 0) {
      return { strength: '', color: '', width: '0%' };
    }

    let strength = 0;
    if (password.length >= 8) strength++;
    if (/[a-z]/.test(password)) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/\d/.test(password)) strength++;
    if (/[@$!%*?&]/.test(password)) strength++;

    if (strength <= 2) {
      return { strength: 'Weak', color: '#f44336', width: '33%' };
    } else if (strength <= 4) {
      return { strength: 'Medium', color: '#ff9800', width: '66%' };
    } else {
      return { strength: 'Strong', color: '#4caf50', width: '100%' };
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage = '';
    this.successMessage = '';

    const formValue = this.form.value;

    if (this.isManagerRegistration) {
      // Manager registration
      const registerDto = {
        name: formValue.name,
        email: formValue.email,
        phoneNumber: formValue.phoneNumber,
        password: formValue.password
      };

      this.auth.registerManager(registerDto).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.successMessage = 'Manager created successfully!';
          setTimeout(() => {
            this.router.navigate(['/admin/dashboard/overview']);
          }, 2000);
        },
        error: (error: Error) => {
          this.isLoading.set(false);
          this.errorMessage = error.message || 'Manager creation failed. Please try again.';
        }
      });
    } else {
      // Customer registration
      const registerDto = {
        name: formValue.name,
        email: formValue.email,
        phoneNumber: formValue.phoneNumber,
        dateOfBirth: new Date(formValue.dateOfBirth).toISOString(),
        annualSalary: parseFloat(formValue.annualSalary),
        panNumber: formValue.panNumber.toUpperCase(),
        password: formValue.password
      };

      this.auth.registerCustomer(registerDto).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.successMessage = 'Registration successful! Redirecting to login...';
          setTimeout(() => {
            this.router.navigate(['/auth/login']);
          }, 2000);
        },
        error: (error: Error) => {
          this.isLoading.set(false);
          this.errorMessage = error.message || 'Registration failed. Please try again.';
        }
      });
    }
  }

  togglePasswordVisibility(): void {
    this.hidePassword = !this.hidePassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.hideConfirmPassword = !this.hideConfirmPassword;
  }
}
