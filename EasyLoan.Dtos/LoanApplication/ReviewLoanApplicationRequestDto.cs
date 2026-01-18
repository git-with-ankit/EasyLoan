using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanApplication
{
    public class ReviewLoanApplicationRequestDto
    {
        [Required]
        public bool IsApproved { get; set; }

        [MaxLength(1000)]
        public string? ManagerComments { get; set; }

        [Required]
        [Precision(18,2)]
        [Range(0,double.MaxValue)]
        public decimal ApprovedAmount { get; set; }
    }
}
