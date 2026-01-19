using EasyLoan.Models.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Loan
{
    public class LoanSummaryResponseDto
    {
        public string LoanNumber { get; set; }
        public decimal PrincipalRemaining { get; set; }
        public decimal InterestRate { get; set; }
        public LoanStatus Status { get; set; }
    }
}
