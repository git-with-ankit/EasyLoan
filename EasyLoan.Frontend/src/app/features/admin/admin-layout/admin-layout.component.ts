import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar.component';
import { HeaderComponent } from '../../../shared/components/header/header.component';
import { AuthService } from '../../auth/auth.service';

@Component({
    selector: 'app-admin-layout',
    standalone: true,
    imports: [CommonModule, RouterModule, SidebarComponent, HeaderComponent],
    templateUrl: './admin-layout.component.html',
    styleUrl: './admin-layout.component.css'
})
export class AdminLayoutComponent {
    constructor(
        private authService: AuthService,
        private router: Router
    ) { }

    onSignout() {
        this.authService.logout();
        this.router.navigate(['/auth/employee/login']);
    }
}

