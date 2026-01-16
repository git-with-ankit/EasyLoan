using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Loan
{
    public class LoanDetailsResponseDto
    {
        public Guid LoanId { get; set; }
        public string LoanType { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal PrincipalRemaining { get; set; }
        public int TenureInMonths { get; set; }
        public decimal InterestRate { get; set; }
        public string Status { get; set; }
    }
}
