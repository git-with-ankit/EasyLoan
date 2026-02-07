using EasyLoan.Dtos.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.Customer
{
    public class RegisterCustomerRequestDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        [RegularExpression(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Please enter a valid email address."
        )]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(
            @"^[6-9]\d{9}$",
            ErrorMessage = "Please enter a valid 10-digit Indian mobile number (starting with 6, 7, 8, or 9)."
        )]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [MaxAgeAttribute(150, ErrorMessage = "Age cannot be more than 150 years.")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Annual salary is required.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Amount should have Max 2 decimals")]
        [Range(0, double.MaxValue, ErrorMessage = "Annual salary must be a positive number and should not exceed range.")]
        public decimal AnnualSalary { get; set; }

        [Required(ErrorMessage = "PAN number is required.")]
        [MaxLength(10, ErrorMessage = "PAN number must be exactly 10 characters.")]
        [RegularExpression(
            @"^[A-Z]{5}[0-9]{4}[A-Z]$",
            ErrorMessage = "Please enter a valid PAN number (example: ABCDE1234F)."
        )]
        public string PanNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$",
            ErrorMessage = "Password must be 8–20 characters and include at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character (@$!%*?&)."
        )]
        public string Password { get; set; }
    }
}
