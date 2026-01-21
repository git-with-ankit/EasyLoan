using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanType
{
    public class LoanTypeResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public decimal InterestRate { get; set; }
        public decimal MinAmount { get; set; }
        public int MaxTenureInMonths { get; set; }
    }
}
