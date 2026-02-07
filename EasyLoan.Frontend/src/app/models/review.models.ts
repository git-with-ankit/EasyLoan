import { LoanApplicationStatus } from './application.models';

// Request DTO that matches backend ReviewLoanApplicationRequestDto
export interface ReviewLoanApplicationRequest {
    isApproved: boolean; // true for Approve, false for Reject
    approvedAmount: number;
    managerComments: string;
}

export interface LoanApplicationReviewResponse {
    applicationNumber: string;
    status: LoanApplicationStatus;
    remarks: string;
    reviewedBy: string;
    reviewedOn: string;
}

// This matches the backend LoanApplicationDetailsWithCustomerDataResponseDto
export interface LoanApplicationDetailsWithCustomerData {
    applicationNumber: string;
    customerName: string;
    annualSalaryOfCustomer: number;
    phoneNumber: string;
    creditScore: number;
    dateOfBirth: string; // ISO date string
    panNumber: string;
    loanType: string;
    requestedAmount: number;
    approvedAmount: number;
    interestRate: number;
    requestedTenureInMonths: number;
    status: LoanApplicationStatus;
    managerComments?: string;
    totalOngoingLoans: number;
}
