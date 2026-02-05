import { Routes } from '@angular/router';

export const CUSTOMER_ROUTES: Routes = [
    {
        path: 'dashboard',
        loadComponent: () =>
            import('../dashboard/layouts/customer-layout/customer-layout.component').then(
                (c) => c.CustomerLayoutComponent
            ),
        children: [
            { path: '', redirectTo: 'overdue-emis', pathMatch: 'full' },
            {
                path: 'overdue-emis',
                loadComponent: () => import('../dashboard/pages/overdue-emis/overdue-emis.component').then(m => m.OverdueEmis)
            },
            {
                path: 'loans',
                loadComponent: () => import('../dashboard/pages/loans-list/loans-list.component').then(m => m.LoansListComponent)
            },
            {
                path: 'applications',
                loadComponent: () => import('../dashboard/pages/applications-list/applications-list.component').then(m => m.ApplicationsListComponent)
            },
            {
                path: 'apply-loan',
                loadComponent: () => import('../dashboard/pages/create-application/create-application.component').then(m => m.CreateApplicationComponent)
            },
            {
                path: 'profile',
                loadComponent: () => import('../dashboard/pages/profile/profile.component').then(m => m.ProfileComponent)
            }
        ]
    },
];
