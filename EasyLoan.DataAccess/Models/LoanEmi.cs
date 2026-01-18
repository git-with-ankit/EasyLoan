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
        public bool IsPaid => RemainingAmount <= 0;

        public DateTime? PaidDate { get; set; }
    }

}
