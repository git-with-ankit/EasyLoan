using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanType
{
    public class EmiScheduleItemResponseDto
    {
        public int EmiNumber { get; set; }
        public DateTime DueDate { get; set; }

        public decimal PrincipalComponent { get; set; }
        public decimal InterestComponent { get; set; }

        public decimal TotalEmiAmount { get; set; }

        public decimal PrincipalRemainingAfterPayment { get; set; }
    }
}
