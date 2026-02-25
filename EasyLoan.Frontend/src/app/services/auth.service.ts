import { HttpClient } from '@angular/common/http';
import { Injectable, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, switchMap } from 'rxjs';
import { Router } from '@angular/router';
import {
  CustomerLoginRequest,
  EmployeeLoginRequest,
  RegisterCustomerRequest,
  CreateManagerRequest,
  CustomerProfileResponse,
  RegisterManagerResponse,
  UserRole,
  ChangePasswordRequest
} from '../models/auth.models';
import { UserService, MeResponse } from './user.service';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = `${environment.apiUrl}`;
  private userService = inject(UserService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  constructor(private http: HttpClient) { }

  loginCustomer(dto: CustomerLoginRequest): Observable<MeResponse | null> {
    return this.http.post(`${this.baseUrl}/customers/login`, dto, {
      withCredentials: true
    }).pipe(
      switchMap(() => this.userService.loadCurrentUser())
    );
  }

  loginEmployee(dto: EmployeeLoginRequest): Observable<MeResponse | null> {
    return this.http.post(`${this.baseUrl}/employees/login`, dto, {
      withCredentials: true
    }).pipe(
      switchMap(() => this.userService.loadCurrentUser())
    );
  }

  registerCustomer(dto: RegisterCustomerRequest): Observable<CustomerProfileResponse> {
    return this.http.post<CustomerProfileResponse>(`${this.baseUrl}/customers/register`, dto);
  }

  registerManager(dto: CreateManagerRequest): Observable<RegisterManagerResponse> {
    return this.http.post<RegisterManagerResponse>(`${this.baseUrl}/employees/manager/register`, dto);
  }

  logout(): void {
    // Use unified auth logout endpoint
    this.http.post(`${this.baseUrl}/auth/logout`, {}, { withCredentials: true })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.userService.clearUser();
          this.router.navigate(['/landing']);
        },
        error: () => {
          // Even if logout fails, clear local state and redirect
          this.userService.clearUser();
          this.router.navigate(['/landing']);
        }
      });
  }

  isAuthenticated(): boolean {
    return this.userService.currentUser() !== null;
  }

  getCurrentUser(): MeResponse | null {
    return this.userService.currentUser();
  }

  changePassword(dto: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auth/change-password`, dto, {
      withCredentials: true
    });
  }
}

