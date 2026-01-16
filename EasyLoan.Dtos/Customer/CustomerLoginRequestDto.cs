using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Customer
{
    public class CustomerLoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("/^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,16}$/")]
        public string Password { get; set; }
    }
}
