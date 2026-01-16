using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanPayment
{
    public class LoanPaymentHistoryResponseDto
    {
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}
