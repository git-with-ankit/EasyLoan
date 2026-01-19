using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.Employee
{
    public class CreateManagerRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Phone Number is not valid indian phone number.")]
        public string PhoneNumber { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$", ErrorMessage = "Password must be 8 to 20 characters long , it must include an upper case letter, a lower case letter and a special character.")]
        public string Password { get; set; }
    }
}
