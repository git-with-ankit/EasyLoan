using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.Common
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$",
            ErrorMessage = "Password must be 8–20 characters and include at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character (@$!%*?&)."
        )]
        public string Password { get; set; }
    }
}
