import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-not-found',
  imports: [MatCardModule, MatButtonModule, MatIconModule, CommonModule, RouterLink],
  templateUrl: './not-found.component.html',
  styleUrl: './not-found.component.css'
})
export class NotFoundComponent {
  homeRoute = '/';

  constructor(
    private router: Router
  ) { }

  goHome() {
    this.router.navigate([this.homeRoute]);
  }
}
