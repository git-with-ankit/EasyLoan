import { CanDeactivateFn } from '@angular/router';
import { ProfileComponent } from '../features/profile/profile.component';

export const canDeactivateProfile: CanDeactivateFn<ProfileComponent> = (component) => {
    // Allow navigation if not editing, form is pristine, or if save is in progress
    if (!component.isEditing() || !component.profileForm.dirty || component.isSaving()) {
        return true;
    }

    // Warn user about unsaved profile changes
    return confirm(
        'You have unsaved profile changes. Are you sure you want to leave?'
    );
};
