using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanApplication
{
    public class CreateLoanApplicationRequestDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid LoanTypeId { get; set; }

        [Required]
        [Range(0,double.MaxValue)]
        public decimal RequestedAmount { get; set; }

        [Required]
        [Range(1, 360)]
        public int RequestedTenureInMonths { get; set; }
    }
}
