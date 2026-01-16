using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanApplication
{
    public class LoanApplicationDetailsResponseDto
    {
        public string ApplicationId { get; set; }
        public string CustomerName { get; set; }
        public string LoanType { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal AppprovedAmount { get; set; }
        public decimal InterestRate { get; set; }
        //public EmiPlan EmiPlan { get; set; }TODO : Implement
        public int RequestedTenureInMonths { get; set; }
        public string Status { get; set; }
        public string? ManagerComments { get; set; }
    }
}
