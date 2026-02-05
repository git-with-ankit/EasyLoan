import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
    LoanApplicationSummary,
    LoanApplicationDetails,
    LoanApplicationStatus
} from './application.models';
import { CreateApplicationRequest, CreateApplicationResponse } from '../loan-types/loan-type.models';
import {
    LoanApplicationDetailsWithCustomerData,
    ReviewLoanApplicationRequest,
    LoanApplicationReviewResponse
} from './review.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApplicationService {
    private baseUrl = `${environment.apiUrl}/loan-applications`;
    private http = inject(HttpClient);

    getApplications(status: LoanApplicationStatus): Observable<LoanApplicationSummary[]> {
        const params = new HttpParams().set('status', status);
        return this.http.get<LoanApplicationSummary[]>(this.baseUrl, { params });
    }

    getApplicationDetails(applicationNumber: string): Observable<LoanApplicationDetails> {
        return this.http.get<LoanApplicationDetails>(`${this.baseUrl}/${applicationNumber}`);
    }

    createApplication(request: CreateApplicationRequest): Observable<CreateApplicationResponse> {
        return this.http.post<CreateApplicationResponse>(this.baseUrl, request);
    }

    getApplicationDetailsForReview(applicationNumber: string): Observable<LoanApplicationDetailsWithCustomerData> {
        return this.http.get<LoanApplicationDetailsWithCustomerData>(`${this.baseUrl}/${applicationNumber}/review`);
    }

    submitReview(applicationNumber: string, request: ReviewLoanApplicationRequest): Observable<LoanApplicationReviewResponse> {
        return this.http.post<LoanApplicationReviewResponse>(`${this.baseUrl}/${applicationNumber}/review`, request);
    }
}
