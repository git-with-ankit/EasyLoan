export interface CustomerProfile {
    name: string;
    email: string;
    dateOfBirth: string;
    phoneNumber: string;
    annualSalary: number;
    panNumber: string;
    creditScore: number;
}

export interface UpdateCustomerProfile {
    name?: string;
    phoneNumber?: string;
    annualSalary?: number;
}
