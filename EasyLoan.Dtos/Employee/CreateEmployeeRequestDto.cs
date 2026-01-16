using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.Employee
{
    public class CreateEmployeeRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$")]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
