import { Routes } from '@angular/router';
import { canDeactivateLoanType } from '../guards/can-deactivate-loan-type.guard';

export const LOAN_TYPES_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('../features/loan-types/loan-types-list/loan-types-list.component').then(m => m.LoanTypesListComponent)
    },
    {
        path: 'create',
        loadComponent: () => import('../features/loan-types/loan-type-create/loan-type-create.component').then(m => m.LoanTypeCreateComponent),
        canDeactivate: [canDeactivateLoanType]
    },
    {
        path: ':id/edit',
        loadComponent: () => import('../features/loan-types/loan-type-update/loan-type-update.component').then(m => m.LoanTypeUpdateComponent),
        canDeactivate: [canDeactivateLoanType]
    }
];
