import { Routes } from '@angular/router';

export const LOAN_TYPES_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('../features/loan-types/loan-types-list/loan-types-list.component').then(m => m.LoanTypesListComponent)
    },
    {
        path: 'create',
        loadComponent: () => import('../features/loan-types/loan-type-create/loan-type-create.component').then(m => m.LoanTypeCreateComponent)
    },
    {
        path: ':id/edit',
        loadComponent: () => import('../features/loan-types/loan-type-update/loan-type-update.component').then(m => m.LoanTypeUpdateComponent)
    }
];
