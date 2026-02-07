import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserService } from '../services/user.service';

export const roleGuard: CanActivateFn = (route, state) => {
    const userService = inject(UserService);
    const router = inject(Router);

    const user = userService.currentUser();
    if (!user) {
        router.navigate(['/landing']);
        return false;
    }

    const requiredRoles = route.data['roles'] as string[];
    if (!requiredRoles || requiredRoles.length === 0) {
        return true;
    }

    if (requiredRoles.includes(user.role)) {
        return true;
    }

    // User doesn't have required role
    router.navigate(['/landing']);
    return false;
};
