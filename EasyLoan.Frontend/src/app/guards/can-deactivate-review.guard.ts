import { CanDeactivateFn } from '@angular/router';
import { ApplicationReviewComponent } from '../features/applications/application-review/application-review.component';

export const canDeactivateReview: CanDeactivateFn<ApplicationReviewComponent> = (component) => {
    // Allow navigation if form is pristine, not pending, or if submission is in progress
    if (!component.reviewForm.dirty || !component.isPending() || component.isLoading()) {
        return true;
    }

    // Warn user about unsaved review comments
    return confirm(
        'You have unsaved review comments. Are you sure you want to leave?'
    );
};
