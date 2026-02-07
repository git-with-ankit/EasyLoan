import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
    LoanApplicationSummary,
    LoanApplicationDetails,
    LoanApplicationStatus
} from '../models/application.models';
import { CreateApplicationRequest, CreateApplicationResponse } from '../models/loan-type.models';
import {
    LoanApplicationDetailsWithCustomerData,
    ReviewLoanApplicationRequest,
    LoanApplicationReviewResponse
} from '../models/review.models';
import { environment } from '../../environments/environment';
import { PagedResponse, PaginationParams } from '../models/pagination.models';

@Injectable({ providedIn: 'root' })
export class ApplicationService {
    private baseUrl = `${environment.apiUrl}/loan-applications`;
    private http = inject(HttpClient);

    getApplications(status: LoanApplicationStatus, pagination: PaginationParams): Observable<PagedResponse<LoanApplicationSummary>> {
        const params = new HttpParams()
            .set('status', status)
            .set('pageNumber', pagination.pageNumber.toString())
            .set('pageSize', pagination.pageSize.toString());
        return this.http.get<PagedResponse<LoanApplicationSummary>>(this.baseUrl, { params });
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
