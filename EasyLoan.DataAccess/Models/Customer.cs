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
//review dtos >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
//trim the fields - done
//add 1:1 relationship between loan application and loan details (also include .Include in the repository) - done
//isPaid ?? - done
//public facing loan id and readable loan application id - done
//EasyLoan.Models.Common - done
//employee features - done
//max age? - done
//logout
//question - should i remove customerId if i am getting from jwt - not req - yes
//problem details class - done
//in make payment controller pass the loanId in the url - done
//separate response patterns - done
//after creating something instead of returning id , return the created object and same in patch - done
//dont seed data , or change password on first login
//types of code coverage
//api testing
//change return type to IENumerbale in repo , in services and controller change List to IEnumerable and also remove .ToListAsync from service - done
//on loan details for a particular loan next emi amount and due date - done
//one which will fetch all the overdue payments - done
//pagination - if time permits
//filter in db only and IQueryable so that ....filteration on db side - if time permits
//one generic repo - done 
//auth controller single - done
//migrate db - done
//application number make dto and regex and for loan number - done
//Remove api dto - done
//check whether it required GetIdWithDetails or it should just call GetId - done
//review dto
//LoanPaymentStatus - done
//NoTracking or Update - done
//TokenGeneratorService move to controllers - rejected
//make payment and emi functions shorten them , use private functions - done
//Testing use in memory for repo
//the date of birth should be till now - done
//use helper-setuser instead of creating a separate setuser in each class(tests)