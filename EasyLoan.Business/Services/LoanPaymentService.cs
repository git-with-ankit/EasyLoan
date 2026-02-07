using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.LoanPayment;
using EasyLoan.Models.Common.Enums;
using System.Text.RegularExpressions;

namespace EasyLoan.Business.Services
{
    public class LoanPaymentService : ILoanPaymentService
    {
        private readonly ILoanPaymentRepository _paymentRepo;
        private readonly ILoanDetailsRepository _loanRepo;
        private readonly ICustomerRepository _customerRepo;
        private static readonly Regex LoanNumberRegex = new(@"^LN-[0-9A-Za-z]{8}$", RegexOptions.Compiled);
        public LoanPaymentService(
            ILoanPaymentRepository repo,
            ILoanDetailsRepository loanRepo,
            ICustomerRepository customerRepo)
        {
            _paymentRepo = repo;
            _loanRepo = loanRepo;
            _customerRepo = customerRepo;
        }

        public async Task<IEnumerable<LoanPaymentHistoryResponseDto>> GetPaymentsHistoryAsync(Guid customerId, string loanNumber)//TODO : Review
        {
            if (!LoanNumberRegex.IsMatch(loanNumber))
            {
                throw new BusinessRuleViolationException(ErrorMessages.WrongFormatForLoanNumber);
            }
            var loan = await _loanRepo.GetByLoanNumberWithDetailsAsync(loanNumber) ?? throw new NotFoundException(ErrorMessages.LoanNotFound);
            if (loan.CustomerId != customerId)
                throw new ForbiddenException(ErrorMessages.AccessDenied);

            var payments = await _paymentRepo.GetByLoanIdAsync(loan.Id);

            return payments
                .Where(p => p.Status == LoanPaymentStatus.Paid)
                .OrderBy(p => p.PaymentDate)
                .Select(p => new LoanPaymentHistoryResponseDto
                {
                    PaymentDate = p.PaymentDate,
                    Amount = p.Amount,
                    Status = p.Status
                });
        }

        //public async Task MakePaymentAsync(Guid customerId, MakeLoanPaymentRequestDto dto)
        //{
        //    var loan = await _loanRepo.GetByIdAsync(dto.LoanId)
        //        ?? throw new NotFoundException(ErrorMessages.LoanNotFound);

        //    if (loan.CustomerId != customerId)
        //        throw new ForbiddenException(ErrorMessages.AccessDenied);

        //    if (loan.Status != LoanStatus.Active)
        //        throw new BusinessRuleViolationException(ErrorMessages.LoanNotActive);

        //    var customer = await _customerRepo.GetByIdAsync(customerId)
        //        ?? throw new NotFoundException(ErrorMessages.CustomerNotFound);

        //    var monthlyRate = loan.InterestRate / 12 / 100;

        //    var requiredEmi = (loan.PrincipalRemaining * monthlyRate *
        //                      (decimal)Math.Pow(1 + (double)monthlyRate, loan.TenureInMonths)) /
        //                      ((decimal)Math.Pow(1 + (double)monthlyRate, loan.TenureInMonths) - 1);

        //    if (dto.Amount < requiredEmi)
        //    {
        //        customer.CreditScore -= 10;
        //    }
        //    else if (dto.Amount == requiredEmi)
        //    {
        //        customer.CreditScore += 2;
        //    }
        //    else // dto.Amount > requiredEmi
        //    {
        //        customer.CreditScore += 5;
        //    }

        //    // Clamp credit score (important)
        //    customer.CreditScore = Math.Clamp(customer.CreditScore, 300, 900);

        //    var interestDue = loan.PrincipalRemaining * monthlyRate;

        //    var interestPaid = Math.Min(dto.Amount, interestDue);
        //    var principalPaid = dto.Amount - interestPaid;

        //    if (principalPaid < 0)
        //        principalPaid = 0;

        //    loan.PrincipalRemaining -= principalPaid;

        //    if (loan.PrincipalRemaining <= 0)
        //    {
        //        loan.PrincipalRemaining = 0;
        //        loan.Status = LoanStatus.Closed;
        //    }

        //    var payment = new LoanPayment
        //    {
        //        Id = Guid.NewGuid(),
        //        LoanDetailsId = loan.Id,
        //        CustomerId = customerId,
        //        Amount = dto.Amount,
        //        PaymentDate = DateTime.UtcNow,
        //        TransactionId = Guid.NewGuid(),
        //        Status = LoanPaymentStatus.Paid
        //    };

