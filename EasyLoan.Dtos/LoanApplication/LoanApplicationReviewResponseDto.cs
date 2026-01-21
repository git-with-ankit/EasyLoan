using EasyLoan.Models.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanApplication
{
    public class LoanApplicationReviewResponseDto
    {
        public string ApplicationNumber { get ; set ; }
        public string? ManagerComments { get ; set ; }
        public decimal ApprovedAmount { get ; set ; }
        public LoanApplicationStatus Status { get ; set ; }
    }
}
