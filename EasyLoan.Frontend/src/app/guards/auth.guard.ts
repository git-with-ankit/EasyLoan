import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { map } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
    const userService = inject(UserService);
    const router = inject(Router);

    // If user is already loaded, allow access
    if (userService.currentUser() !== null) {
        return true;
    }

    // Try to load user from cookie
    return userService.loadCurrentUser().pipe(
        map(user => {
            if (user) {
                return true;
            }

            // No user, redirect to login
            router.navigate(['/auth/login'], {
                queryParams: { returnUrl: state.url }
            });
            return false;
        })
    );
};
