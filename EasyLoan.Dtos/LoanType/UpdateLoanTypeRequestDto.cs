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
        [Precision(5, 2)]
        [Range(0.01, 100)]
        public decimal? InterestRate { get; set; }

        [Precision(18, 2)]
        [Range(1, double.MaxValue)]
        public decimal? MinAmount { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxTenureInMonths { get; set; }
    }
}
