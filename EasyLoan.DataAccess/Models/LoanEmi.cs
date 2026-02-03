using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyLoan.DataAccess.Models
{
    public class LoanEmi
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid LoanDetailsId { get; set; }

        [ForeignKey(nameof(LoanDetailsId))]
        public LoanDetails LoanDetails { get; set; }

        [Required]
        [Range(0,int.MaxValue)]
        public int EmiNumber { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Precision(18,2)]
        [Range(0,double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        [Precision(18, 2)]
        [Range(0, double.MaxValue)]
        public decimal RemainingAmount { get; set; }

        [Required]
        [Precision(18, 2)]
        [Range(0, double.MaxValue)]
        public decimal InterestComponent { get; set; }

        [Required]
        [Precision(18, 2)]
        [Range(0, double.MaxValue)]
        public decimal PrincipalComponent { get; set; }

        [Precision(18, 2)]
        [Range(0, double.MaxValue)]
        public decimal PenaltyAmount { get; set; } = 0;

        [Precision(18, 2)]
        [Range(0, double.MaxValue)]
        public decimal PaidPenaltyAmount { get; set; } = 0;

        [NotMapped]
        public bool IsPaid => RemainingAmount <= 0;

        public DateTime? PaidDate { get; set; }
    }

}
