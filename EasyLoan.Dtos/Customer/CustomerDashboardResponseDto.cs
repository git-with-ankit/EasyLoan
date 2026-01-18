using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Customer
{
    public class CustomerDashboardResponseDto
    {
        public int CreditScore { get; set; }
        public int TotalNumberOfPendingApplications { get; set; }
        public int TotalNumberOfApprovedApplications { get; set; }
        public int TotalNumberOfRejectedApplications { get; set; }
        public int TotalNumberOfActiveLoans { get; set; }
        public int TotalNumberOfClosedLoans { get; set; }
        //public int NumberOfPendingPayments { get; set; }
    }
}
