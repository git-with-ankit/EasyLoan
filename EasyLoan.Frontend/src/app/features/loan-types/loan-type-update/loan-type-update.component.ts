import { Component, OnInit, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { LoanTypeFormComponent } from '../loan-type-form/loan-type-form.component';
import { LoanTypeService } from '../../../services/loan-type.service';

@Component({
  selector: 'app-loan-type-update',
  standalone: true,
  imports: [CommonModule, LoanTypeFormComponent],
  templateUrl: './loan-type-update.component.html',
  styleUrl: './loan-type-update.component.css',
})
export class LoanTypeUpdateComponent implements OnInit {
  loanTypeId: string = '';

  private destroyRef = inject(DestroyRef);

  constructor(
    private loanTypeService: LoanTypeService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.loanTypeId = this.route.snapshot.paramMap.get('id') || '';
  }

  onFormSubmit(formData: any): void {
    this.loanTypeService.updateLoanType(this.loanTypeId, formData)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.router.navigate(['/admin/loan-types']);
        },
        error: (err) => {
          console.error('Error updating loan type:', err);
          // Error will be handled by the form component
        }
      });
  }

  onFormCancel(): void {
    this.router.navigate(['/admin/loan-types']);
  }
}
