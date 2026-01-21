using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasyLoan.Models.Common.Enums;

namespace EasyLoan.DataAccess.Models
{
    [Index(nameof(TransactionId), IsUnique = true)]
    public class LoanPayment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid LoanDetailsId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        public DateTime? PaymentDate { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal Amount { get; set; }

        [Required]
        public Guid TransactionId { get; set; }

        [Required]
        public LoanPaymentStatus Status { get; set; }

        [ForeignKey(nameof(LoanDetailsId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public LoanDetails LoanDetails { get; set; }

        [ForeignKey(nameof(CustomerId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Customer Customer { get; set; }
    }
}
