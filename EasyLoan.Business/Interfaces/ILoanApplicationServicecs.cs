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
        Task<LoanApplicationDetailsResponseDto> GetByApplicationIdAsync(string applicationId);
        Task UpdateReviewAsync(string applicationId, ReviewLoanApplicationRequestDto dto);
        Task<LoanApplicationDetailsWithCustomerDataResponseDto> GetApplicationDetailsForReview(string applicationId);
        Task<List<LoanApplicationListItemResponseDto>> GetAllPendingApplicationsAsync();
        Task<List<LoanApplicationListItemResponseDto>> GetAssignedApplicationsAsync(Guid assignedManagerId);
    }
}