        //    await _paymentRepo.AddAsync(payment);
        //    await _loanRepo.UpdateAsync(loan);
        //    await _customerRepo.UpdateAsync(customer);

        //    await _paymentRepo.SaveChangesAsync();
        //}

        //public async Task<LoanPaymentResponseDto> MakePaymentAsync(Guid customerId,string loanNumber, MakeLoanPaymentRequestDto dto)
        //{
        //    if (!LoanNumberRegex.IsMatch(loanNumber))
        //    {
        //        throw new BusinessRuleViolationException(ErrorMessages.WrongFormatForLoanNumber);
        //    }
        //    var loan = await _loanRepo.GetByLoanNumberWithDetailsAsync(loanNumber)
        //        ?? throw new NotFoundException(ErrorMessages.LoanNotFound);

        //    if (loan.CustomerId != customerId)
        //        throw new ForbiddenException(ErrorMessages.AccessDenied);

        //    if (loan.Status != LoanStatus.Active)
        //        throw new BusinessRuleViolationException(ErrorMessages.LoanNotActive);

        //    var customer = await _customerRepo.GetByIdAsync(customerId)
        //        ?? throw new NotFoundException(ErrorMessages.CustomerNotFound);

        //    var unpaidEmis = loan.Emis
        //        .Where(e => !e.IsPaid)
        //        .OrderBy(e => e.DueDate)
        //        .ToList();

        //    if (!unpaidEmis.Any())
        //        throw new BusinessRuleViolationException("All EMIs are already paid.");

        //    var paymentAmount = dto.Amount;
        //    var monthlyRate = loan.InterestRate / 12 / 100;

        //    foreach (var emi in unpaidEmis)
        //    {
        //        if (paymentAmount <= 0)
        //            break;

        //        //  Calculate interest on CURRENT principal
        //        var interestDue = loan.PrincipalRemaining * monthlyRate;

        //        var interestPaid = Math.Min(paymentAmount, interestDue);
        //        paymentAmount -= interestPaid;

        //        //Remaining goes to principal
        //        var principalPaid = Math.Min(paymentAmount, emi.RemainingAmount - interestPaid);
        //        if (principalPaid < 0)
        //            principalPaid = 0;

        //        paymentAmount -= principalPaid;

        //        // Update balances
        //        loan.PrincipalRemaining -= principalPaid;
        //        emi.RemainingAmount -= (interestPaid + principalPaid);

        //        if (emi.RemainingAmount <= 0)
        //        {
        //            emi.RemainingAmount = 0;
        //        }

        //        if (loan.PrincipalRemaining <= 0)
        //        {
        //            loan.PrincipalRemaining = 0;
        //            loan.Status = LoanStatus.Closed;
        //            break;
        //        }
        //    }

        //    //Credit score logic based on FIRST unpaid EMI only
        //    var firstEmi = unpaidEmis.First();

        //    if (dto.Amount < firstEmi.TotalAmount)
        //        customer.CreditScore -= 10;
        //    else if (dto.Amount == firstEmi.TotalAmount)
        //        customer.CreditScore += 2;
        //    else
        //        customer.CreditScore += 5;

        //    customer.CreditScore = Math.Clamp(customer.CreditScore, 300, 900);

        //    // Close loan if everything is paid
        //    if (loan.Emis.All(e => e.IsPaid))
        //        loan.Status = LoanStatus.Closed;

        //    var payment = new LoanPayment
        //    {
        //        Id = Guid.NewGuid(),
        //        LoanDetailsId = loan.Id,
        //        CustomerId = customerId,
        //        Amount = dto.Amount,
        //        PaymentDate = DateTime.UtcNow,
        //        TransactionId = Guid.NewGuid(),
        //        Status = LoanPaymentStatus.Paid
        //    };

        //    await _paymentRepo.AddAsync(payment);
        //    //await _customerRepo.UpdateAsync(customer);
        //    //await _loanRepo.UpdateAsync(loan);
        //    await _paymentRepo.SaveChangesAsync();

        //    return new LoanPaymentResponseDto()
        //    {
        //        PaymentDate = payment.PaymentDate,
        //        Amount = payment.Amount,
        //        TransactionId = payment.TransactionId
        //    };
        //}


