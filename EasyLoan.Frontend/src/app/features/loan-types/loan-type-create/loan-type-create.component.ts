import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { LoanTypeFormComponent } from '../loan-type-form/loan-type-form.component';
import { LoanTypeService } from '../../../services/loan-type.service';

@Component({
  selector: 'app-loan-type-create',
  standalone: true,
  imports: [CommonModule, LoanTypeFormComponent],
  templateUrl: './loan-type-create.component.html',
  styleUrl: './loan-type-create.component.css',
})
export class LoanTypeCreateComponent {
  constructor(
    private loanTypeService: LoanTypeService,
    private router: Router
  ) { }

  onFormSubmit(formData: any): void {
    this.loanTypeService.createLoanType(formData).subscribe({
      next: () => {
        this.router.navigate(['/admin/loan-types']);
      },
      error: (err) => {
        console.error('Error creating loan type:', err);
        // Error will be handled by the form component
      }
    });
  }

  onFormCancel(): void {
    this.router.navigate(['/admin/loan-types']);
  }
}
