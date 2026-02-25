import { HttpEventType, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, tap, throwError } from 'rxjs';
import { UserService } from '../services/user.service';
import { NotificationService } from '../services/notification.service';
import { extractErrorMessage } from '../models/error.model';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const userService = inject(UserService);
    const router = inject(Router);
    const notificationService = inject(NotificationService);

    req = req.clone({
        withCredentials: true
    });

    return next(req).pipe(
        catchError((error) => {
            const errorMessage = extractErrorMessage(error);

            const isAuthMeEndpoint = req.url.includes('/auth/me');

            // Handle specific status codes
            if (error.status === 401) {
                // Unauthorized - clear user and redirect to landing
                userService.clearUser();

                // Only show notification if not from auth/me endpoint
                if (!isAuthMeEndpoint) {
                    notificationService.error(errorMessage);
                    router.navigate(['/landing']);
                }
            } else if (error.status === 403) {
                // Forbidden - show error and redirect to landing
                notificationService.error(errorMessage);
                router.navigate(['/landing']);
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