// Loan Application Models
export interface LoanApplicationSummary {
    applicationNumber: string;
    loanTypeName: string;
    requestedAmount: number;
    tenureInMonths: number;
    assignedEmployeeId: string;
    status: LoanApplicationStatus;
    createdDate: string; 
}

export interface LoanApplicationDetails {
    applicationNumber: string;
    customerName: string;
    loanType: string;
    requestedAmount: number;
    approvedAmount: number;
    interestRate: number;
    assignedEmployeeId: string;
    requestedTenureInMonths: number;
    status: LoanApplicationStatus;
    managerComments?: string;
}

export enum LoanApplicationStatus {
    Pending = 'Pending',
    Approved = 'Approved',
    Rejected = 'Rejected'
}
