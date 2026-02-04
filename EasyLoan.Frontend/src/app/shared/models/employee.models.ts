export enum EmployeeRole {
    Manager = 'Manager',
    Admin = 'Admin'
}

export interface EmployeeProfile {
    name: string;
    email: string;
    phoneNumber: string;
    role: EmployeeRole;
}

export interface UpdateEmployeeProfile {
    name?: string;
    phoneNumber?: string;
}
