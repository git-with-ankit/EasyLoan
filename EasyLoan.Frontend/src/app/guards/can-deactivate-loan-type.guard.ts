import { CanDeactivateFn } from '@angular/router';
import { LoanTypeFormComponent } from '../features/loan-types/loan-type-form/loan-type-form.component';

export const canDeactivateLoanType: CanDeactivateFn<LoanTypeFormComponent> = (component) => {
    // Allow navigation if form is pristine or if submission is in progress
    if (!component.loanTypeForm.dirty || component.submitting()) {
        return true;
    }

    // Warn user about unsaved changes to loan type
    return confirm(
        'You have unsaved changes to the loan type. Are you sure you want to leave?'
    );
};
