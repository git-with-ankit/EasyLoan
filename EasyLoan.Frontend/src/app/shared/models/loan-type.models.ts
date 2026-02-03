export interface LoanType {
    id: string;
    name: string;
    interestRate: number;
    minAmount: number;
    maxTenureInMonths: number;
}

export interface EmiScheduleItem {
    emiNumber: number;
    dueDate: string;
    principalComponent: number;
    interestComponent: number;
    totalEmiAmount: number;
    principalRemainingAfterPayment: number;
}

export interface CreateApplicationRequest {
    loanTypeId: string;
    requestedAmount: number;
    requestedTenureInMonths: number;
}

export interface CreateApplicationResponse {
    applicationNumber: string;
    status: string;
    createdDate: string;
}
