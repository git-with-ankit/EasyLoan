using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal Amount { get; set; }

        [Required]
        public Guid TransactionId { get; set; }

        [Required]
        [Precision(5,2)]
        public decimal InterestPaid { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal PrincipalPaid { get; set; }

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
