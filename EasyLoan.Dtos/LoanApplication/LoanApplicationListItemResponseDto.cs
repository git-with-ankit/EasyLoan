using EasyLoan.DataAccess.Models;
using System.ComponentModel.DataAnnotations;
using EasyLoan.Models.Common.Enums;

namespace EasyLoan.Dtos.LoanApplication
{
    public class LoanApplicationsResponseDto
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
