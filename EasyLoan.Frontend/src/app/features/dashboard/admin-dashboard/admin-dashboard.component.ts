import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { AdminService } from '../../../services/dashboard.service';
import { AdminDashboardResponse } from '../../../models/dashboard.models';

@Component({
    selector: 'app-admin-dashboard',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatButtonModule
    ],
    templateUrl: './admin-dashboard.component.html',
    styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
    dashboardData = signal<AdminDashboardResponse | null>(null);
    loading = signal<boolean>(true);
    error = signal<string | null>(null);

    private destroyRef = inject(DestroyRef);

    constructor(private adminService: AdminService) { }

    ngOnInit(): void {
        this.loadDashboard();
    }

    loadDashboard(): void {
        this.loading.set(true);
        this.error.set(null);

        this.adminService.getAdminDashboard()
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
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
