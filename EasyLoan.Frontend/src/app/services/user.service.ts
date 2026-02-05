import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, catchError, of, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface MeResponse {
    email: string;
    role: 'Customer' | 'Manager' | 'Admin';
}

@Injectable({ providedIn: 'root' })
export class UserService {
    private currentUserSignal = signal<MeResponse | null>(null);

    // Public readonly signal
    currentUser = this.currentUserSignal.asReadonly();

    constructor(private http: HttpClient) { }

    loadCurrentUser(): Observable<MeResponse | null> {
        // Use unified auth endpoint
        return this.http.get<MeResponse>(`${environment.apiUrl}/auth/me`).pipe(
            catchError(() => {
                // User is not authenticated
                this.currentUserSignal.set(null);
                return of(null);
            }),
            tap(user => {
                if (user) {
                    this.currentUserSignal.set(user);
                }
            })
        );
    }

    clearUser(): void {
        this.currentUserSignal.set(null);
    }
}
