using EasyLoan.Business.Enums;
using EasyLoan.Dtos.LoanApplication;
using EasyLoan.Models.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Interfaces
{
    public interface ILoanApplicationService
    {
        Task<CreatedApplicationResponseDto> CreateAsync(Guid customerId, CreateLoanApplicationRequestDto dto);
        //Task<List<LoanApplicationsResponseDto>> GetCustomerApplicationsAsync(Guid customerId);
        Task<LoanApplicationDetailsResponseDto> GetByApplicationNumberAsync(string applicationNumber);
        Task<LoanApplicationReviewResponseDto> UpdateReviewAsync(string applicationNumber, Guid managerId, ReviewLoanApplicationRequestDto dto);
        Task<LoanApplicationDetailsWithCustomerDataResponseDto> GetApplicationDetailsForReview(string applicationNumber, Guid userId, Role userRole);
        //Task<List<LoanApplicationsAdminResponseDto>> GetAllPendingApplicationsAsync();
        //Task<List<LoanApplicationsResponseDto>> GetAssignedApplicationsAsync(Guid assignedManagerId);
        Task<IEnumerable<LoanApplicationsResponseDto>> GetApplicationsAsync(Guid userId, Role userRole, LoanApplicationStatus status);
    }
}
