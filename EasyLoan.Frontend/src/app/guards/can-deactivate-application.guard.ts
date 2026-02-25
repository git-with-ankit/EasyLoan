import { CanDeactivateFn } from '@angular/router';
import { CreateApplicationComponent } from '../features/applications/create-application/create-application.component';

export const canDeactivateApplication: CanDeactivateFn<CreateApplicationComponent> = (component) => {
    // Allow navigation if form is pristine or if submission is in progress/completed
    if (!component.applicationForm.dirty || component.isSubmitting()) {
        return true;
    }

    // Warn user about unsaved changes
    return confirm(
        'You have unsaved changes in your loan application. Are you sure you want to leave?'
    );
};
