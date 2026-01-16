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
        [Required]
        [Precision(5, 2)]
        [Range(0.01, 100)]
        public decimal InterestRate { get; set; }

        [Required]
        [Precision(18, 2)]
        [Range(1, double.MaxValue)]
        public decimal MinAmount { get; set; }

        [Required]
        [Range(1, 480)]
        public int MaxTenureInMonths { get; set; }
    }
}
