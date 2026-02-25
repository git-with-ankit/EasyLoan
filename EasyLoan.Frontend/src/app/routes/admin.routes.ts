import { Routes } from '@angular/router';
import { canDeactivateReview } from '../guards/can-deactivate-review.guard';
import { canDeactivateProfile } from '../guards/can-deactivate-profile.guard';

export const ADMIN_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () =>
            import('../layouts/admin-layout/admin-layout.component').then(
                (c) => c.AdminLayoutComponent
            ),
        children: [
            {
                path: '',
                redirectTo: 'dashboard',
                pathMatch: 'full'
            },
            {
                path: 'dashboard',
                loadComponent: () => import('../features/dashboard/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
            },
            {
                path: 'loan-types',
                loadChildren: () => import('./loan-types.routes').then(m => m.LOAN_TYPES_ROUTES)
            },
            {
                path: 'assigned-applications',
                loadComponent: () => import('../features/applications/assigned-applications/assigned-applications.component').then(m => m.AssignedApplicationsComponent)
            },
            {
                path: 'assigned-applications/:applicationNumber/review',
                loadComponent: () => import('../features/applications/application-review/application-review.component').then(m => m.ApplicationReviewComponent),
                canDeactivate: [canDeactivateReview]
            },
            {
                path: 'create-manager',
                loadComponent: () => import('../features/auth/register/register.component').then(m => m.RegisterComponent)
            },
            {
                path: 'profile',
                loadComponent: () => import('../features/profile/profile.component').then(m => m.ProfileComponent),
                canDeactivate: [canDeactivateProfile]
            }
        ]
    },
];

