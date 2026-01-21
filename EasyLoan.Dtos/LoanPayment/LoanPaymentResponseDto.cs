using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanPayment
{
    public class LoanPaymentResponseDto
    {
        public DateTime? PaymentDate { get; set; }

        public decimal Amount { get; set; }

        public Guid TransactionId { get; set; }

    }
}
