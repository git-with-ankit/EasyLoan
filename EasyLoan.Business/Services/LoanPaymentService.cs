using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.LoanPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Services
{
    public class LoanPaymentService : ILoanPaymentService
    {
        private readonly ILoanPaymentRepository _paymentRepo;
        private readonly ILoanDetailsRepository _loanRepo;

        public LoanPaymentService(
            ILoanPaymentRepository repo,
            ILoanDetailsRepository loanRepo)
        {
            _paymentRepo = repo;
            _loanRepo = loanRepo;
        }

        public async Task<List<LoanPaymentHistoryResponseDto>> GetPaymentHistoryAsync(Guid customerId, LoanPaymentHistoryRequestDto dto)//TODO : Review
        {
            var loan = await _loanRepo.GetByIdAsync(dto.LoanId) ?? throw new KeyNotFoundException("Loan not found");
            if (loan.CustomerId != customerId)
                throw new UnauthorizedAccessException("Access denied to loan payments");
            var payments = await _paymentRepo.GetByLoanIdAsync(dto.LoanId);
            return payments
                .Where(p => p.Status == LoanPaymentStatus.Paid)
                .OrderBy(p => p.DueDate)
                .Select(p => new LoanPaymentHistoryResponseDto
                {
                    DueDate = p.DueDate,
                    PaymentDate = p.PaymentDate,
                    Amount = p.Amount,
                    Status = p.Status.ToString()
                })
                .ToList();
        }

        public async Task MakePaymentAsync(Guid customerId, MakeLoanPaymentRequestDto dto)//TODO : Review
        {
            var loan = await _loanRepo.GetByIdAsync(dto.LoanId)
                ?? throw new KeyNotFoundException("Loan not found");

            if (loan.CustomerId != customerId)
                throw new UnauthorizedAccessException();

            if (loan.Status != LoanStatus.Active)
                throw new InvalidOperationException("Loan is not active");

            var monthlyRate = loan.InterestRate / 12 / 100;
            var interestDue = loan.PrincipalRemaining * monthlyRate;

            var interestPaid = Math.Min(dto.Amount, interestDue);
            var principalPaid = dto.Amount - interestPaid;

            if (principalPaid < 0)
                principalPaid = 0;

            loan.PrincipalRemaining -= principalPaid;

            if (loan.PrincipalRemaining <= 0)
            {
                loan.PrincipalRemaining = 0;
                loan.Status = LoanStatus.Closed;
            }
            
            var payment = new LoanPayment
            {
                Id = Guid.NewGuid(),
                LoanDetailsId = loan.Id,
                CustomerId = loan.CustomerId,
                Amount = dto.Amount,
                InterestPaid = Math.Round(interestPaid, 2),
                PrincipalPaid = Math.Round(principalPaid, 2),
                PaymentDate = DateTime.UtcNow,
                TransactionId = Guid.NewGuid(),
                Status = LoanPaymentStatus.Paid
            };

            await _paymentRepo.AddAsync(payment);
            await _loanRepo.UpdateAsync(loan);
            await _paymentRepo.SaveChangesAsync();
        }
        public async Task<NextEmiPaymentResponseDto> GetNextPaymentAsync(
    Guid customerId,
    Guid loanId)
        {
            var loan = await _loanRepo.GetByIdAsync(loanId)
                ?? throw new KeyNotFoundException("Loan not found");

            if (loan.CustomerId != customerId)
                throw new UnauthorizedAccessException();

            if (loan.Status != LoanStatus.Active)
                throw new InvalidOperationException("Loan is not active");

            var today = DateTime.UtcNow.Date;

            // 1️⃣ Months elapsed since loan start
            var monthsElapsed =
                ((today.Year - loan.CreatedDate.Year) * 12) +
                (today.Month - loan.CreatedDate.Month);

            if (today.Day < loan.CreatedDate.Day)
                monthsElapsed--;

            // 2️⃣ Next due date
            var nextDueDate = loan.CreatedDate.AddMonths(monthsElapsed + 1);

            // 3️⃣ EMI calculation based on CURRENT principal
            var monthlyRate = loan.InterestRate / 12 / 100;

            var emi = (loan.PrincipalRemaining * monthlyRate *
                      (decimal)Math.Pow(1 + (double)monthlyRate, loan.TenureInMonths)) /
                      ((decimal)Math.Pow(1 + (double)monthlyRate, loan.TenureInMonths) - 1);

            var interest = loan.PrincipalRemaining * monthlyRate;
            var principal = emi - interest;

            if (principal > loan.PrincipalRemaining)
                principal = loan.PrincipalRemaining;

            return new NextEmiPaymentResponseDto
            {
                DueDate = nextDueDate,
                EmiAmount = Math.Round(emi, 2),
                InterestComponent = Math.Round(interest, 2),
                PrincipalComponent = Math.Round(principal, 2),
                PrincipalRemainingAfterPayment =
                    Math.Round(loan.PrincipalRemaining - principal, 2)
            };
        }

    }
}
