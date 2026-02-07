using System;
using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.LoanApplication
{
    public class CreateLoanApplicationRequestDto
    {
        [Required(ErrorMessage = "Loan type is required.")]
        public Guid LoanTypeId { get; set; }

        [Required(ErrorMessage = "Requested amount is required.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Amount should have Max 2 decimals")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Requested amount must be a positive number.")]
        public decimal RequestedAmount { get; set; }

        [Required(ErrorMessage = "Requested tenure is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Requested tenure must be at least 1 month.")]
        public int RequestedTenureInMonths { get; set; }
    }
}
