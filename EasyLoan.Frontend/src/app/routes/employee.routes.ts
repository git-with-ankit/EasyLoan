import { Routes } from '@angular/router';

export const EMPLOYEE_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () =>
            import('../layouts/employee-layout/employee-layout.component').then(
                (c) => c.EmployeeLayoutComponent
            ),
        children: [
            { path: '', redirectTo: 'assigned-applications', pathMatch: 'full' },
            {
                path: 'assigned-applications',
                loadComponent: () => import('../features/applications/assigned-applications/assigned-applications.component').then(m => m.AssignedApplicationsComponent)
            },
            {
                path: 'assigned-applications/:applicationNumber/review',
                loadComponent: () => import('../features/applications/application-review/application-review.component').then(m => m.ApplicationReviewComponent)
            },
            {
                path: 'profile',
                loadComponent: () => import('../features/profile/profile.component').then(m => m.ProfileComponent)
            }
        ]
    },
];
