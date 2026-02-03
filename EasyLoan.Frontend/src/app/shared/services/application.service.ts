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
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApplicationService {
    private baseUrl = `${environment.apiUrl}/loan-applications`;
    private http = inject(HttpClient);

    /**
     * Get loan applications for the authenticated customer
     * @param status - Filter by application status (Pending, Approved, Rejected)
     * @returns Observable of loan applications
     */
    getApplications(status: LoanApplicationStatus): Observable<LoanApplicationSummary[]> {
        const params = new HttpParams().set('status', status);
        return this.http.get<LoanApplicationSummary[]>(this.baseUrl, { params });
    }

    /**
     * Get detailed information for a specific loan application
     * @param applicationNumber - The application number
     * @returns Observable of application details
     */
    getApplicationDetails(applicationNumber: string): Observable<LoanApplicationDetails> {
        return this.http.get<LoanApplicationDetails>(`${this.baseUrl}/${applicationNumber}`);
    }

    /**
     * Create a new loan application
     * @param request - Application request data
     * @returns Observable of created application response
     */
    createApplication(request: CreateApplicationRequest): Observable<CreateApplicationResponse> {
        return this.http.post<CreateApplicationResponse>(this.baseUrl, request);
    }

    /**
     * Get application details with customer data for review (Manager only)
     * @param applicationNumber - The application number
     * @returns Observable of application details with customer data
     */
    getApplicationDetailsForReview(applicationNumber: string): Observable<LoanApplicationDetailsWithCustomerData> {
        return this.http.get<LoanApplicationDetailsWithCustomerData>(`${this.baseUrl}/${applicationNumber}/review`);
    }

    /**
     * Submit a review for a loan application (Manager only)
     * @param applicationNumber - The application number
     * @param request - Review data (status and remarks)
     * @returns Observable of review response
     */
    submitReview(applicationNumber: string, request: ReviewLoanApplicationRequest): Observable<LoanApplicationReviewResponse> {
        return this.http.post<LoanApplicationReviewResponse>(`${this.baseUrl}/${applicationNumber}/review`, request);
    }
}
