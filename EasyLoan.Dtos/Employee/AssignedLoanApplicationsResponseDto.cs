using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Employee
{
    public class AssignedLoanApplicationsResponseDto
    {
        public string ApplicationId { get; set; }
        public string CustomerName { get; set; }
        public string LoanTypeName { get; set; }
        public decimal RequestedAmount { get; set; }
        public int RequestedTenureInMonths { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }
}
