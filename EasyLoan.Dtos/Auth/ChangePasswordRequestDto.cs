using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.Auth
{
    public class ChangePasswordRequestDto
    {
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$", 
            ErrorMessage = "Password must be 8 to 20 characters long, it must include an upper case letter, a lower case letter and a special character.")]
        public required string OldPassword { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$",
            ErrorMessage = "Password must be 8 to 20 characters long, it must include an upper case letter, a lower case letter and a special character.")]
        public required string NewPassword { get; set; }
    }
}
