using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Customer
{
    public class CustomerProfileResponseDto
    {
        public Guid CustomerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public decimal AnnualSalary { get; set; }
        public string PanNumber { get; set; }
        public int CreditScore { get; set; }
    }
}
