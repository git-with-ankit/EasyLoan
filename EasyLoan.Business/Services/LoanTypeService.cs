using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Helper;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.LoanType;

public class LoanTypeService : ILoanTypeService
{
    private readonly ILoanTypeRepository _loanTypeRepo;

    public LoanTypeService(ILoanTypeRepository loanTypeRepository)
    {
        _loanTypeRepo = loanTypeRepository;
    }

    public async Task<List<LoanTypeResponseDto>> GetAllAsync()
    {
        var loanTypes = await _loanTypeRepo.GetAllAsync();

        return loanTypes.Select(lt => new LoanTypeResponseDto
        {
            Id = lt.Id,
            Name = lt.Name,
            InterestRate = lt.InterestRate,
            MinAmount = lt.MinAmount,
            MaxTenureInMonths = lt.MaxTenureInMonths
        }).ToList();
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
    public async Task<Guid> CreateLoanTypeAsync(CreateLoanTypeRequestDto dto)
    {
        var type = new LoanType
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            InterestRate = dto.InterestRate,
            MinAmount = dto.MinAmount,
            MaxTenureInMonths = dto.MaxTenureInMonths
        };

        await _loanTypeRepo.AddAsync(type);
        await _loanTypeRepo.SaveChangesAsync();

        return type.Id;
    }

    public async Task UpdateLoanTypeAsync(Guid loanTypeId, UpdateLoanTypeRequestDto dto)
    {
        var type = await _loanTypeRepo.GetByIdAsync(loanTypeId)
            ?? throw new NotFoundException(ErrorMessages.LoanTypeNotFound);

        type.InterestRate = dto.InterestRate;
        type.MinAmount = dto.MinAmount;
        type.MaxTenureInMonths = dto.MaxTenureInMonths;

        await _loanTypeRepo.UpdateAsync(type);
        await _loanTypeRepo.SaveChangesAsync();
    }

    public async Task<List<LoanTypeResponseDto>> GetLoanTypesAsync()
    {
        var types = await _loanTypeRepo.GetAllAsync();

        return types.Select(t => new LoanTypeResponseDto
        {
            Id = t.Id,
            Name = t.Name,
            InterestRate = t.InterestRate,
            MinAmount = t.MinAmount,
            MaxTenureInMonths = t.MaxTenureInMonths
        }).ToList();
    }
    public async Task<List<EmiScheduleItemResponseDto>> PreviewEmiAsync(
       Guid loanTypeId,
       decimal amount,
       int tenureInMonths)
    {
        var loanType = await _loanTypeRepo.GetByIdAsync(loanTypeId)
            ?? throw new NotFoundException(ErrorMessages.LoanTypeNotFound);

        if(tenureInMonths > loanType.MaxTenureInMonths)
        {
            throw new BusinessRuleViolationException(ErrorMessages.ExceededMaxTenure);
        }

        if (amount < loanType.MinAmount)
            throw new BusinessRuleViolationException(ErrorMessages.AmountLessThanMinAmount);

        return EmiCalculator.GenerateSchedule(
            principal: amount,
            annualInterestRate: loanType.InterestRate,
            tenureInMonths: tenureInMonths,
            startDate: DateTime.UtcNow);
    }

}
