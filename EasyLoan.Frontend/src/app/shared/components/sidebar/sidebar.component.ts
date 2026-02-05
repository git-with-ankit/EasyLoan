import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './sidebar.component.html',
    styleUrl: './sidebar.component.css'
})
export class SidebarComponent {
    @Input() activeRoute: string = '';
    @Input() role: string = 'Customer';

    get navItems() {
        // Handle Admin role
        if (this.role === 'Admin') {
            return [
                { path: '/admin/dashboard', label: 'Dashboard' },
                { path: '/admin/loan-types', label: 'Loan Types' },
                { path: '/admin/assigned-applications', label: 'All Applications' },
                { path: '/admin/create-manager', label: 'Create Manager' }
            ];
        }

        // Handle Manager role (no sidebar items needed, but kept for future use)
        if (this.role === 'Manager') {
            return [];
        }

        // Default to Customer items
        return [
            { path: '/customer/overdue-emis', label: 'Overdue EMIs' },
            { path: '/customer/loans', label: 'My Loans' },
            { path: '/customer/applications', label: 'Applications' },
            { path: '/customer/apply-loan', label: 'Apply for Loan' }
        ];
    }
}
