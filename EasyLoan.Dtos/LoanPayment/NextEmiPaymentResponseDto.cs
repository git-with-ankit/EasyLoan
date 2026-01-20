using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanPayment
{
    public class DueEmisResponseDto
    {
        public DateTime DueDate { get; set; }
        public decimal EmiAmount { get; set; }
        public decimal InterestComponent { get; set; }
        public decimal PrincipalComponent { get; set; }
        public decimal PrincipalRemainingAfterPayment { get; set; }
        public decimal RemainingEmiAmount { get; set; }
    }

}
