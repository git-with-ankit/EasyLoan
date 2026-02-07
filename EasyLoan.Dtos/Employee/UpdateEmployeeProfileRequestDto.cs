using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.Employee
{
    public class UpdateEmployeeProfileRequestDto
    {
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string? Name { get; set; }

        [RegularExpression(
            @"^[6-9]\d{9}$",
            ErrorMessage = "Please enter a valid 10-digit Indian mobile number (starting with 6, 7, 8, or 9)."
        )]
        public string? PhoneNumber { get; set; }
    }
}
