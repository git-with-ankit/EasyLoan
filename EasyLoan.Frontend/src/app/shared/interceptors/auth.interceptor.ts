import { HttpEventType, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, tap, throwError } from 'rxjs';
import { TokenService } from '../services/token.service';
import { NotificationService } from '../services/notification.service';
import { extractErrorMessage } from '../models/error.model';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const tokenService = inject(TokenService);
    const router = inject(Router);
    const notificationService = inject(NotificationService);

    // Clone request and add authorization header if token exists
    const token = tokenService.getToken();
    if (token) {
        req = req.clone({
            setHeaders: {
                Authorization: `Bearer ${token}`
            }
        });
    }

    return next(req).pipe(
        catchError((error) => {
            // Use the utility function to extract error message
            const errorMessage = extractErrorMessage(error);

            // Handle specific status codes
            if (error.status === 401) {
                // Unauthorized - clear token and redirect to login
                tokenService.removeToken();
                notificationService.error(errorMessage);
                router.navigate(['/auth/login']);
            } else if (error.status === 403) {
                // Forbidden - show error and redirect
                notificationService.error(errorMessage);
                router.navigate(['/unauthorized']);
            } else if (error.status === 404) {
                notificationService.error(errorMessage);
            } else if (error.status === 500) {
                notificationService.error(errorMessage);
            } else if (error.status === 0) {
                // Network error
                notificationService.error('Network error. Please check your connection.');
            } else {
                // Display the extracted error message
                notificationService.error(errorMessage);
            }

            return throwError(() => new Error(errorMessage));
        })
    );
};

function LoggingInterceptor(request: HttpRequest<unknown>, next: HttpHandlerFn) {
    console.log(request);
    return next(request).pipe(
        tap({
            next: event => {
                if (event.type === HttpEventType.Response) {
                    console.log('[Incoming Request]');
                    console.log(event.status);
                    console.log(event.body);
                }
            }
        })
    )
}