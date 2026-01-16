using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Customer
{
    public class UpdateCustomerProfileRequestDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$")]
        public string PhoneNumber { get; set; }

        [Required]
        public decimal AnnualSalary { get; set; }
    }
}
