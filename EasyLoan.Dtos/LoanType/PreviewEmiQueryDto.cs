using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.Loan
{
    public class PreviewEmiQueryDto
    {
        [Required(ErrorMessage = "Loan amount is required.")]
        [RegularExpression(
            @"^\d+(\.\d{1,2})?$",
            ErrorMessage = "Loan amount can have a maximum of 2 decimal places."
        )]
        [Range(1, double.MaxValue, ErrorMessage = "Loan amount must be at least ₹1.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Tenure is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Tenure must be at least 1 month.")]
        public int TenureInMonths { get; set; }
    }
}
