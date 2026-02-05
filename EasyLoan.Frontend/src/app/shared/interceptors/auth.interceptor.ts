import { HttpEventType, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, tap, throwError } from 'rxjs';
import { TokenService } from '../services/token.service';
import { NotificationService } from '../services/notification.service';
import { isValidationProblemDetails, isProblemDetails } from '../models/error.model';

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
            let errorMessage = 'An unexpected error occurred';

            // Debug logging
            console.log('=== ERROR INTERCEPTOR DEBUG ===');
            console.log('Full error object:', error);
            console.log('error.error:', error.error);
            console.log('error.error type:', typeof error.error);
            console.log('error.status:', error.status);

            // Extract error message from HTTP error response
            if (error.error) {
                let errorData = error.error;

                // If error.error is a stringified JSON, parse it first
                if (typeof error.error === 'string') {
                    try {
                        errorData = JSON.parse(error.error);
                        console.log('Parsed JSON error:', errorData);
                    } catch (e) {
                        // If parsing fails, it's a plain string error
                        console.log('Plain string error (not JSON)');
                        errorMessage = error.error;
                        errorData = null;
                    }
                }

                // Now check the parsed/object error
                if (errorData) {
                    // Check if it's a ValidationProblemDetails (model validation errors)
                    if (isValidationProblemDetails(errorData)) {
                        console.log('Detected ValidationProblemDetails');
                        const validationErrors: string[] = [];
                        Object.entries(errorData.errors).forEach(([field, messages]) => {
                            if (Array.isArray(messages)) {
                                messages.forEach((msg: string) => {
                                    validationErrors.push(`${field}: ${msg}`);
                                });
                            }
                        });

                        if (validationErrors.length > 0) {
                            errorMessage = validationErrors.join('; ');
                        } else if (errorData.detail) {
                            errorMessage = errorData.detail;
                        }
                    }
                    // Check if it's a standard ProblemDetails
                    else if (isProblemDetails(errorData)) {
                        console.log('Detected ProblemDetails');
                        errorMessage = errorData.detail || errorData.title;
                    }
                }
            }
            // Handle JavaScript Error objects
            else if (error.message) {
                console.log('Using error.message');
                errorMessage = error.message;
            }

            console.log('Extracted errorMessage:', errorMessage);
            console.log('errorMessage type:', typeof errorMessage);
            console.log('=== END DEBUG ===');

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