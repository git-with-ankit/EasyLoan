import { Component, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { UserService } from '../../../services/user.service';
import { ChangePasswordComponent } from '../../../features/auth/change-password/change-password.component';

@Component({
    selector: 'app-header',
    standalone: true,
    imports: [CommonModule, MatDialogModule],
    templateUrl: './header.component.html',
    styleUrl: './header.component.css'
})
export class HeaderComponent {
    @Output() signout = new EventEmitter<void>();

    userEmail = signal('');

    constructor(
        private userService: UserService,
        private router: Router,
        private dialog: MatDialog
    ) {
        const user = this.userService.currentUser();
        if (user) {
            this.userEmail.set(user.email);
        }
    }

    onLogoClick() {
        const user = this.userService.currentUser();
        if (user) {
            // Navigate explicitly based on role requirements
            if (user.role === 'Customer') {
                this.router.navigate(['/customer/overdue-emis']);
            } else if (user.role === 'Manager') {
                this.router.navigate(['/employee/assigned-applications']);
            } else if (user.role === 'Admin') {
                this.router.navigate(['/admin/dashboard']);
            }
        }
    }

    onProfile() {
        const user = this.userService.currentUser();
        if (user) {

            // Navigate based on role
            if (user.role === 'Customer') {
                this.router.navigate(['/customer/profile']);
            } else if (user.role === 'Manager') {
                this.router.navigate(['/employee/profile']);
            } else if (user.role === 'Admin') {
                this.router.navigate(['/admin/profile']);
            }
        }
    }

    openChangePasswordDialog() {
        this.dialog.open(ChangePasswordComponent, {
            width: '450px',
            disableClose: false
        });
    }

    onSignout() {
        this.signout.emit();
    }
}
