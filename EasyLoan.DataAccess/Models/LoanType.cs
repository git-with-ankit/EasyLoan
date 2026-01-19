using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EasyLoan.Models.Common.Enums;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class LoanType
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Precision(5, 2)]
        public decimal InterestRate { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal MinAmount { get; set; }

        [Required]
        public int MaxTenureInMonths { get; set; }

        public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();

        public ICollection<LoanDetails> Loans { get; set; } = new List<LoanDetails>();
    }
}
