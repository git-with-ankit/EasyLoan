import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { map, catchError, of } from 'rxjs';

/**
 * Guard to prevent authenticated users from accessing public pages (landing, login, register).
 * If user is logged in, redirects to their role-based dashboard.
 * If user is not logged in, allows access to the public page.
 */
export const publicPageGuard: CanActivateFn = (route, state) => {
    const userService = inject(UserService);
    const router = inject(Router);

    // If user is already loaded, redirect to appropriate dashboard
    const currentUser = userService.currentUser();
    if (currentUser) {
        redirectToDashboard(currentUser.role, router);
        return false;
    }

    // Try to load user from cookie
    return userService.loadCurrentUser().pipe(
        map(user => {
            if (user) {
                // User is authenticated, redirect to their dashboard
                redirectToDashboard(user.role, router);
                return false;
            }
            // No user, allow access to public page
            return true;
        }),
        catchError(() => {
            // If there's any error checking auth, allow access to public page
            return of(true);
        })
    );
};

function redirectToDashboard(role: string, router: Router): void {
    switch (role) {
        case 'Customer':
            router.navigate(['/customer']);
            break;
        case 'Manager':
            router.navigate(['/employee']);
            break;
        case 'Admin':
            router.navigate(['/admin']);
            break;
        default:
            // Unknown role - do nothing
            break;
    }
}
