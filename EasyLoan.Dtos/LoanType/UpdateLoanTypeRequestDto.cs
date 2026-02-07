using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanType
{
    public class UpdateLoanTypeRequestDto
    {
        [RegularExpression(@"^\d{0,3}(\.\d{0,2})?$", ErrorMessage = "Interest rate must have max 3 digits before and 2 after decimal")]
        [Range(0.01, 100, ErrorMessage = "Interest rate must be between 0.01% and 100%.")]
        public decimal? InterestRate { get; set; }

        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Amount should have Max 2 decimals")]
        [Range(1, double.MaxValue, ErrorMessage = "Minimum amount must be at least ₹1.")]
        public decimal? MinAmount { get; set; }

        [Required(ErrorMessage = "Maximum tenure is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum tenure must be at least 1 month and within your life span to pay")]
        public int? MaxTenureInMonths { get; set; }
    }
}
