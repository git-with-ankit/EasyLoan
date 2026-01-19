using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Employee
{
    public class UpdateEmployeeProfileRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$",ErrorMessage = "Please enter correct Indian phone number.")]
        public string PhoneNumber { get; set; }

    }
}
