using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.Dtos.LoanType
{
    public class LoanTypeRequestDto
    {
        [Required(ErrorMessage = "Loan type name is required.")]
        [MaxLength(100, ErrorMessage = "Loan type name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Interest rate is required.")]
        [RegularExpression(@"^\d{0,3}(\.\d{0,2})?$", ErrorMessage = "Interest rate must have max 3 digits before and 2 after decimal")]
        [Range(0.01, 100, ErrorMessage = "Interest rate must be between 0.01% and 100%.")]
        public decimal InterestRate { get; set; }

        [Required(ErrorMessage = "Minimum amount is required.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Amount should have Max 2 decimals")]
        [Range(1, double.MaxValue, ErrorMessage = "Minimum amount must be at least ₹1.")]
        public decimal MinAmount { get; set; }

        [Required(ErrorMessage = "Maximum tenure is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum tenure must be at least 1 month and within your life span to pay")]
        public int MaxTenureInMonths { get; set; }
    }
}
