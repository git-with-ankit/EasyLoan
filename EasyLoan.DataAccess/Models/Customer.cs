using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EasyLoan.DataAccess.Models
{
    [Index(nameof(Email),nameof(PanNumber), IsUnique=true)]
    public class Customer
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal AnnualSalary { get; set; }

        [Required]
        [MaxLength(10)]
        public string PanNumber { get; set; }

        [Required]
        [RegularExpression("/^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,16}$/")]
        public string Password { get; set; }

        [Required]
        public int CreditScore { get; set; } = 800;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
        public ICollection<LoanDetails> Loans { get; set; } = new List<LoanDetails>();
        public ICollection<LoanPayment> LoanPayments { get; set; } = new List<LoanPayment>();
    }
}
