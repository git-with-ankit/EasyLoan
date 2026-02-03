import { Routes } from '@angular/router';

export const ADMIN_ROUTES: Routes = [
    {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
    },
    {
        path: 'dashboard',
        loadComponent: () =>
            import('../dashboard/layouts/admin-layout/admin-layout.component').then(
                (c) => c.AdminLayoutComponent
            ),
        children: [
            {
                path: '',
                redirectTo: 'loan-types',
                pathMatch: 'full'
            },
            {
                path: 'loan-types',
                loadChildren: () => import('../loan-types/loan-types.routes').then(m => m.LOAN_TYPES_ROUTES)
            },
            {
                path: 'assigned-applications',
                loadComponent: () => import('../dashboard/pages/assigned-applications/assigned-applications.component').then(m => m.AssignedApplicationsComponent)
            },
            {
                path: 'assigned-applications/:applicationNumber/review',
                loadComponent: () => import('../dashboard/pages/application-review/application-review.component').then(m => m.ApplicationReviewComponent)
            },
            {
                path: 'create-manager',
                loadComponent: () => import('../auth/register/register.component').then(m => m.RegisterComponent)
            },
            {
                path: 'profile',
                loadComponent: () => import('../dashboard/pages/profile/profile.component').then(m => m.ProfileComponent)
            }
        ]
    },
];

