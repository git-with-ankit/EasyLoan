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
//All the todos
//aditya sir suggested (remove preview)
//add enum in dtos
//exception handling and from responses
//jwt
//loanId should not be public create a public loan id
//put to patch

//Model validation errors with api response dto - done
//enums in dtos
//db enum stored as int - add a migration and done
//saare required payment store kar lo pehle se hee and then override the status and amount - done
//hash password
//review dtos
//question - should i remove customerId if i am getting from jwt