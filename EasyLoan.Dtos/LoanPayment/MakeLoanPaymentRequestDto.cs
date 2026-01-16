using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanPayment
{
    public class MakeLoanPaymentRequestDto
    {
        [Required]
        public Guid LoanId { get; set; }

        [Required]
        [Range(0,Double.MaxValue)]
        public decimal Amount { get; set; }
    }
}
