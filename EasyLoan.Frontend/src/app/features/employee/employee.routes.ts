import { Routes } from '@angular/router';

export const EMPLOYEE_ROUTES: Routes = [
    {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
    },
    {
        path: 'dashboard',
        loadComponent: () =>
            import('../dashboard/layouts/employee-layout/employee-layout.component').then(
                (c) => c.EmployeeLayoutComponent
            ),
        children: [
            { path: '', redirectTo: 'assigned-applications', pathMatch: 'full' },
            {
                path: 'assigned-applications',
                loadComponent: () => import('../dashboard/pages/assigned-applications/assigned-applications.component').then(m => m.AssignedApplicationsComponent)
            },
            {
                path: 'assigned-applications/:applicationNumber/review',
                loadComponent: () => import('../dashboard/pages/application-review/application-review.component').then(m => m.ApplicationReviewComponent)
            },
            {
                path: 'profile',
                loadComponent: () => import('../dashboard/pages/profile/profile.component').then(m => m.ProfileComponent)
            }
        ]
    },
];
