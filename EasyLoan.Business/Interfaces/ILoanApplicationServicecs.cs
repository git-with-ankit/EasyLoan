using EasyLoan.Dtos.LoanApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Interfaces
{
    public interface ILoanApplicationService
    {
        Task<string> CreateAsync(Guid customerId, CreateLoanApplicationRequestDto dto);
        Task<List<LoanApplicationListItemResponseDto>> GetCustomerApplicationsAsync(Guid customerId);
        Task<LoanApplicationDetailsResponseDto> GetByApplicationNumberAsync(string applicationNumber);
        Task UpdateReviewAsync(string applicationNumber, Guid managerId, ReviewLoanApplicationRequestDto dto);
        Task<LoanApplicationDetailsWithCustomerDataResponseDto> GetApplicationDetailsForReview(string applicationNumber, Guid managerId);
        Task<List<LoanApplicationListItemResponseForAdminDto>> GetAllPendingApplicationsAsync();
        Task<List<LoanApplicationListItemResponseDto>> GetAssignedApplicationsAsync(Guid assignedManagerId);
    }
}
