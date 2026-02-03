using EasyLoan.Models.Common.Enums;

namespace EasyLoan.Dtos.LoanApplication
{
    public class LoanApplicationDetailsResponseDto
    {
        public string ApplicationNumber { get; set; }
        public string CustomerName { get; set; }
        public string LoanType { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal InterestRate { get; set; }
        public Guid AssignedEmployeeId { get; set; }
        public int RequestedTenureInMonths { get; set; }
        public LoanApplicationStatus Status { get; set; }
        public string? ManagerComments { get; set; }
    }
}