        //    public async Task<NextEmiPaymentResponseDto> GetNextPaymentAsync(
        //Guid customerId,
        //Guid loanId)
        //    {
        //        var loan = await _loanRepo.GetByIdAsync(loanId)
        //            ?? throw new NotFoundException(ErrorMessages.LoanNotFound);

        //        if (loan.CustomerId != customerId)
        //            throw new ForbiddenException(ErrorMessages.AccessDenied);

        //        if (loan.Status != LoanStatus.Active)
        //            throw new BusinessRuleViolationException(ErrorMessages.LoanNotActive);

        //        //Should i check for if (monthsElapsed >= loan.TenureInMonths) throw new BusinessRuleViolationException( "All EMIs for this loan have already been completed.");

        //        var today = DateTime.UtcNow.Date;

        //        //Months elapsed since loan start
        //        var monthsElapsed =
        //            ((today.Year - loan.CreatedDate.Year) * 12) +
        //            (today.Month - loan.CreatedDate.Month);

        //        if (today.Day < loan.CreatedDate.Day)
        //            monthsElapsed--;

        //        //Next due date
        //        var nextDueDate = loan.CreatedDate.AddMonths(monthsElapsed + 1);

        //       var emiSchedule = EmiCalculator.GenerateSchedule(loan.PrincipalRemaining, loan.InterestRate, loan.TenureInMonths, nextDueDate.AddMonths(-1));

        //        return new NextEmiPaymentResponseDto
        //        {
        //            DueDate = nextDueDate,
        //            EmiAmount = emiSchedule[0].TotalEmiAmount,
        //            InterestComponent = emiSchedule[0].InterestComponent,
        //            PrincipalComponent = emiSchedule[0].PrincipalComponent,
        //            PrincipalRemainingAfterPayment =
        //                emiSchedule[0].PrincipalRemainingAfterPayment
        //        };
        //    }
        //public async Task<IEnumerable<DueEmisResponseDto>> GetDueEmisAsync(Guid customerId,string loanNumber, EmiDueStatus status)
        //{
        //    if (!LoanNumberRegex.IsMatch(loanNumber))
        //    {
        //        throw new BusinessRuleViolationException(ErrorMessages.WrongFormatForLoanNumber);
        //    }
        //    var loan = await _loanRepo.GetByLoanNumberWithDetailsAsync(loanNumber)
        //        ?? throw new NotFoundException(ErrorMessages.LoanNotFound);

        //    if (loan.CustomerId != customerId)
        //        throw new ForbiddenException(ErrorMessages.AccessDenied);

        //    if (loan.Status != LoanStatus.Active)
        //        throw new BusinessRuleViolationException(ErrorMessages.LoanNotActive);

        //    var unpaidEmis = loan.Emis
        //        .Where(e => !e.IsPaid)
        //        .OrderBy(e => e.DueDate)
        //        .ToList();

        //    if (!unpaidEmis.Any())
        //        throw new BusinessRuleViolationException("No pending EMIs.");

        //    var response = new List<DueEmisResponseDto>();

        //    var principalSnapshot = loan.PrincipalRemaining;
        //    var monthlyRate = loan.InterestRate / 12 / 100;

        //    foreach (var emi in unpaidEmis)
        //    {
        //        if (principalSnapshot <= 0)
        //            break;

        //        var interest = principalSnapshot * monthlyRate;

        //        var principal = emi.RemainingAmount - interest;
        //        if (principal < 0)
        //            principal = 0;

        //        if (principal > principalSnapshot)
        //            principal = principalSnapshot;

        //        if (status == EmiDueStatus.Overdue && emi.DueDate.Date < DateTime.UtcNow.Date)
        //        {
        //            response.Add(new DueEmisResponseDto
        //            {
        //                DueDate = emi.DueDate,
        //                EmiAmount = Math.Round(interest + principal, 2),
        //                InterestComponent = Math.Round(interest, 2),
        //                PrincipalComponent = Math.Round(principal, 2),
        //                PrincipalRemainingAfterPayment = Math.Round(principalSnapshot - principal, 2),
        //                RemainingEmiAmount = emi.RemainingAmount
        //            });
        //        }
        //        else if (status == EmiDueStatus.Upcoming && emi.DueDate.Date >= DateTime.UtcNow.Date)
        //        {

