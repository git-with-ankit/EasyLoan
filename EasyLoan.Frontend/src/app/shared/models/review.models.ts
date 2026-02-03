import { LoanApplicationStatus } from './application.models';

export interface ReviewLoanApplicationRequest {
    status: LoanApplicationStatus; // Approved or Rejected
    approvedAmount: number;
    remarks: string;
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
