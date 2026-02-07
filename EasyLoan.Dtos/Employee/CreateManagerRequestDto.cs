using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.Employee
{
    public class CreateManagerRequestDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(
            @"^[6-9]\d{9}$",
            ErrorMessage = "Please enter a valid 10-digit Indian mobile number (starting with 6, 7, 8, or 9)."
        )]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$",
            ErrorMessage = "Password must be 8–20 characters and include at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character (@$!%*?&)."
        )]
        public string Password { get; set; }
    }
}
