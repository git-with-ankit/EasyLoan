import { Component, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
import { TokenService } from '../services/token.service';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-not-found',
  imports: [MatCardModule, MatButtonModule, CommonModule],
  template: `
    <div class="error-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>404 - Page Not Found</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>The page you're looking for doesn't exist.</p>
          <button mat-raised-button color="primary" (click)="goHome()">
            Go to Home
          </button>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .error-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: #f5f5f5;
    }
    mat-card {
      text-align: center;
      padding: 40px;
    }
  `]
})
export class NotFoundComponent implements OnInit {
  homeRoute = '/auth/login';

  constructor(
    private tokenService: TokenService,
    private router: Router
  ) { }

  ngOnInit() {
    // Determine home route based on authentication and role
    const user = this.tokenService.getCurrentUser();
    if (user) {
      if (user.role === 'Customer') {
        this.homeRoute = '/customer/dashboard';
      } else if (user.role === 'Manager' || user.role === 'Admin') {
        this.homeRoute = '/employee/dashboard';
      }
    }
  }

  goHome() {
    this.router.navigate([this.homeRoute]);
  }
}
