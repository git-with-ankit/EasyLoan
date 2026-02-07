using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.Customer
{
    public class UpdateCustomerProfileRequestDto
    {
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string? Name { get; set; }

        [RegularExpression(
            @"^[6-9]\d{9}$",
            ErrorMessage = "Please enter a valid 10-digit Indian mobile number (starting with 6, 7, 8, or 9)."
        )]
        public string? PhoneNumber { get; set; }

        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Amount should have Max 2 decimals")]
        [Range(0, double.MaxValue, ErrorMessage = "Annual salary must be a positive number and should not exceed range.")]
        public decimal? AnnualSalary { get; set; }
    }
}
