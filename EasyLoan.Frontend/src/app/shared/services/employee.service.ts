import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { EmployeeProfile, UpdateEmployeeProfile } from '../models/employee.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
    private baseUrl = `${environment.apiUrl}/employees`;
    private http = inject(HttpClient);

    getProfile(): Observable<EmployeeProfile> {
        return this.http.get<EmployeeProfile>(`${this.baseUrl}/profile`);
    }

    updateProfile(profile: UpdateEmployeeProfile): Observable<EmployeeProfile> {
        return this.http.patch<EmployeeProfile>(`${this.baseUrl}/profile`, profile);
    }
}
