// import { Component, Input, Output, EventEmitter } from '@angular/core';
// import { CommonModule } from '@angular/common';
// import { DueEmiResponse } from '../../../../shared/models/loan.models';
// import { LoanService } from '../../../../shared/services/loan.service';

// @Component({
//     selector: 'app-emi-payment-card',
//     standalone: true,
//     imports: [CommonModule],
//     templateUrl: './emi-payment-card.component.html',
//     styleUrl: './emi-payment-card.component.css'
// })
// export class EmiPaymentCardComponent {
//     @Input() emi!: DueEmiResponse;
//     @Input() loanNumber!: string;
//     @Output() paymentSuccess = new EventEmitter<void>();
//     @Output() cancel = new EventEmitter<void>();

//     isProcessing = false;
//     errorMessage = '';
//     successMessage = '';

//     constructor(private loanService: LoanService) { }

//     get totalAmount(): number {
//         return this.emi.remainingEmiAmount + this.emi.penaltyAmount;
//     }

//     onPay(): void {
//         this.isProcessing = true;
//         this.errorMessage = '';
//         this.successMessage = '';

//         this.loanService.makePayment(this.loanNumber, this.totalAmount).subscribe({
//             next: (response) => {
//                 this.isProcessing = false;
//                 this.successMessage = 'Payment successful!';
//                 setTimeout(() => {
//                     this.paymentSuccess.emit();
//                 }, 1500);
//             },
//             error: (error) => {
//                 this.isProcessing = false;
//                 // this.errorMessage = error.message || 'Payment failed. Please try again.';
//             }
//         });
//     }

//     onCancel(): void {
//         this.cancel.emit();
//     }
// }
