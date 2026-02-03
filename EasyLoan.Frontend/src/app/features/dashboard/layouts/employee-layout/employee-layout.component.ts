import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { HeaderComponent } from '../../components/header/header.component';
import { AuthService } from '../../../auth/auth.service';

@Component({
    selector: 'app-employee-layout',
    standalone: true,
    imports: [CommonModule, RouterModule, HeaderComponent],
    templateUrl: './employee-layout.component.html',
    styleUrl: './employee-layout.component.css'
})
export class EmployeeLayoutComponent implements OnInit {
    userRole: string = 'Manager';

    constructor(
        private authService: AuthService,
        private router: Router
    ) { }

    ngOnInit() {
        const user = this.authService.getCurrentUser();
        if (user) {
            this.userRole = user.role;
        }
    }

    onSignout() {
        this.authService.logout();
        this.router.navigate(['/auth/login']);
    }
}
