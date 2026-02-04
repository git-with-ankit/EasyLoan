import { Injectable } from '@angular/core';
import { AuthUser } from '../models/auth.models';

@Injectable({
    providedIn: 'root'
})
export class TokenService {
    private readonly TOKEN_KEY = 'easyloan_token';

    constructor() { }

    setToken(token: string): void {
        localStorage.setItem(this.TOKEN_KEY, token);
    }

    getToken(): string | null {
        return localStorage.getItem(this.TOKEN_KEY);
    }

    removeToken(): void {
        localStorage.removeItem(this.TOKEN_KEY);
    }

    decodeToken(): AuthUser | null {
        const token = this.getToken();
        if (!token) return null;

        try {
            const payload = token.split('.')[1];
            const decoded = JSON.parse(atob(payload));

            // Get role from token - handle both string and array formats
            let role = decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

            // If role is an array, take the first element
            if (Array.isArray(role)) {
                role = role[0];
            }

            return {
                id: decoded.sub || decoded.id || decoded.nameid,
                email: decoded.email,
                role: role,
                exp: decoded.exp
            };
        } catch (error) {
            console.error('Error decoding token:', error);
            return null;
        }
    }

    isTokenExpired(): boolean {
        const user = this.decodeToken();
        if (!user || !user.exp) return true;

        const currentTime = Math.floor(Date.now() / 1000);
        return user.exp < currentTime;
    }

    isAuthenticated(): boolean {
        return this.getToken() !== null && !this.isTokenExpired();
    }

    getCurrentUser(): AuthUser | null {
        if (!this.isAuthenticated()) return null;
        return this.decodeToken();
    }
}
