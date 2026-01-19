using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasyLoan.Models.Common.Enums;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.Models
{
    [Index(nameof(ApplicationNumber), IsUnique = true)]
    public class LoanApplication
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(15)]
        [RegularExpression("^[LA|LN]-[A-Za-z0-9]{8}$")]
        public string ApplicationNumber { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid LoanTypeId { get; set; }

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

        public LoanDetails? LoanDetails { get; set; }

    }
}
