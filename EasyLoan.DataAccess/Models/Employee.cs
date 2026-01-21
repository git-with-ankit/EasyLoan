using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using EasyLoan.Models.Common.Enums;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Employee
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
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        [Required]
        [RegularExpression("/^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,16}$/")]
        public string Password { get; set; }
        [Required]
        public EmployeeRole Role { get; set; }
        public ICollection<LoanDetails> ApprovedLoans { get; set; } = new List<LoanDetails>();
        public ICollection<LoanApplication> AssignedLoanApplications { get; set; } = new List<LoanApplication>();
    }
}
