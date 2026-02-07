using EasyLoan.Models.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.Auth
{
    public class MeResponseDto
    {
        [Required(ErrorMessage = "Email is missing in the response.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "User role is missing in the response.")]
        public required Role Role { get; set; }
    }
}
