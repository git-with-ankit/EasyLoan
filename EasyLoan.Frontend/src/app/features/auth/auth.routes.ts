import { Routes } from '@angular/router';
import { publicPageGuard } from '../../guards/public-page.guard';

export const AUTH_ROUTES: Routes = [
  {
    path: 'customer/login',
    canActivate: [publicPageGuard],
    loadComponent: () =>
      import('./login/login.component').then(c => c.LoginComponent),
    data: { type: 'Customer' }
  },
  {
    path: 'employee/login',
    canActivate: [publicPageGuard],
    loadComponent: () =>
      import('./login/login.component').then(c => c.LoginComponent),
    data: { type: 'Employee' }
  },
  {
    path: 'register',
    canActivate: [publicPageGuard],
    loadComponent: () =>
      import('./register/register.component').then(c => c.RegisterComponent),
  },
];
