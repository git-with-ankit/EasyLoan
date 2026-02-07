using System;
using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.LoanPayment
{
    public class MakeLoanPaymentRequestDto
    {
        [Required(ErrorMessage = "Payment amount is required.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Amount should have Max 2 decimals")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be at least 0.01 and should not exceed the amount range.")]
        public decimal Amount { get; set; }
    }
}