        //            response.Add(new DueEmisResponseDto
        //            {
        //                DueDate = emi.DueDate,
        //                EmiAmount = Math.Round(interest + principal, 2),
        //                InterestComponent = Math.Round(interest, 2),
        //                PrincipalComponent = Math.Round(principal, 2),
        //                PrincipalRemainingAfterPayment = Math.Round(principalSnapshot - principal, 2),
        //                RemainingEmiAmount = emi.RemainingAmount
        //            });
        //        }
                    
        //        principalSnapshot -= principal;
                
        //    }

        //    return response;
        //}
        public async Task<IEnumerable<LoanEmiGroupResponseDto>> GetAllDueEmisAsync(Guid customerId, EmiDueStatus status)
        {
            var customerLoans = await _loanRepo.GetLoansByCustomerIdWithDetailsAsync(customerId);

            // Return empty array if customer has no loans
            if (!customerLoans.Any())
                return new List<LoanEmiGroupResponseDto>();

            var activeLoans = customerLoans.Where(l => l.Status == LoanStatus.Active);

            // Return empty array if customer has no active loans
            if (!activeLoans.Any())
                return new List<LoanEmiGroupResponseDto>();

            var response = new List<LoanEmiGroupResponseDto>();

            foreach (var loan in activeLoans)
            {
                var loanNumber = loan.LoanNumber;
                var emis = await GetDueEmisAsync(customerId, loanNumber, status);
                
                // Only include loans that have EMIs matching the status
                if (emis.Any())
                {
                    response.Add(new LoanEmiGroupResponseDto
                    {
                        LoanNumber = loanNumber,
                        Emis = emis
                    });
                }
            }

            return response;            
        }
        public async Task<LoanPaymentResponseDto> MakePaymentAsync(Guid customerId,string loanNumber, MakeLoanPaymentRequestDto dto)
        {
            if (!LoanNumberRegex.IsMatch(loanNumber))
            {
                throw new BusinessRuleViolationException(ErrorMessages.WrongFormatForLoanNumber);
            }
            var loan = await _loanRepo.GetByLoanNumberWithDetailsAsync(loanNumber)
                ?? throw new NotFoundException(ErrorMessages.LoanNotFound);

            if (loan.CustomerId != customerId)
                throw new ForbiddenException(ErrorMessages.AccessDenied);

            if (loan.Status != LoanStatus.Active)
                throw new BusinessRuleViolationException(ErrorMessages.LoanNotActive);

            var customer = await _customerRepo.GetByIdAsync(customerId)
                ?? throw new NotFoundException(ErrorMessages.CustomerNotFound);

            var unpaidEmis = loan.Emis
                .Where(e => !e.IsPaid)
                .OrderBy(e => e.DueDate)
                .ToList();

            if (!unpaidEmis.Any())
                throw new BusinessRuleViolationException("All EMIs are already paid.");

            foreach (var emi in unpaidEmis)
            {
                ApplyPenaltyIfOverdue(emi);
            }

            var totalOutstanding = unpaidEmis.Sum(e => e.RemainingAmount + e.PenaltyAmount
            );

            if (dto.Amount > totalOutstanding)
            {
                throw new BusinessRuleViolationException(
                    "Payment amount exceeds total outstanding loan amount.");
            }

            ApplyPaymentToEmis(loan, unpaidEmis, dto.Amount);

            var newlyPaidEmis = unpaidEmis
                                    .Where(e => e.IsPaid) 
                                    .ToList();
            UpdateCreditScore(customer, newlyPaidEmis);
            CloseLoanIfCompleted(loan);

            var payment = new LoanPayment
            {
                Id = Guid.NewGuid(),
                LoanDetailsId = loan.Id,
                CustomerId = customerId,
                Amount = dto.Amount,
                PaymentDate = DateTime.UtcNow,
                TransactionId = Guid.NewGuid(),
                Status = LoanPaymentStatus.Paid
            };

            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveChangesAsync();

            return new LoanPaymentResponseDto
            {
                PaymentDate = payment.PaymentDate,
                Amount = payment.Amount,
                TransactionId = payment.TransactionId
            };
        }
        //private static void ApplyPaymentToEmis(LoanDetails loan, List<LoanEmi> unpaidEmis, decimal paymentAmount)
        //{
        //    var monthlyRate = loan.InterestRate / 12 / 100;

        //    foreach (var emi in unpaidEmis)
        //    {
        //        if (paymentAmount <= 0)
        //            break;

