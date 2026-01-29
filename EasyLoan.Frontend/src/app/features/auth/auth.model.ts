export interface CustomerLoginRequestDto {
  email: string;
  password: string;
}

export interface EmployeeLoginRequestDto {
  email: string;
  password: string;
}

export interface RegisterCustomerRequestDto {
  name: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
  annualSalary: number;
  panNumber: string;
  password: string;
}

export interface CreateManagerRequestDto {
  name: string;
  email: string;
  phoneNumber: string;
  password: string;
}

export type UserRole = 'Customer' | 'Employee' | 'Manager';
