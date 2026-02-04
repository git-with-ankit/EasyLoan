using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Employee
{
    public class AdminDashboardResponseDto
    {
        public int NumberOfLoanTypes { get; set; }
        public int NumberOfPendingApplications { get; set; }
        public int NumberOfApprovedApplications { get; set; }
        public int NumberOfRejectedApplications { get; set; }
        public int NumberOfManagers { get; set; }

    }
}
