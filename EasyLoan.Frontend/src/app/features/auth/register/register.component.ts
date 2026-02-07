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
import { AuthService } from '../../../services/auth.service';

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
  private readonly EMAIL_REGEX = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  private readonly PHONE_REGEX = /^[6-9]\d{9}$/;
  private readonly PAN_REGEX = /^[A-Za-z]{5}[0-9]{4}[A-Za-z]$/;
  private readonly MAX_ANNUAL_SALARY = 1000000000000000; // 1000 trillion rupees

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
    this.isManagerRegistration = this.router.url.includes('/admin/create-manager');
    this.registrationType = this.isManagerRegistration ? 'Manager' : 'Customer';

    if (this.isManagerRegistration) {
      this.form = this.fb.group({
        role: ['Manager', Validators.required],
        name: ['', [Validators.required, Validators.maxLength(100)]],
        email: ['', [Validators.required, Validators.pattern(this.EMAIL_REGEX), Validators.maxLength(150)]],
        phoneNumber: ['', [Validators.required, Validators.pattern(this.PHONE_REGEX)]],
        password: ['', [Validators.required, Validators.pattern(this.PASSWORD_REGEX)]],
        confirmPassword: ['', Validators.required]
      });
    } else {
      this.form = this.fb.group({
        role: ['Customer', Validators.required],
        name: ['', [Validators.required, Validators.maxLength(100)]],
        email: ['', [Validators.required, Validators.pattern(this.EMAIL_REGEX), Validators.maxLength(150)]],
        phoneNumber: ['', [Validators.required, Validators.pattern(this.PHONE_REGEX)]],
        dateOfBirth: ['', Validators.required],
        annualSalary: ['', [Validators.required, Validators.min(0), Validators.max(this.MAX_ANNUAL_SALARY), this.decimalPlacesValidator(2)]],
        panNumber: ['', [Validators.required, Validators.pattern(this.PAN_REGEX)]],
        password: ['', [Validators.required, Validators.pattern(this.PASSWORD_REGEX)]],
        confirmPassword: ['', Validators.required]
      });
    }

    // Add dynamic password match validator to confirmPassword
    const confirmPasswordControl = this.form.get('confirmPassword');
    confirmPasswordControl?.addValidators(this.passwordMatchValidator());

    // Re-validate confirmPassword when password changes
    this.form.get('password')?.valueChanges.subscribe(() => {
      confirmPasswordControl?.updateValueAndValidity();
    });
  }

  // Custom validator that checks if passwords match
  passwordMatchValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;

      const password = this.form?.get('password')?.value;
      if (!password) return null;

      return control.value !== password ? { passwordMismatch: true } : null;
    };
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
            this.router.navigate(['/admin/dashboard']);
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
            this.router.navigate(['/auth/customer/login']);
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

  // Prevent non-numeric input for phone number
  onPhoneKeyPress(event: KeyboardEvent): void {
    const charCode = event.charCode;
    // Allow only numbers (0-9)
    if (charCode < 48 || charCode > 57) {
      event.preventDefault();
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
      const parts = currentValue.split('.');
      const decimalPart = parts[1] || '';
      const selectionStart = input.selectionStart || 0;
      const selectionEnd = input.selectionEnd || 0;
      const decimalIndex = currentValue.indexOf('.');

      // If we already have 2 decimal places and cursor is after decimal point
      if (decimalPart.length >= 2 && selectionStart > decimalIndex && selectionStart === selectionEnd && event.key >= '0' && event.key <= '9') {
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

  // Validate date input to prevent invalid dates
  onDateInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;

    // Check if the input contains letters
    if (/[a-zA-Z]/.test(value)) {
      input.value = '';
      const control = this.form.get('dateOfBirth');
      if (control) {
        control.setErrors({ invalidDate: true });
      }
    }
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
