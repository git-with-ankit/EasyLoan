using EasyLoan.Models.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanPayment
{
    public class LoanPaymentHistoryResponseDto
    {
        public DateTime? PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public LoanPaymentStatus Status { get; set; }
    }
}
