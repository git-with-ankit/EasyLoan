import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';
import { publicPageGuard } from './guards/public-page.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'landing',
    pathMatch: 'full',
  },

  {
    path: 'landing',
    canActivate: [publicPageGuard],
    loadComponent: () =>
      import('./features/landing/landing.component').then(c => c.LandingComponent),
  },

  {
    path: 'auth',
    loadChildren: () =>
      import('./routes/auth.routes').then(m => m.AUTH_ROUTES),
  },

  {
    path: 'customer',
    canActivate: [authGuard, roleGuard],
    canActivateChild: [authGuard, roleGuard],
    data: { roles: ['Customer'] },
    loadChildren: () =>
      import('./routes/customer.routes').then(m => m.CUSTOMER_ROUTES),
  },

  {
    path: 'employee',
    canActivate: [authGuard, roleGuard],
    canActivateChild: [authGuard, roleGuard],
    data: { roles: ['Manager', 'Admin'] },
    loadChildren: () =>
      import('./routes/employee.routes').then(m => m.EMPLOYEE_ROUTES),
  },

  {
    path: 'admin',
    canActivate: [authGuard, roleGuard],
    canActivateChild: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    loadChildren: () =>
      import('./routes/admin.routes').then(m => m.ADMIN_ROUTES),
  },

  {
    path: '**',
    loadComponent: () =>
      import('./shared/components/not-found/not-found.component').then(c => c.NotFoundComponent),
  },
];
