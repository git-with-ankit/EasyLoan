using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.LoanApplication
{
    public class ReviewLoanApplicationRequestDto
    {
        [Required(ErrorMessage = "Decision is required.")]
        public bool IsApproved { get; set; }

        [MaxLength(1000, ErrorMessage = "Manager comments cannot exceed 1000 characters.")]
        public string? ManagerComments { get; set; }

        [Required(ErrorMessage = "Approved amount is required.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Amount should have Max 2 decimals")]
        [Range(0, double.MaxValue, ErrorMessage = "Approved amount must be a positive number.")]
        public decimal ApprovedAmount { get; set; }
    }
}
