import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token.service';

export const roleGuard: CanActivateFn = (route, state) => {
    const tokenService = inject(TokenService);
    const router = inject(Router);

    const user = tokenService.getCurrentUser();
    if (!user) {
        router.navigate(['/auth/login']);
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
    router.navigate(['/unauthorized']);
    return false;
};
