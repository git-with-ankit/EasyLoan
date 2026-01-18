using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.Loan
{
    public class PreviewEmiQueryDto
    {
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Tenure must be between 1 and 480 months.")]
        public int TenureInMonths { get; set; }
    }
}
