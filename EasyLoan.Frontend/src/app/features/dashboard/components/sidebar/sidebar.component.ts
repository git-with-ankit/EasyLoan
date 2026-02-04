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
                { path: '/admin/dashboard/overview', label: 'Dashboard', icon: '📊' },
                { path: '/admin/dashboard/loan-types', label: 'Loan Types', icon: '💼' },
                { path: '/admin/dashboard/assigned-applications', label: 'All Applications', icon: '📂' },
                { path: '/admin/dashboard/create-manager', label: 'Create Manager', icon: '👥' }
            ];
        }

        // Handle Manager role (no sidebar items needed, but kept for future use)
        if (this.role === 'Manager') {
            return [];
        }

        // Default to Customer items
        return [
            { path: '/customer/dashboard/emi-payments', label: 'Overdue EMIs', icon: '💳' },
            { path: '/customer/dashboard/loans', label: 'My Loans', icon: '💰' },
            { path: '/customer/dashboard/applications', label: 'Applications', icon: '📝' },
            { path: '/customer/dashboard/apply-loan', label: 'Apply for Loan', icon: '✨' }
        ];
    }
}
