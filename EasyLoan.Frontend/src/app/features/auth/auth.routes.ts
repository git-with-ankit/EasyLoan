import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: 'customer/login',
    loadComponent: () =>
      import('./login/login.component').then(c => c.LoginComponent),
    data: { type: 'Customer' }
  },
  {
    path: 'employee/login',
    loadComponent: () =>
      import('./login/login.component').then(c => c.LoginComponent),
    data: { type: 'Employee' }
  },
  {
    path: 'login',
    redirectTo: 'customer/login',
    pathMatch: 'full',
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./register/register.component').then(c => c.RegisterComponent),
  },
];
