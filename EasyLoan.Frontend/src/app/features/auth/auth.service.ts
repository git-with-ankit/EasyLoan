import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
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
      tap(token => this.handleLoginSuccess(token))
    );
  }

  loginEmployee(dto: EmployeeLoginRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/employees/login`, dto, {
      responseType: 'text' as 'json'
    }).pipe(
      tap(token => this.handleLoginSuccess(token))
    );
  }

  registerCustomer(dto: RegisterCustomerRequest): Observable<CustomerProfileResponse> {
    return this.http.post<CustomerProfileResponse>(`${this.baseUrl}/customers/register`, dto);
  }

  registerManager(dto: CreateManagerRequest): Observable<RegisterManagerResponse> {
    return this.http.post<RegisterManagerResponse>(`${this.baseUrl}/employees/manager/register`, dto);
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
}

