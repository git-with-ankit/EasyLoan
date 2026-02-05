import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminDashboardResponse } from './dashboard.models';

@Injectable({
    providedIn: 'root'
})
export class AdminService {
    private apiUrl = `${environment.apiUrl}/employees`;

    constructor(private http: HttpClient) { }

    getAdminDashboard(): Observable<AdminDashboardResponse> {
        return this.http.get<AdminDashboardResponse>(`${this.apiUrl}/admin/dashboard`);
    }
}
