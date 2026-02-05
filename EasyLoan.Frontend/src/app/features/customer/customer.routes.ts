import { Routes } from '@angular/router';

export const CUSTOMER_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () =>
            import('./customer-layout/customer-layout.component').then(
                (c) => c.CustomerLayoutComponent
            ),
        children: [
            { path: '', redirectTo: 'overdue-emis', pathMatch: 'full' },
            {
                path: 'overdue-emis',
                loadComponent: () => import('../loans/overdue-emis/overdue-emis.component').then(m => m.OverdueEmis)
            },
            {
                path: 'loans',
                loadComponent: () => import('../loans/loans-list/loans-list.component').then(m => m.LoansListComponent)
            },
            {
                path: 'applications',
                loadComponent: () => import('../applications/applications-list/applications-list.component').then(m => m.ApplicationsListComponent)
            },
            {
                path: 'apply-loan',
                loadComponent: () => import('../applications/create-application/create-application.component').then(m => m.CreateApplicationComponent)
            },
            {
                path: 'profile',
                loadComponent: () => import('../profile/profile.component').then(m => m.ProfileComponent)
            }
        ]
    },
];
