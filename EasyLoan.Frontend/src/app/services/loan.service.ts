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
import { environment } from '../../environments/environment';
import { PagedResponse, PaginationParams } from '../models/pagination.models';

@Injectable({ providedIn: 'root' })
export class LoanService {
    private baseUrl = `${environment.apiUrl}/loans`;
    private http = inject(HttpClient);

    // Overload signatures
    getLoans(status: LoanStatus): Observable<LoanSummary[]>;
    getLoans(status: LoanStatus, pagination: PaginationParams): Observable<PagedResponse<LoanSummary>>;

    // Implementation
    getLoans(status: LoanStatus, pagination?: PaginationParams): Observable<LoanSummary[] | PagedResponse<LoanSummary>> {
        let params = new HttpParams().set('status', status);

        if (pagination) {
            params = params
                .set('pageNumber', pagination.pageNumber.toString())
                .set('pageSize', pagination.pageSize.toString());
            return this.http.get<PagedResponse<LoanSummary>>(this.baseUrl, { params });
        }

        return this.http.get<LoanSummary[]>(this.baseUrl, { params });
    }

    getLoanDetails(loanNumber: string): Observable<LoanDetails> {
        return this.http.get<LoanDetails>(`${this.baseUrl}/${loanNumber}`);
    }

    // Overload signatures
    getAllDueEmis(status: EmiDueStatus): Observable<LoanEmiGroup[]>;
    getAllDueEmis(status: EmiDueStatus, pagination: PaginationParams): Observable<PagedResponse<LoanEmiGroup>>;

    // Implementation
    getAllDueEmis(status: EmiDueStatus, pagination?: PaginationParams): Observable<LoanEmiGroup[] | PagedResponse<LoanEmiGroup>> {
        let params = new HttpParams().set('status', status);

        if (pagination) {
            params = params
                .set('pageNumber', pagination.pageNumber.toString())
                .set('pageSize', pagination.pageSize.toString());
            return this.http.get<PagedResponse<LoanEmiGroup>>(`${this.baseUrl}/emis`, { params });
        }

        return this.http.get<LoanEmiGroup[]>(`${this.baseUrl}/emis`, { params });
    }

    // Overload signatures
    getDueEmisForLoan(loanNumber: string, status: EmiDueStatus): Observable<DueEmiResponse[]>;
    getDueEmisForLoan(loanNumber: string, status: EmiDueStatus, pagination: PaginationParams): Observable<PagedResponse<DueEmiResponse>>;

    // Implementation
    getDueEmisForLoan(loanNumber: string, status: EmiDueStatus, pagination?: PaginationParams): Observable<DueEmiResponse[] | PagedResponse<DueEmiResponse>> {
        let params = new HttpParams().set('status', status);

        if (pagination) {
            params = params
                .set('pageNumber', pagination.pageNumber.toString())
                .set('pageSize', pagination.pageSize.toString());
            return this.http.get<PagedResponse<DueEmiResponse>>(`${this.baseUrl}/${loanNumber}/emis`, { params });
        }

        return this.http.get<DueEmiResponse[]>(`${this.baseUrl}/${loanNumber}/emis`, { params });
    }

    makePayment(loanNumber: string, amount: number): Observable<LoanPaymentResponse> {
        const request: MakeLoanPaymentRequest = { amount };
        return this.http.post<LoanPaymentResponse>(
            `${this.baseUrl}/${loanNumber}/payments`,
            request
        );
    }

    // Overload signatures
    getPaymentHistory(loanNumber: string): Observable<PaymentHistory[]>;
    getPaymentHistory(loanNumber: string, pagination: PaginationParams): Observable<PagedResponse<PaymentHistory>>;

    // Implementation
    getPaymentHistory(loanNumber: string, pagination?: PaginationParams): Observable<PaymentHistory[] | PagedResponse<PaymentHistory>> {
        if (pagination) {
            const params = new HttpParams()
                .set('pageNumber', pagination.pageNumber.toString())
                .set('pageSize', pagination.pageSize.toString());
            return this.http.get<PagedResponse<PaymentHistory>>(`${this.baseUrl}/${loanNumber}/payments`, { params });
        }

        return this.http.get<PaymentHistory[]>(`${this.baseUrl}/${loanNumber}/payments`);
    }
}