        //        var interestDue = loan.PrincipalRemaining * monthlyRate;
        //        var interestPaid = Math.Min(paymentAmount, interestDue);
        //        paymentAmount -= interestPaid;

        //        var principalPaid = Math.Min(
        //            paymentAmount,
        //            emi.RemainingAmount - interestPaid);

        //        principalPaid = Math.Max(0, principalPaid);
        //        paymentAmount -= principalPaid;

        //        loan.PrincipalRemaining -= principalPaid;
        //        emi.RemainingAmount -= (interestPaid + principalPaid);

        //        if (emi.RemainingAmount <= 0)
        //        {
        //            emi.RemainingAmount = 0;
        //        }

        //        if (loan.PrincipalRemaining <= 0)
        //        {
        //            loan.PrincipalRemaining = 0;
        //            loan.Status = LoanStatus.Closed;
        //            break;
        //        }
        //    }
        //}
        private static void ApplyPaymentToEmis(LoanDetails loan, List<LoanEmi> unpaidEmis, decimal paymentAmount)
        {
            foreach (var emi in unpaidEmis)
            {
                if (paymentAmount <= 0)
                    break;

                var penaltyPaid = Math.Min(paymentAmount, emi.PenaltyAmount);
                emi.PenaltyAmount -= penaltyPaid;
                emi.PaidPenaltyAmount += penaltyPaid;
                paymentAmount -= penaltyPaid;

                var interestPaid = Math.Min(paymentAmount, emi.InterestComponent);
                emi.InterestComponent -= interestPaid;
                paymentAmount -= interestPaid;

                var principalPaid = Math.Min(paymentAmount, emi.PrincipalComponent);
                emi.PrincipalComponent -= principalPaid;
                paymentAmount -= principalPaid;

                emi.RemainingAmount -= (interestPaid + principalPaid);
                loan.PrincipalRemaining -= principalPaid;

                if (emi.RemainingAmount < 0) emi.RemainingAmount = 0;
                if (emi.PenaltyAmount < 0) emi.PenaltyAmount = 0;
                if (loan.PrincipalRemaining < 0) loan.PrincipalRemaining = 0;

                var interestCleared = emi.InterestComponent == 0m;
                var principalCleared = emi.PrincipalComponent == 0m;
                var penaltyCleared = emi.PenaltyAmount == 0m;
                var roundingResidue = emi.RemainingAmount <= 0.01m;

                if (interestCleared && principalCleared && penaltyCleared && roundingResidue)
                {
                    emi.RemainingAmount = 0;
                    emi.PaidDate = DateTime.UtcNow;

                    if (paymentAmount <= 0.01m)
                        paymentAmount = 0;
                }
            }

            if (loan.Emis.All(e => e.IsPaid))
            {
                loan.PrincipalRemaining = 0;
                loan.Status = LoanStatus.Closed;
            }
        }
        private static void UpdateCreditScore(Customer customer, IEnumerable<LoanEmi> newlyPaidEmis)
        {
            var today = DateTime.UtcNow.Date;

            foreach (var emi in newlyPaidEmis)
            {
                if (emi.DueDate.Date < today)
                {
                    customer.CreditScore -= 10; // overdue EMI paid
                }
                else
                {
                    customer.CreditScore += 2;  // paid on time / early
                }
            }

            customer.CreditScore = Math.Clamp(customer.CreditScore, 300, 900);
        }
        private static void CloseLoanIfCompleted(LoanDetails loan)
        {
            if (loan.Emis.All(e => e.IsPaid))
                loan.Status = LoanStatus.Closed;
        }

        public async Task<IEnumerable<DueEmisResponseDto>> GetDueEmisAsync(Guid customerId, string loanNumber, EmiDueStatus status)
        {
            if (!LoanNumberRegex.IsMatch(loanNumber))
            {
                throw new BusinessRuleViolationException(ErrorMessages.WrongFormatForLoanNumber);
            }
            var loan = await _loanRepo.GetByLoanNumberWithDetailsAsync(loanNumber)
                ?? throw new NotFoundException(ErrorMessages.LoanNotFound);

            if (loan.CustomerId != customerId)
                throw new ForbiddenException(ErrorMessages.AccessDenied);

            if (loan.Status != LoanStatus.Active)
                return new List<DueEmisResponseDto>();//changes

            var unpaidEmis = loan.Emis
                .Where(e => !e.IsPaid)
                .OrderBy(e => e.DueDate)
                .ToList();

            if (!unpaidEmis.Any())
                return new List<DueEmisResponseDto>();//Changes

            foreach (var emi in unpaidEmis)
            {
                ApplyPenaltyIfOverdue(emi);
            }

            return CalculateDueEmis(loan, unpaidEmis, status);
        }
        //private static IEnumerable<DueEmisResponseDto> CalculateDueEmis( LoanDetails loan, List<LoanEmi> unpaidEmis, EmiDueStatus status)
        //{
        //    var response = new List<DueEmisResponseDto>();

