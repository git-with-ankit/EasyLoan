using EasyLoan.Dtos.LoanType;

public interface ILoanTypeService
{
    Task<List<LoanTypeResponseDto>> GetAllAsync();
    Task<LoanTypeResponseDto> GetByIdAsync(Guid loanTypeId);
    Task<Guid> CreateLoanTypeAsync(CreateLoanTypeRequestDto dto);
    Task UpdateLoanTypeAsync(Guid loanTypeId, UpdateLoanTypeRequestDto dto);
    Task<List<LoanTypeResponseDto>> GetLoanTypesAsync();
    Task<List<EmiScheduleItemResponseDto>> PreviewEmiAsync(Guid loanTypeId,decimal amount,int tenureInMonths);
}