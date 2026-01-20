using EasyLoan.Models.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanApplication
{
    public class LoanApplicationsAdminResponseDto
    {
        public string ApplicationNumber { get; set; }
        public string LoanTypeName { get; set; }
        public decimal RequestedAmount { get; set; }
        public int TenureInMonths { get; set; }
        public Guid AssignedEmployeeId { get; set; }
        public LoanApplicationStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
