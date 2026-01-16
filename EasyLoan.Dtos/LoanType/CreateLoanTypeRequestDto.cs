using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.Dtos.LoanType
{
    public class CreateLoanTypeRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

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
