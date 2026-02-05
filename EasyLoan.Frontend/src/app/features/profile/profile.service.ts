import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, switchMap, of, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UserService } from '../../services/user.service';
import { CustomerProfile, UpdateCustomerProfile } from './customer-profile.models';
import { EmployeeProfile, UpdateEmployeeProfile } from './employee-profile.models';

export type UserProfile = CustomerProfile | EmployeeProfile;
export type UpdateUserProfile = UpdateCustomerProfile | UpdateEmployeeProfile;

@Injectable({ providedIn: 'root' })
export class ProfileService {
    private http = inject(HttpClient);
    private userService = inject(UserService);
    private apiUrl = environment.apiUrl;

    getProfile(): Observable<UserProfile> {
        const user = this.userService.currentUser();
        if (!user) {
            return throwError(() => new Error('User not authenticated'));
        }

        if (user.role === 'Customer') {
            return this.http.get<CustomerProfile>(`${this.apiUrl}/customers/profile`);
        } else {
            return this.http.get<EmployeeProfile>(`${this.apiUrl}/employees/profile`);
        }
    }

    updateProfile(profile: UpdateUserProfile): Observable<UserProfile> {
        const user = this.userService.currentUser();
        if (!user) {
            return throwError(() => new Error('User not authenticated'));
        }

        if (user.role === 'Customer') {
            return this.http.patch<CustomerProfile>(`${this.apiUrl}/customers/profile`, profile);
        } else {
            return this.http.patch<EmployeeProfile>(`${this.apiUrl}/employees/profile`, profile);
        }
    }
}
