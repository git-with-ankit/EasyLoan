using EasyLoan.Dtos.LoanType;

public interface ILoanTypeService
{
    Task<IEnumerable<LoanTypeResponseDto>> GetAllAsync();
    Task<LoanTypeResponseDto> GetByIdAsync(Guid loanTypeId);
    Task<LoanTypeResponseDto> CreateLoanTypeAsync(LoanTypeRequestDto dto);
    Task<LoanTypeResponseDto> UpdateLoanTypeAsync(Guid loanTypeId, UpdateLoanTypeRequestDto dto);
    Task<IEnumerable<LoanTypeResponseDto>> GetLoanTypesAsync();
    Task<IEnumerable<EmiScheduleItemResponseDto>> PreviewEmiAsync(Guid loanTypeId,decimal amount,int tenureInMonths);
}