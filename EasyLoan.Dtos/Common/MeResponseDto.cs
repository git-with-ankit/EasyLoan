using EasyLoan.Models.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.Common
{
    public class MeResponseDto
    {
        [Required]
        public required string Email { get; set; }

        [Required]
        public required Role Role { get; set; }
    }
}


