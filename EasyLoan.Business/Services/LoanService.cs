using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.Models.Common.Enums;
using EasyLoan.Dtos.Loan;

namespace EasyLoan.Business.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanDetailsRepository _repo;

        public LoanService(ILoanDetailsRepository repo)
        {
            _repo = repo;
        }

        public async Task<LoanDetailsResponseDto> GetLoanDetailsAsync(Guid customerId, string loanNumber)
        {
            var loan = await _repo.GetByLoanNumberAsync(loanNumber)
                ?? throw new NotFoundException(ErrorMessages.LoanNotFound);

            if (loan.CustomerId != customerId)
                throw new ForbiddenException(ErrorMessages.AccessDenied);

            return new LoanDetailsResponseDto
            {
                LoanNumber = loan.LoanNumber,
                LoanType = loan.LoanType.Name,
                ApprovedAmount = loan.ApprovedAmount,
                PrincipalRemaining = loan.PrincipalRemaining,
                TenureInMonths = loan.TenureInMonths,
                InterestRate = loan.InterestRate,
                Status = loan.Status
            };
        }

        public async Task<IEnumerable<LoanSummaryResponseDto>> GetCustomerLoansAsync(Guid customerId, LoanStatus status)
        {
            var loans = await _repo.GetLoansByCustomerIdAsync(customerId);

            return loans
                .Where(l => l.Status == status)
                .Select(l => new LoanSummaryResponseDto
                {
                    LoanNumber = l.LoanNumber,
                    PrincipalRemaining = l.PrincipalRemaining,
                    InterestRate = l.InterestRate,
                    Status = l.Status
                });
        }

        //public async Task<List<EmiScheduleItemResponseDto>>GetEmiScheduleAsync(Guid customerId, Guid loanId)//TODO : Review
        //{
        //    var loan = await _repo.GetByIdAsync(loanId)
        //        ?? throw new KeyNotFoundException();

        //    if (loan.CustomerId != customerId)
        //        throw new UnauthorizedAccessException();

        //    var monthlyRate = loan.InterestRate / 12 / 100;
        //    var emi = (loan.ApprovedAmount * monthlyRate *
        //              (decimal)Math.Pow(1 + (double)monthlyRate, loan.TenureInMonths)) /
        //              ((decimal)Math.Pow(1 + (double)monthlyRate, loan.TenureInMonths) - 1);

        //    var schedule = new List<EmiScheduleItemResponseDto>();
        //    var remaining = loan.ApprovedAmount;

        //    for (int i = 1; i <= loan.TenureInMonths; i++)
        //    {
        //        var interest = remaining * monthlyRate;
        //        var principal = emi - interest;
        //        remaining -= principal;

        //        schedule.Add(new EmiScheduleItemResponseDto
        //        {
        //            EmiNumber = i,
        //            DueDate = DateTime.UtcNow.AddMonths(i),
        //            InterestComponent = Math.Round(interest, 2),
        //            PrincipalComponent = Math.Round(principal, 2),
        //            TotalEmiAmount = Math.Round(emi, 2),
        //            PrincipalRemainingAfterPayment = Math.Round(remaining, 2),
        //            IsPaid = false
        //        });
        //    }

        //    return schedule;
        //}


        //public async Task<List<LoanSummaryResponseDto>> GetAllActiveLoansAsync(Guid customerId)
        //{
        //    var loans = await _repo.GetLoansByCustomerIdAsync(customerId);

        //    return loans
        //        .Where(l => l.Status == LoanStatus.Active)
        //        .Select(l => new LoanSummaryResponseDto
        //        {
        //            LoanId = l.Id,
        //            PrincipalRemaining = l.PrincipalRemaining,
        //            InterestRate = l.InterestRate,
        //            Status = l.Status.ToString()
        //        }).ToList();
        //}
        //public async Task<List<LoanSummaryResponseDto>> GetAllClosedLoansAsync(Guid customerId)
        //{
        //    var loans = await _repo.GetLoansByCustomerIdAsync(customerId);

        //    return loans
        //        .Where(l => l.Status == LoanStatus.Closed)
        //        .Select(l => new LoanSummaryResponseDto
        //        {
        //            LoanId = l.Id,
        //            PrincipalRemaining = l.PrincipalRemaining,
        //            InterestRate = l.InterestRate,
        //            Status = l.Status.ToString()
        //        }).ToList();
        //}
    }
}
