import { Routes } from '@angular/router';

export const EMPLOYEE_ROUTES: Routes = [
    {
        path: 'dashboard',
        loadComponent: () =>
            import('../dashboard/pages/employee-dashboard/employee-dashboard.component').then(
                (c) => c.EmployeeDashboardComponent
            ),
    },
];
