import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../../../shared/services/admin.service';
import { AdminDashboardResponse } from '../../../../shared/models/admin.models';

@Component({
    selector: 'app-admin-dashboard',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './admin-dashboard.component.html',
    styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
    dashboardData = signal<AdminDashboardResponse | null>(null);
    loading = signal<boolean>(true);
    error = signal<string | null>(null);

    constructor(private adminService: AdminService) { }

    ngOnInit(): void {
        this.loadDashboard();
    }

    loadDashboard(): void {
        this.loading.set(true);
        this.error.set(null);

        this.adminService.getAdminDashboard().subscribe({
            next: (data) => {
                this.dashboardData.set(data);
                this.loading.set(false);
            },
            error: (err) => {
                this.error.set('Failed to load dashboard data. Please try again.');
                this.loading.set(false);
                console.error('Error loading dashboard:', err);
            }
        });
    }
}
