import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { map, catchError, of } from 'rxjs';


export const publicPageGuard: CanActivateFn = (route, state) => {
    const userService = inject(UserService);
    const router = inject(Router);

    const currentUser = userService.currentUser();
    if (currentUser) {
        redirectToDashboard(currentUser.role, router);
        return false;
    }

    // Try to load user from cookie
    return userService.loadCurrentUser().pipe(
        map(user => {
            if (user) {
                redirectToDashboard(user.role, router);
                return false;
            }
            return true;
        }),
        catchError(() => {
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
            break;
    }
}
