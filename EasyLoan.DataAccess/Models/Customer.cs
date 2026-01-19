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
//hash password - done
//review dtos
//trim the fields - done
//add 1:1 relationship between loan application and loan details (also include .Include in the repository) - done
//isPaid ?? - done
//public facing loan id and readable loan application id
//EasyLoan.Models.Common - done
//employee features - done
//max age?
//logout
//question - should i remove customerId if i am getting from jwt - not req
//problem details class
//in make payment controller pass the loanId in the url and after creating something instead of returning id , return the created object
//separate response patterns
//on the basis of status no different controller .. take an enum for it in the query param
//dont seed data , or change password on first login