import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CustomerProfile, UpdateCustomerProfile } from '../models/customer.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CustomerService {
    private baseUrl = `${environment.apiUrl}/customers`;
    private http = inject(HttpClient);

    /**
     * Get the authenticated customer's profile
     * @returns Observable of customer profile
     */
    getProfile(): Observable<CustomerProfile> {
        return this.http.get<CustomerProfile>(`${this.baseUrl}/profile`);
    }

    /**
     * Update the authenticated customer's profile
     * @param profile - Updated profile data
     * @returns Observable of updated customer profile
     */
    updateProfile(profile: UpdateCustomerProfile): Observable<CustomerProfile> {
        return this.http.patch<CustomerProfile>(`${this.baseUrl}/profile`, profile);
    }
}