        //    var principalSnapshot = loan.PrincipalRemaining;
        //    var monthlyRate = loan.InterestRate / 12 / 100;

        //    foreach (var emi in unpaidEmis)
        //    {
        //        if (principalSnapshot <= 0)
        //            break;

        //        var interest = principalSnapshot * monthlyRate;

        //        var principal = emi.RemainingAmount - interest;
        //        if (principal < 0)
        //            principal = 0;

        //        if (principal > principalSnapshot)
        //            principal = principalSnapshot;

        //        if (status == EmiDueStatus.Overdue && emi.DueDate.Date < DateTime.UtcNow.Date)
        //        {
        //            response.Add(new DueEmisResponseDto
        //            {
        //                DueDate = emi.DueDate,
        //                EmiAmount = Math.Round(interest + principal, 2),
        //                InterestComponent = Math.Round(interest, 2),
        //                PrincipalComponent = Math.Round(principal, 2),
        //                PrincipalRemainingAfterPayment = Math.Round(principalSnapshot - principal, 2),
        //                RemainingEmiAmount = emi.RemainingAmount
        //            });
        //        }
        //        else if (status == EmiDueStatus.Upcoming && emi.DueDate.Date >= DateTime.UtcNow.Date)
        //        {

        //            response.Add(new DueEmisResponseDto
        //            {
        //                DueDate = emi.DueDate,
        //                EmiAmount = Math.Round(interest + principal, 2),
        //                InterestComponent = Math.Round(interest, 2),
        //                PrincipalComponent = Math.Round(principal, 2),
        //                PrincipalRemainingAfterPayment = Math.Round(principalSnapshot - principal, 2),
        //                RemainingEmiAmount = emi.RemainingAmount
        //            });
        //        }

        //        principalSnapshot -= principal;
        //    }
        //    return response;

        //}
        private static IEnumerable<DueEmisResponseDto> CalculateDueEmis(LoanDetails loan, List<LoanEmi> unpaidEmis,EmiDueStatus status)
        {
            var today = DateTime.UtcNow.Date;

            return unpaidEmis
                .Where(e =>
                    (status == EmiDueStatus.Overdue && e.DueDate.Date < today) ||
                    (status == EmiDueStatus.Upcoming && e.DueDate.Date >= today))
                .Select(e => new DueEmisResponseDto
                {
                    DueDate = e.DueDate,
                    EmiAmount = e.RemainingAmount + e.PenaltyAmount,
                    InterestComponent = e.InterestComponent,
                    PrincipalComponent = e.PrincipalComponent,
                    RemainingEmiAmount = e.RemainingAmount,
                    PenaltyAmount = e.PenaltyAmount
                });
        }
        private static void ApplyPenaltyIfOverdue(LoanEmi emi)
        {
            if (emi.IsPaid)
                return;

            var today = DateTime.UtcNow.Date;

            if (today <= emi.DueDate.Date)
            {
                //emi.PenaltyAmount = 0;
                return;
            }

            var monthsOverdue = GetElapsedMonths(emi.DueDate.Date, today);

            const decimal monthlyPenaltyRate = 0.02m; // 2% per month

            emi.PenaltyAmount = Math.Max(0,Math.Round((emi.RemainingAmount * monthlyPenaltyRate * monthsOverdue) - emi.PaidPenaltyAmount,2));
        }
        private static int GetElapsedMonths(DateTime from, DateTime to)
        {
            if (to.Date <= from.Date)
                return 0;

            var months = (to.Year - from.Year) * 12 + (to.Month - from.Month);

            // If current day hasn't crossed due day, don't count the month yet
            if (to.Day < from.Day)
                months--;

            return Math.Max(0, months);
        }
    }
}
