import { HttpClient } from "@angular/common/http";
import { CreateManagerRequestDto, CustomerLoginRequestDto, EmployeeLoginRequestDto, RegisterCustomerRequestDto, UserRole } from "./auth.model";
import { Injectable } from "@angular/core";

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = '/api';

  constructor(private http: HttpClient) {}

  login(role: UserRole, dto: CustomerLoginRequestDto | EmployeeLoginRequestDto) {
    const url =
      role === 'Customer'
        ? `${this.baseUrl}/customers/login`
        : `${this.baseUrl}/employees/login`;

    return this.http.post(url, dto);
  }

  register(role: UserRole, dto: RegisterCustomerRequestDto | CreateManagerRequestDto) {
    const url =
      role === 'Customer'
        ? `${this.baseUrl}/customers/register`
        : `${this.baseUrl}/employees/manager/register`;

    return this.http.post(url, dto);
  }
}
