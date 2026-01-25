using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.LoanType;
using EasyLoan.Business.Interfaces;

public class LoanTypeService : ILoanTypeService
{
    private readonly ILoanTypeRepository _loanTypeRepo;
    private readonly IEmiCalculatorService _emiCalculatorService;

    public LoanTypeService(ILoanTypeRepository loanTypeRepository, IEmiCalculatorService emiCalculatorService)
    {
        _loanTypeRepo = loanTypeRepository;
        _emiCalculatorService = emiCalculatorService;
    }

    public async Task<IEnumerable<LoanTypeResponseDto>> GetAllAsync()
    {
        var loanTypes = await _loanTypeRepo.GetAllAsync();

        return loanTypes.Select(lt => new LoanTypeResponseDto
        {
            Id = lt.Id,
            Name = lt.Name,
            InterestRate = lt.InterestRate,
            MinAmount = lt.MinAmount,
            MaxTenureInMonths = lt.MaxTenureInMonths
        });
    }

    public async Task<LoanTypeResponseDto> GetByIdAsync(Guid loanTypeId)
    {
        var loanType = await _loanTypeRepo.GetByIdAsync(loanTypeId)
            ?? throw new NotFoundException(ErrorMessages.LoanTypeNotFound);

        return new LoanTypeResponseDto
        {
            Id = loanType.Id,
            Name = loanType.Name,
            InterestRate = loanType.InterestRate,
            MinAmount = loanType.MinAmount,
            MaxTenureInMonths = loanType.MaxTenureInMonths
        };
    }
    public async Task<LoanTypeResponseDto> CreateLoanTypeAsync(LoanTypeRequestDto dto)
    {
        var type = new LoanType
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            InterestRate = dto.InterestRate,
            MinAmount = dto.MinAmount,
            MaxTenureInMonths = dto.MaxTenureInMonths
        };

        await _loanTypeRepo.AddAsync(type);
        await _loanTypeRepo.SaveChangesAsync();

        return new LoanTypeResponseDto()
        {
            Name = type.Name,
            MinAmount = type.MinAmount,
            Id = type.Id,
            InterestRate = type.InterestRate,
            MaxTenureInMonths = type.MaxTenureInMonths
        };
    }

    public async Task<LoanTypeResponseDto> UpdateLoanTypeAsync(Guid loanTypeId, UpdateLoanTypeRequestDto dto)
    {
        var type = await _loanTypeRepo.GetByIdAsync(loanTypeId)
            ?? throw new NotFoundException(ErrorMessages.LoanTypeNotFound);

        type.InterestRate = dto.InterestRate ?? type.InterestRate;
        type.MinAmount = dto.MinAmount ?? type.MinAmount;
        type.MaxTenureInMonths = dto.MaxTenureInMonths ?? type.MaxTenureInMonths;

        var updatedEntity = type;
        
        //await _loanTypeRepo.UpdateAsync(type);
        await _loanTypeRepo.SaveChangesAsync();

        return new LoanTypeResponseDto()
        {
            Name = type.Name,
            MinAmount = type.MinAmount,
            Id = type.Id,
            InterestRate = type.InterestRate,
            MaxTenureInMonths = type.MaxTenureInMonths
        };
    }

    public async Task<IEnumerable<LoanTypeResponseDto>> GetLoanTypesAsync()
    {
        var types = await _loanTypeRepo.GetAllAsync();

        return types.Select(t => new LoanTypeResponseDto
        {
            Id = t.Id,
            Name = t.Name,
            InterestRate = t.InterestRate,
            MinAmount = t.MinAmount,
            MaxTenureInMonths = t.MaxTenureInMonths
        });
    }
    public async Task<IEnumerable<EmiScheduleItemResponseDto>> PreviewEmiAsync(Guid loanTypeId, decimal amount, int tenureInMonths)
    {
        var loanType = await _loanTypeRepo.GetByIdAsync(loanTypeId)
            ?? throw new NotFoundException(ErrorMessages.LoanTypeNotFound);

        if(tenureInMonths > loanType.MaxTenureInMonths)
        {
            throw new BusinessRuleViolationException(ErrorMessages.ExceededMaxTenure);
        }

        if (amount < loanType.MinAmount)
            throw new BusinessRuleViolationException(ErrorMessages.AmountLessThanMinAmount);

        return _emiCalculatorService.GenerateSchedule(
            principal: amount,
            annualInterestRate: loanType.InterestRate,
            tenureInMonths: tenureInMonths,
            startDate: DateTime.UtcNow);
    }

}
