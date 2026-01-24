using EasyLoan.Models.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanApplication
{
    public class LoanApplicationDetailsWithCustomerDataResponseDto
    {
        public string ApplicationNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal AnnualSalaryOfCustomer { get; set; }
        public string PhoneNumber { get; set; }
        public int CreditScore { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PanNumber { get; set; }
        public string LoanType { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal AppprovedAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int RequestedTenureInMonths { get; set; }
        public LoanApplicationStatus Status { get; set; }
        public string? ManagerComments { get; set; }
        public int TotalOngoingLoans { get; set; }
    }
}
