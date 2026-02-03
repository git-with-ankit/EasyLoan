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

    navItems = [
        { path: '/customer/dashboard/emi-payments', label: 'Overdue EMIs', icon: '💳' },
        { path: '/customer/dashboard/loans', label: 'My Loans', icon: '💰' },
        { path: '/customer/dashboard/applications', label: 'Applications', icon: '📝' },
        { path: '/customer/dashboard/apply-loan', label: 'Apply for Loan', icon: '✨' }
    ];
}
