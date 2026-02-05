import { Component, OnInit, signal } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule
} from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../auth.service';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';

type LoginRole = 'Customer' | 'Employee';

@Component({
  standalone: true,
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  imports: [
    ReactiveFormsModule,
    CommonModule,
    RouterLink,
    MatCardModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ]
})
export class LoginComponent implements OnInit {
  form!: FormGroup;
  isLoading = signal<boolean>(false);
  errorMessage = '';
  hidePassword = true;
  loginType: LoginRole = 'Customer';

  private readonly PASSWORD_REGEX =
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$/;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    // Get login type from route data
    this.route.data.subscribe(data => {
      if (data['type']) {
        this.loginType = data['type'];
      }
    });

    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.pattern(this.PASSWORD_REGEX)]]
    });
  }

  submit(): void {
    if (this.form.invalid) return;

    this.isLoading.set(true);
    this.errorMessage = '';

    const { email, password } = this.form.value;
    const loginDto = { email, password };

    const login$ = this.loginType === 'Customer'
      ? this.auth.loginCustomer(loginDto)
      : this.auth.loginEmployee(loginDto);

    login$.subscribe({
      next: () => {
        this.isLoading.set(false);

        // Get the actual user role from the token
        const user = this.auth.getCurrentUser();

        if (user) {
          // Navigate based on the actual role
          if (user.role === 'Customer') {
            this.router.navigate(['/customer']);
          } else if (user.role === 'Manager') {
            this.router.navigate(['/employee']);
          } else if (user.role === 'Admin') {
            this.router.navigate(['/admin']);
          } else {
            // Fallback
            this.router.navigate(['/unauthorized']);
          }
        } else {
          this.errorMessage = 'Failed to retrieve user information';
        }
      },
      error: (error: Error) => {
        this.isLoading.set(false);
        // this.errorMessage = error.message || 'Login failed. Please check your credentials.';
      }
    });
  }

  togglePasswordVisibility(): void {
    this.hidePassword = !this.hidePassword;
  }
}

