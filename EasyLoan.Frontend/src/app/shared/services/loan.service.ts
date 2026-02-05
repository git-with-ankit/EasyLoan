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

    getLoans(status: LoanStatus): Observable<LoanSummary[]> {
        const params = new HttpParams().set('status', status);
        return this.http.get<LoanSummary[]>(this.baseUrl, { params });
    }

    getLoanDetails(loanNumber: string): Observable<LoanDetails> {
        return this.http.get<LoanDetails>(`${this.baseUrl}/${loanNumber}`);
    }

    getAllDueEmis(status: EmiDueStatus): Observable<LoanEmiGroup[]> {
        const params = new HttpParams().set('status', status);
        return this.http.get<LoanEmiGroup[]>(`${this.baseUrl}/emis`, { params });
    }

    getDueEmisForLoan(loanNumber: string, status: EmiDueStatus): Observable<DueEmiResponse[]> {
        const params = new HttpParams().set('status', status);
        return this.http.get<DueEmiResponse[]>(`${this.baseUrl}/${loanNumber}/emis`, { params });
    }

    makePayment(loanNumber: string, amount: number): Observable<LoanPaymentResponse> {
        const request: MakeLoanPaymentRequest = { amount };
        return this.http.post<LoanPaymentResponse>(
            `${this.baseUrl}/${loanNumber}/payments`,
            request
        );
    }

    getPaymentHistory(loanNumber: string): Observable<PaymentHistory[]> {
        return this.http.get<PaymentHistory[]>(`${this.baseUrl}/${loanNumber}/payments`);
    }
}
