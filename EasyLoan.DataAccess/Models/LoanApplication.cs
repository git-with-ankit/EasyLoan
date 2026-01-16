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
    public class LoanApplication
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string ApplicationId { get; set; }//cutomerid_date
        [Required]
        public Guid CustomerId { get; set; }
        [Required]
        public Guid LoanTypeId { get; set; }

        [Required]
        [Precision(5, 2)]
        public decimal InterestRate { get; set; }

        [Required]
        public Guid AssignedEmployeeId { get; set; }
        [Required]
        [Precision(18, 2)]
        public decimal RequestedAmount { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal ApprovedAmount { get; set; }
        [Required]
        public int RequestedTenureInMonths { get; set; }
        [Required]
        public LoanApplicationStatus Status { get; set; }
        [MaxLength(1000)]
        public string? ManagerComments { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CustomerId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Customer Customer { get; set; }

        [ForeignKey(nameof(LoanTypeId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public LoanType LoanType { get; set; }

        [ForeignKey(nameof(AssignedEmployeeId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Employee ApprovedByEmployee { get; set; }

    }
}
