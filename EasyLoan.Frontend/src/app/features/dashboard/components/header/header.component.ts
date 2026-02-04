import { Component, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TokenService } from '../../../../shared/services/token.service';

@Component({
    selector: 'app-header',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './header.component.html',
    styleUrl: './header.component.css'
})
export class HeaderComponent {
    @Output() signout = new EventEmitter<void>();

    userEmail = signal('');

    constructor(
        private tokenService: TokenService,
        private router: Router
    ) {
        const user = this.tokenService.getCurrentUser();
        if (user) {
            this.userEmail.set(user.email);
        }
    }

    onProfile() {
        const user = this.tokenService.getCurrentUser();
        console.log('Header - Current User:', user);
        if (user) {
            console.log('Header - Role:', user.role);

            // Navigate based on role
            if (user.role === 'Customer') {
                this.router.navigate(['/customer/dashboard/profile']);
            } else if (user.role === 'Manager' || user.role === 'Admin') {
                this.router.navigate(['/employee/dashboard/profile']);
            }
        }
    }

    onSignout() {
        this.signout.emit();
    }
}
