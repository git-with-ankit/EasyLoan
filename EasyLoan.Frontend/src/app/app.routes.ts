import { Routes } from '@angular/router';
import { authGuard } from './shared/guards/auth.guard';
import { roleGuard } from './shared/guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'auth/login',
    pathMatch: 'full',
  },

  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES),
  },

  {
    path: 'customer',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Customer'] },
    loadChildren: () =>
      import('./features/customer/customer.routes').then(m => m.CUSTOMER_ROUTES),
  },

  {
    path: 'employee',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Manager', 'Admin'] },
    loadChildren: () =>
      import('./features/employee/employee.routes').then(m => m.EMPLOYEE_ROUTES),
  },

  {
    path: 'admin',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    loadChildren: () =>
      import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES),
  },

  {
    path: 'unauthorized',
    loadComponent: () =>
      import('./shared/pages/unauthorized.component').then(c => c.UnauthorizedComponent),
  },

  {
    path: '**',
    loadComponent: () =>
      import('./shared/pages/not-found.component').then(c => c.NotFoundComponent),
  },
];
