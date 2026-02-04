import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoanType, EmiScheduleItem, CreateLoanTypeRequest, UpdateLoanTypeRequest } from '../models/loan-type.models';

@Injectable({
    providedIn: 'root'
})
export class LoanTypeService {
    private apiUrl = `${environment.apiUrl}/loan-types`;

    constructor(private http: HttpClient) { }

    getLoanTypes(): Observable<LoanType[]> {
        return this.http.get<LoanType[]>(this.apiUrl);
    }

    getLoanTypeById(id: string): Observable<LoanType> {
        return this.http.get<LoanType>(`${this.apiUrl}/${id}`);
    }

    createLoanType(data: CreateLoanTypeRequest): Observable<LoanType> {
        return this.http.post<LoanType>(this.apiUrl, data);
    }

    updateLoanType(id: string, data: UpdateLoanTypeRequest): Observable<LoanType> {
        return this.http.patch<LoanType>(`${this.apiUrl}/${id}`, data);
    }

    previewEmiPlan(loanTypeId: string, amount: number, tenureInMonths: number): Observable<EmiScheduleItem[]> {
        const params = new HttpParams()
            .set('amount', amount.toString())
            .set('tenureInMonths', tenureInMonths.toString());

        return this.http.get<EmiScheduleItem[]>(`${this.apiUrl}/${loanTypeId}/emi-plan`, { params });
    }
}
