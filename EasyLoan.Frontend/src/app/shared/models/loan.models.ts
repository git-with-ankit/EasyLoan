export interface DueEmiResponse {
    dueDate: string; // ISO date string
    emiAmount: number;
    interestComponent: number;
    principalComponent: number;
    principalRemainingAfterPayment: number;
    remainingEmiAmount: number;
    penaltyAmount: number;
}

export interface MakeLoanPaymentRequest {
    amount: number;
}

export interface LoanPaymentResponse {
    paymentDate: string; // ISO date string
    amount: number;
    transactionId: string;
}

export enum EmiDueStatus {
    Upcoming = 'Upcoming',
    Overdue = 'Overdue'
}

export interface LoanEmiGroup {
    loanNumber: string;
    emis: DueEmiResponse[];
}

// Loan Summary and Details
export interface LoanSummary {
    loanNumber: string;
    principalRemaining: number;
    interestRate: number;
    status: LoanStatus;
}

export interface LoanDetails {
    loanNumber: string;
    loanType: string;
    approvedAmount: number;
    principalRemaining: number;
    tenureInMonths: number;
    interestRate: number;
    status: LoanStatus;
}

export enum LoanStatus {
    Active = 'Active',
    Closed = 'Closed'
}

export interface PaymentHistory {
    paymentDate: string; // ISO date string
    amount: number;
    status: LoanPaymentStatus;
}

export enum LoanPaymentStatus {
    Paid = 'Paid',
    Pending = 'Pending',
    Failed = 'Failed'
}
