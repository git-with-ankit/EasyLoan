import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { LoanTypeService } from '../../../../shared/services/loan-type.service';
import { LoanType } from '../../../../shared/models/loan-type.models';

@Component({
  selector: 'app-loan-types-list',
  imports: [CommonModule],
  templateUrl: './loan-types-list.component.html',
  styleUrl: './loan-types-list.component.css',
})
export class LoanTypesListComponent implements OnInit {
  loanTypes = signal<LoanType[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

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

    this.loanTypeService.getLoanTypes().subscribe({
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
    this.router.navigate(['/admin/dashboard/loan-types/create']);
  }

  navigateToEdit(id: string): void {
    this.router.navigate([`/admin/dashboard/loan-types/${id}/edit`]);
  }
}
