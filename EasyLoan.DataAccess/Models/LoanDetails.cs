using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasyLoan.Models.Common.Enums;

namespace EasyLoan.DataAccess.Models
{
    [Index(nameof(LoanNumber),IsUnique = true)]
    public class LoanDetails
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(15)]
        [RegularExpression("^[LN]-[A-Za-z0-9]{8}$")]
        public string LoanNumber { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid LoanApplicationId { get; set; }

        [Required]
        public Guid LoanTypeId { get; set; }

        [Required]
        public Guid ApprovedByEmployeeId { get; set; }

        [Required]
        [Precision(18,2)]
        public decimal ApprovedAmount { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal PrincipalRemaining { get; set; }

        [Required]
        public int TenureInMonths { get; set; }

        [Required]
        [Precision(5, 2)]
        public decimal InterestRate { get; set; }

        [Required]
        public LoanStatus Status { get; set; }

        [Required]
        public DateTime CreatedDate {  get; set; }

        [ForeignKey(nameof(CustomerId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Customer Customer { get; set; }

        [ForeignKey(nameof(LoanTypeId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public LoanType LoanType { get; set; }

        [ForeignKey(nameof(ApprovedByEmployeeId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Employee ApprovedByEmployee { get; set; }

        [ForeignKey(nameof(LoanApplicationId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public LoanApplication LoanApplication { get; set; }
        public ICollection<LoanPayment> LoanPayments { get; set; } = new List<LoanPayment>();
        public ICollection<LoanEmi> Emis { get; set; } = new List<LoanEmi>();
    }
}
