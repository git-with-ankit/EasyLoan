using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.LoanApplication;
using EasyLoan.Models.Common.Enums;

namespace EasyLoan.Business.Interfaces
{
    public interface ILoanApplicationService
    {
        Task<CreatedApplicationResponseDto> CreateAsync(Guid customerId, CreateLoanApplicationRequestDto dto);
        Task<LoanApplicationDetailsResponseDto> GetByApplicationNumberAsync(string applicationNumber);
        Task<LoanApplicationReviewResponseDto> UpdateReviewAsync(string applicationNumber, Guid managerId, ReviewLoanApplicationRequestDto dto);
        Task<LoanApplicationDetailsWithCustomerDataResponseDto> GetApplicationDetailsForReview(string applicationNumber, Guid userId, Role userRole);
        Task<PagedResponseDto<LoanApplicationsResponseDto>> GetApplicationsAsync(Guid userId, Role userRole, LoanApplicationStatus status, int pageNumber, int pageSize);
    }
}
