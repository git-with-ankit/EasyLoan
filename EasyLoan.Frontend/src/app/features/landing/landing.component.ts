import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

@Component({
    standalone: true,
    selector: 'app-landing',
    templateUrl: './landing.component.html',
    styleUrl: './landing.component.css',
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatMenuModule,
        MatIconModule
    ]
})
export class LandingComponent {
    constructor(private router: Router) { }

    navigateToRegister(): void {
        this.router.navigate(['/auth/register']);
    }

    navigateToCustomerLogin(): void {
        this.router.navigate(['/auth/customer/login']);
    }

    navigateToEmployeeLogin(): void {
        this.router.navigate(['/auth/employee/login']);
    }
}
