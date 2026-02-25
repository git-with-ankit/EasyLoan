import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoanTypeService } from '../../../services/loan-type.service';
import { LoanType } from '../../../models/loan-type.models';

@Component({
  selector: 'app-loan-types-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './loan-types-list.component.html',
  styleUrl: './loan-types-list.component.css',
})
export class LoanTypesListComponent implements OnInit {
  loanTypes = signal<LoanType[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  private destroyRef = inject(DestroyRef);

  constructor(
    private loanTypeService: LoanTypeService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadLoanTypes();
  }

  loadLoanTypes(): void {
    this.loading.set(true);
    this.error.set(null);

    this.loanTypeService.getLoanTypes()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          console.log('Loan types received:', data);
          this.loanTypes.set(data);
          this.loading.set(false);
          console.log('Loading set to false, loanTypes count:', this.loanTypes().length);
        },
        error: (err) => {
          this.error.set('Failed to load loan types. Please try again.');
          this.loading.set(false);
          console.error('Error loading loan types:', err);
        }
      });
  }

  navigateToCreate(): void {
    this.router.navigate(['/admin/loan-types/create']);
  }

  navigateToEdit(id: string): void {
    this.router.navigate([`/admin/loan-types/${id}/edit`]);
  }
}
