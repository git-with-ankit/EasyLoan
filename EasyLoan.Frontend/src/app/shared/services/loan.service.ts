import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import {
    DueEmiResponse,
    EmiDueStatus,
    LoanEmiGroup,
    MakeLoanPaymentRequest,
    LoanPaymentResponse,
    LoanSummary,
    LoanDetails,
    LoanStatus,
    PaymentHistory
} from '../models/loan.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LoanService {
    private baseUrl = `${environment.apiUrl}/loans`;
    private http = inject(HttpClient);

    /**
     * Get loans for the authenticated customer
     * @param status - Filter by loan status (Active, Closed, Defaulted)
     * @returns Observable of loans
     */
    getLoans(status: LoanStatus): Observable<LoanSummary[]> {
        const params = new HttpParams().set('status', status);
        return this.http.get<LoanSummary[]>(this.baseUrl, { params });
    }

    /**
     * Get detailed information for a specific loan
     * @param loanNumber - The loan number
     * @returns Observable of loan details
     */
    getLoanDetails(loanNumber: string): Observable<LoanDetails> {
        return this.http.get<LoanDetails>(`${this.baseUrl}/${loanNumber}`);
    }

    /**
     * Get all due EMIs for the authenticated customer
     * @param status - Filter by Upcoming or Overdue status
     * @returns Observable of EMIs grouped by loan
     */
    getAllDueEmis(status: EmiDueStatus): Observable<LoanEmiGroup[]> {
        const params = new HttpParams().set('status', status);
        return this.http.get<LoanEmiGroup[]>(`${this.baseUrl}/emis`, { params });
    }

    /**
     * Get due EMIs for a specific loan
     * @param loanNumber - The loan number
     * @param status - Filter by Upcoming or Overdue status
     * @returns Observable of EMIs for the loan
     */
    getDueEmisForLoan(loanNumber: string, status: EmiDueStatus): Observable<DueEmiResponse[]> {
        const params = new HttpParams().set('status', status);
        return this.http.get<DueEmiResponse[]>(`${this.baseUrl}/${loanNumber}/emis`, { params });
    }

    /**
     * Make a payment for a specific loan
     * @param loanNumber - The loan number to pay
     * @param amount - The payment amount
     * @returns Observable of payment response
     */
    makePayment(loanNumber: string, amount: number): Observable<LoanPaymentResponse> {
        const request: MakeLoanPaymentRequest = { amount };
        return this.http.post<LoanPaymentResponse>(
            `${this.baseUrl}/${loanNumber}/payments`,
            request
        );
    }

    /**
     * Get payment history for a specific loan
     * @param loanNumber - The loan number
     * @returns Observable of payment history
     */
    getPaymentHistory(loanNumber: string): Observable<PaymentHistory[]> {
        return this.http.get<PaymentHistory[]>(`${this.baseUrl}/${loanNumber}/payments`);
    }
}
