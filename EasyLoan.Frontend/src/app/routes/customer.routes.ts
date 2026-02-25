import { Routes } from '@angular/router';
import { canDeactivateApplication } from '../guards/can-deactivate-application.guard';
import { canDeactivateProfile } from '../guards/can-deactivate-profile.guard';

export const CUSTOMER_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () =>
            import('../layouts/customer-layout/customer-layout.component').then(
                (c) => c.CustomerLayoutComponent
            ),
        children: [
            { path: '', redirectTo: 'overdue-emis', pathMatch: 'full' },
            {
                path: 'overdue-emis',
                loadComponent: () => import('../features/loans/overdue-emis/overdue-emis.component').then(m => m.OverdueEmis)
            },
            {
                path: 'loans',
                loadComponent: () => import('../features/loans/loans-list/loans-list.component').then(m => m.LoansListComponent)
            },
            {
                path: 'applications',
                loadComponent: () => import('../features/applications/applications-list/applications-list.component').then(m => m.ApplicationsListComponent)
            },
            {
                path: 'apply-loan',
                loadComponent: () => import('../features/applications/create-application/create-application.component').then(m => m.CreateApplicationComponent),
                canDeactivate: [canDeactivateApplication]
            },
            {
                path: 'profile',
                loadComponent: () => import('../features/profile/profile.component').then(m => m.ProfileComponent),
                canDeactivate: [canDeactivateProfile]
            }
        ]
    },
];
