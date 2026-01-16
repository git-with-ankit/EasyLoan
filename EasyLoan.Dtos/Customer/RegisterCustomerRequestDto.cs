using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Customer
{
    public class RegisterCustomerRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(150)]
        [RegularExpression(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(
            @"^[6-9]\d{9}$",
            ErrorMessage = "Invalid Indian phone number")]
        public string PhoneNumber { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public decimal AnnualSalary { get; set; }

        [Required, MaxLength(10)]
        [RegularExpression(
            @"^[A-Z]{5}[0-9]{4}[A-Z]$",
            ErrorMessage = "Invalid PAN format")]
        public string PanNumber { get; set; }

        [Required]
        [RegularExpression("/^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,16}$/")]
        public string Password { get; set; }
    }
}
