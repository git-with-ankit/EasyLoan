// Login Request DTOs
export interface CustomerLoginRequest {
  email: string;
  password: string;
}

export interface EmployeeLoginRequest {
  email: string;
  password: string;
}

// Register Request DTOs
export interface RegisterCustomerRequest {
  name: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string; // ISO date string
  annualSalary: number;
  panNumber: string;
  password: string;
}

export interface CreateManagerRequest {
  name: string;
  email: string;
  phoneNumber: string;
  password: string;
}

// Response DTOs
export interface CustomerProfileResponse {
  name: string;
  email: string;
  dateOfBirth: string;
  phoneNumber: string;
  annualSalary: number;
  panNumber: string;
  creditScore: number;
}

export interface RegisterManagerResponse {
  name: string;
  email: string;
  phoneNumber: string;
  role: EmployeeRole;
}

export interface EmployeeProfileResponse {
  name: string;
  email: string;
  phoneNumber: string;
  role: EmployeeRole;
}

// Enums
export enum EmployeeRole {
  Manager = 'Manager',
  Admin = 'Admin'
}

export type UserRole = 'Customer' | 'Manager' | 'Admin';

// Auth User (decoded from JWT)
export interface AuthUser {
  id: string;
  email: string;
  role: UserRole;
  exp?: number; // Token expiration timestamp
}

// Login Response
export interface LoginResponse {
  token: string;
}
