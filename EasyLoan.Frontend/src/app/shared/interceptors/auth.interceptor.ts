import { HttpEventType, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, tap, throwError } from 'rxjs';
import { TokenService } from '../services/token.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const tokenService = inject(TokenService);
    const router = inject(Router);

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
            if (error.status === 401) {
                // Unauthorized - clear token and redirect to login
                tokenService.removeToken();
                router.navigate(['/auth/login']);
            } else if (error.status === 403) {
                // Forbidden - redirect to unauthorized page
                router.navigate(['/unauthorized']);
            }
            return throwError(() => error);
        })
    );
};

function LoggingInterceptor(request : HttpRequest<unknown> , next : HttpHandlerFn){
     console.log(request);
     return next(request).pipe(
        tap({
            next : event => {
                    if(event.type === HttpEventType.Response){
                    console.log('[Incoming Request]');
                    console.log(event.status);
                    console.log(event.body);
                    }
            }
        })
     )
}