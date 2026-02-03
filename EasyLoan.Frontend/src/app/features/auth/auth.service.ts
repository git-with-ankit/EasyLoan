import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import {
  CustomerLoginRequest,
  EmployeeLoginRequest,
  RegisterCustomerRequest,
  CreateManagerRequest,
  CustomerProfileResponse,
  RegisterManagerResponse,
  UserRole,
  AuthUser
} from '../../shared/models/auth.models';
import { TokenService } from '../../shared/services/token.service';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = `${environment.apiUrl}`;
  private tokenService = inject(TokenService);
  private router = inject(Router);

  constructor(private http: HttpClient) { }

  loginCustomer(dto: CustomerLoginRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/customers/login`, dto, {
      responseType: 'text' as 'json'
    }).pipe(
      tap(token => this.handleLoginSuccess(token)),
      catchError(this.handleError)
    );
  }

  loginEmployee(dto: EmployeeLoginRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/employees/login`, dto, {
      responseType: 'text' as 'json'
    }).pipe(
      tap(token => this.handleLoginSuccess(token)),
      catchError(this.handleError)
    );
  }

  registerCustomer(dto: RegisterCustomerRequest): Observable<CustomerProfileResponse> {
    return this.http.post<CustomerProfileResponse>(`${this.baseUrl}/customers/register`, dto).pipe(
      catchError(this.handleError)
    );
  }

  registerManager(dto: CreateManagerRequest): Observable<RegisterManagerResponse> {
    return this.http.post<RegisterManagerResponse>(`${this.baseUrl}/employees/manager/register`, dto).pipe(
      catchError(this.handleError)
    );
  }

  logout(): void {
    this.tokenService.removeToken();
    this.router.navigate(['/auth/login']);
  }

  isAuthenticated(): boolean {
    return this.tokenService.isAuthenticated();
  }

  getCurrentUser(): AuthUser | null {
    return this.tokenService.getCurrentUser();
  }

  private handleLoginSuccess(token: string): void {
    this.tokenService.setToken(token);
  }

  private handleError = (error: any): Observable<never> => {
    let errorMessage = 'An error occurred. Please try again.';

    if (error.error) {
      if (typeof error.error === 'string') {
        errorMessage = error.error;
      } else if (error.error.title) {
        errorMessage = error.error.title;
      } else if (error.error.errors) {
        // Validation errors
        const errors = Object.values(error.error.errors).flat();
        errorMessage = errors.join(', ');
      }
    } else if (error.message) {
      errorMessage = error.message;
    }

    return throwError(() => new Error(errorMessage));
  }
}

