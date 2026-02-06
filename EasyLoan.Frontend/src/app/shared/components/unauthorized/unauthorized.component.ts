import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-unauthorized',
  imports: [MatCardModule, MatButtonModule, RouterLink],
  template: `
    <div class="error-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>403 - Unauthorized</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>You don't have permission to access this page.</p>
          <button mat-raised-button color="primary" routerLink="/landing">
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
export class UnauthorizedComponent { }
