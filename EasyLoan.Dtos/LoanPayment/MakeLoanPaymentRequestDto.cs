using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.LoanPayment
{
    public class MakeLoanPaymentRequestDto
    {

        [Required]
        [Range(0,Double.MaxValue)]
        public decimal Amount { get; set; }
    }
}
