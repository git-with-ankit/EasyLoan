using EasyLoan.DataAccess.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyLoan.Dtos.LoanApplication
{
    public class LoanApplicationListItemResponseDto
    {
        public string ApplicationId { get; set; }
        public string LoanTypeName { get; set; }
        public decimal RequestedAmount { get; set; }
        public int TenureInMonths { get; set; }
        public Guid AssignedEmployeeId {  get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
