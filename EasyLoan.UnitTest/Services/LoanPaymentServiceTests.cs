using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Services;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.LoanPayment;
using EasyLoan.Models.Common.Enums;
using Moq;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    public class LoanPaymentServiceTests
    {
        private Mock<ILoanPaymentRepository> _paymentRepoMock = null!;
        private Mock<ILoanDetailsRepository> _loanRepoMock = null!;
        private Mock<ICustomerRepository> _customerRepoMock = null!;
        private LoanPaymentService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _paymentRepoMock = new Mock<ILoanPaymentRepository>();
            _loanRepoMock = new Mock<ILoanDetailsRepository>();
            _customerRepoMock = new Mock<ICustomerRepository>();

            _service = new LoanPaymentService(
                _paymentRepoMock.Object,
                _loanRepoMock.Object,
                _customerRepoMock.Object
            );
        }

        [TestMethod]
        public async Task GetPaymentsHistoryAsync_InvalidLoanNumber_ThrowsBusinessRuleViolation()
        {
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.GetPaymentsHistoryAsync(Guid.NewGuid(), "INVALID"));

            Assert.AreEqual(ErrorMessages.WrongFormatForLoanNumber, ex.Message);
        }

        [TestMethod]
        public async Task GetPaymentsHistoryAsync_LoanNotFound_ThrowsNotFound()
        {
            _loanRepoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((LoanDetails?)null);

            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetPaymentsHistoryAsync(Guid.NewGuid(), "LN-ABCDEFGH"));

            Assert.AreEqual(ErrorMessages.LoanNotFound, ex.Message);
        }

        [TestMethod]
        public async Task GetPaymentsHistoryAsync_LoanDoesNotBelongToCustomer_ThrowsForbidden()
        {
            var loan = new LoanDetails
            {
                CustomerId = Guid.NewGuid()
            };

            _loanRepoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(loan);

            var ex = await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.GetPaymentsHistoryAsync(Guid.NewGuid(), "LN-ABCDEFGH"));

            Assert.AreEqual(ErrorMessages.AccessDenied, ex.Message);
        }

        [TestMethod]
        public async Task GetPaymentsHistoryAsync_ReturnsOnlyPaidPayments_OrderedByDate()
        {
            var customerId = Guid.NewGuid();
            var loanId = Guid.NewGuid();

            var loan = new LoanDetails
            {
                Id = loanId,
                CustomerId = customerId
            };

            var payments = new List<LoanPayment>
            {
                new() { Status = LoanPaymentStatus.Paid, Amount = 2000, PaymentDate = DateTime.UtcNow.AddDays(-1) },
                new() { Status = LoanPaymentStatus.Pending, Amount = 3000, PaymentDate = DateTime.UtcNow.AddDays(-2) },
                new() { Status = LoanPaymentStatus.Paid, Amount = 1000, PaymentDate = DateTime.UtcNow.AddDays(-3) }
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(loan);

            _paymentRepoMock.Setup(r => r.GetByLoanIdAsync(loanId))
                .ReturnsAsync(payments);

            var result = (await _service.GetPaymentsHistoryAsync(customerId, "LN-ABCDEFGH")).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0].PaymentDate < result[1].PaymentDate);
            Assert.AreEqual(1000, result[0].Amount);
            Assert.AreEqual(2000, result[1].Amount);
        }

        [TestMethod]
        public async Task GetPaymentsHistoryAsync_WhenNoPaidPayments_ReturnsEmpty()
        {
            var customerId = Guid.NewGuid();
            var loanId = Guid.NewGuid();

            var loan = new LoanDetails
            {
                Id = loanId,
                CustomerId = customerId
            };

            var payments = new List<LoanPayment>
            {
                new() { Status = LoanPaymentStatus.Pending, Amount = 2000, PaymentDate = DateTime.UtcNow.AddDays(-1) },
                new() { Status = LoanPaymentStatus.Pending, Amount = 1000, PaymentDate = DateTime.UtcNow.AddDays(-3) }
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(loan);

            _paymentRepoMock.Setup(r => r.GetByLoanIdAsync(loanId))
                .ReturnsAsync(payments);

            var result = (await _service.GetPaymentsHistoryAsync(customerId, "LN-ABCDEFGH")).ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetDueEmisAsync_InvalidLoanNumber_ThrowsBusinessRuleViolation()
        {
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.GetDueEmisAsync(Guid.NewGuid(), "INVALID", EmiDueStatus.Upcoming));

            Assert.AreEqual(ErrorMessages.WrongFormatForLoanNumber, ex.Message);
        }

        [TestMethod]
        public async Task GetDueEmisAsync_LoanNotFound_ThrowsNotFound()
        {
            _loanRepoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((LoanDetails?)null);

            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetDueEmisAsync(Guid.NewGuid(), "LN-ABCDEFGH", EmiDueStatus.Upcoming));

            Assert.AreEqual(ErrorMessages.LoanNotFound, ex.Message);
        }

        [TestMethod]
        public async Task GetDueEmisAsync_LoanDoesNotBelongToCustomer_ThrowsForbidden()
        {
            var customerId = Guid.NewGuid();

            var loan = new LoanDetails
            {
                LoanNumber = "LN-ABCDEFGH",
                CustomerId = Guid.NewGuid(),
                Status = LoanStatus.Active
            };

            _loanRepoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(loan.LoanNumber))
                .ReturnsAsync(loan);

            var ex = await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.GetDueEmisAsync(customerId, loan.LoanNumber, EmiDueStatus.Upcoming));

            Assert.AreEqual(ErrorMessages.AccessDenied, ex.Message);
        }

        [TestMethod]
        public async Task GetDueEmisAsync_LoanNotActive_ReturnsEmptyList()
        {
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var loan = new LoanDetails
            {
                LoanNumber = loanNumber,
                CustomerId = customerId,
                Status = LoanStatus.Closed,
                Emis = new List<LoanEmi>()
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            var result = (await _service.GetDueEmisAsync(customerId, loanNumber, EmiDueStatus.Upcoming)).ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetDueEmisAsync_NoUnpaidEmis_ReturnsEmptyList()
        {
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var loan = new LoanDetails
            {
                LoanNumber = loanNumber,
                CustomerId = customerId,
                Status = LoanStatus.Active,
                Emis = new List<LoanEmi>
                {
                    new LoanEmi
                    {
                        DueDate = DateTime.UtcNow.AddMonths(-1),
                        RemainingAmount = 0,
                        PenaltyAmount = 0,
                        InterestComponent = 0,
                        PrincipalComponent = 0,
                        PaidDate = DateTime.UtcNow.AddDays(-1)
                    }
                }
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            var result = (await _service.GetDueEmisAsync(customerId, loanNumber, EmiDueStatus.Overdue)).ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetDueEmisAsync_Overdue_ReturnsOnlyOverdue_AndAppliesPenalty()
        {
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var emi1 = new LoanEmi
            {
                DueDate = DateTime.UtcNow.AddMonths(-2),
                RemainingAmount = 1000,
                InterestComponent = 200,
                PrincipalComponent = 800,
                PenaltyAmount = 0,
                PaidPenaltyAmount = 0
            };

            var emi2 = new LoanEmi
            {
                DueDate = DateTime.UtcNow.AddDays(10),
                RemainingAmount = 1000,
                InterestComponent = 250,
                PrincipalComponent = 750,
                PenaltyAmount = 0
            };

            var loan = new LoanDetails
            {
                LoanNumber = loanNumber,
                CustomerId = customerId,
                Status = LoanStatus.Active,
                InterestRate = 12,
                PrincipalRemaining = 5000,
                Emis = new List<LoanEmi> { emi1, emi2 }
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            var result = (await _service.GetDueEmisAsync(customerId, loanNumber, EmiDueStatus.Overdue)).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(emi1.DueDate.Date, result[0].DueDate.Date);
            Assert.IsTrue(result[0].PenaltyAmount > 0);
        }

        [TestMethod]
        public async Task GetDueEmisAsync_Upcoming_ReturnsOnlyUpcoming_NoPenaltyApplied()
        {
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var emi1 = new LoanEmi
            {
                DueDate = DateTime.UtcNow.AddMonths(-2),
                RemainingAmount = 1000,
                InterestComponent = 200,
                PrincipalComponent = 800,
                PenaltyAmount = 0
            };

            var emi2 = new LoanEmi
            {
                DueDate = DateTime.UtcNow.AddDays(10),
                RemainingAmount = 1000,
                InterestComponent = 250,
                PrincipalComponent = 750,
                PenaltyAmount = 0
            };

            var loan = new LoanDetails
            {
                LoanNumber = loanNumber,
                CustomerId = customerId,
                Status = LoanStatus.Active,
                InterestRate = 12,
                PrincipalRemaining = 5000,
                Emis = new List<LoanEmi> { emi1, emi2 }
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            var result = (await _service.GetDueEmisAsync(customerId, loanNumber, EmiDueStatus.Upcoming)).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(emi2.DueDate.Date, result[0].DueDate.Date);

            Assert.AreEqual(0, emi2.PenaltyAmount);
        }

        [TestMethod]
        public async Task GetAllDueEmisAsync_NoLoans_ReturnsEmpty()
        {
            _loanRepoMock
                .Setup(r => r.GetLoansByCustomerIdWithDetailsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Enumerable.Empty<LoanDetails>());

            var result = (await _service.GetAllDueEmisAsync(Guid.NewGuid(), EmiDueStatus.Upcoming)).ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllDueEmisAsync_NoActiveLoans_ReturnsEmpty()
        {
            var customerId = Guid.NewGuid();

            _loanRepoMock
                .Setup(r => r.GetLoansByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(new List<LoanDetails>
                {
                    new LoanDetails { CustomerId = customerId, Status = LoanStatus.Closed, LoanNumber = "LN-11111111" }
                });

            var result = (await _service.GetAllDueEmisAsync(customerId, EmiDueStatus.Upcoming)).ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllDueEmisAsync_ActiveLoans_OnlyIncludesLoansWithMatchingEmis()
        {
            var customerId = Guid.NewGuid();

            var loan1 = new LoanDetails
            {
                LoanNumber = "LN-11111111",
                CustomerId = customerId,
                Status = LoanStatus.Active,
                Emis = new List<LoanEmi>
                {
                    new()
                    {
                        DueDate = DateTime.UtcNow.AddDays(5),
                        InterestComponent = 100,
                        PrincipalComponent = 900,
                        RemainingAmount = 1000,
                        PenaltyAmount = 0
                    }
                }
            };

            var loan2 = new LoanDetails
            {
                LoanNumber = "LN-22222222",
                CustomerId = customerId,
                Status = LoanStatus.Active,
                Emis = new List<LoanEmi>
                {
                    new()
                    {
                        DueDate = DateTime.UtcNow.AddMonths(-2),
                        InterestComponent = 100,
                        PrincipalComponent = 900,
                        RemainingAmount = 1000,
                        PenaltyAmount = 0
                    }
                }
            };

            _loanRepoMock
                .Setup(r => r.GetLoansByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(new[] { loan1, loan2 });

            // LoanPaymentService calls GetDueEmisAsync internally, which calls this repo method
            _loanRepoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((string ln) => ln == loan1.LoanNumber ? loan1 : loan2);

            // Act: ask only Upcoming
            var result = (await _service.GetAllDueEmisAsync(customerId, EmiDueStatus.Upcoming)).ToList();

            // Assert: loan1 should appear, loan2 should not
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("LN-11111111", result[0].LoanNumber);
            Assert.AreEqual(1, result[0].Emis.Count());
        }

        [TestMethod]
        public async Task MakePaymentAsync_InvalidLoanNumber_ThrowsBusinessRuleViolation()
        {
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.MakePaymentAsync(Guid.NewGuid(), "INVALID", new MakeLoanPaymentRequestDto { Amount = 100 }));

            Assert.AreEqual(ErrorMessages.WrongFormatForLoanNumber, ex.Message);
        }

        [TestMethod]
        public async Task MakePaymentAsync_LoanNotFound_ThrowsNotFound()
        {
            _loanRepoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((LoanDetails?)null);

            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.MakePaymentAsync(Guid.NewGuid(), "LN-ABCDEFGH", new MakeLoanPaymentRequestDto { Amount = 100 }));

            Assert.AreEqual(ErrorMessages.LoanNotFound, ex.Message);
        }

        [TestMethod]
        public async Task MakePaymentAsync_LoanDoesNotBelongToCustomer_ThrowsForbidden()
        {
            var loan = new LoanDetails
            {
                LoanNumber = "LN-ABCDEFGH",
                CustomerId = Guid.NewGuid(),
                Status = LoanStatus.Active
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(loan.LoanNumber))
                .ReturnsAsync(loan);

            var ex = await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.MakePaymentAsync(Guid.NewGuid(), loan.LoanNumber, new MakeLoanPaymentRequestDto { Amount = 100 }));

            Assert.AreEqual(ErrorMessages.AccessDenied, ex.Message);
        }

        [TestMethod]
        public async Task MakePaymentAsync_LoanNotActive_ThrowsBusinessRuleViolation()
        {
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var loan = new LoanDetails
            {
                LoanNumber = loanNumber,
                CustomerId = customerId,
                Status = LoanStatus.Closed
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.MakePaymentAsync(customerId, loanNumber, new MakeLoanPaymentRequestDto { Amount = 100 }));

            Assert.AreEqual(ErrorMessages.LoanNotActive, ex.Message);
        }

        [TestMethod]
        public async Task MakePaymentAsync_CustomerNotFound_ThrowsNotFound()
        {
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var loan = new LoanDetails
            {
                LoanNumber = loanNumber,
                CustomerId = customerId,
                Status = LoanStatus.Active,
                Emis = new List<LoanEmi>
                {
                    new LoanEmi
                    {
                        DueDate = DateTime.UtcNow.AddDays(10),
                        RemainingAmount = 500,
                        InterestComponent = 100,
                        PrincipalComponent = 400
                    }
                }
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.MakePaymentAsync(customerId, loanNumber, new MakeLoanPaymentRequestDto { Amount = 100 }));

            Assert.AreEqual(ErrorMessages.CustomerNotFound, ex.Message);
        }

        [TestMethod]
        public async Task MakePaymentAsync_WhenAllEmisAlreadyPaid_ThrowsBusinessRuleViolation()
        {
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var loan = new LoanDetails
            {
                LoanNumber = loanNumber,
                CustomerId = customerId,
                Status = LoanStatus.Active,
                Emis = new List<LoanEmi>
                {
                    new LoanEmi
                    {
                        DueDate = DateTime.UtcNow.AddMonths(-1),
                        RemainingAmount = 0,
                        InterestComponent = 0,
                        PrincipalComponent = 0,
                        PaidDate = DateTime.UtcNow.AddDays(-1)
                    }
                }
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(new Customer { Id = customerId, CreditScore = 650 });

            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.MakePaymentAsync(customerId, loanNumber, new MakeLoanPaymentRequestDto { Amount = 100 }));

            Assert.AreEqual("All EMIs are already paid.", ex.Message);
        }

        [TestMethod]
        public async Task MakePaymentAsync_WhenPaymentExceedsOutstanding_ThrowsBusinessRuleViolation()
        {
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var emi = new LoanEmi
            {
                DueDate = DateTime.UtcNow.AddDays(5),
                RemainingAmount = 500,
                InterestComponent = 100,
                PrincipalComponent = 400,
                PenaltyAmount = 0
            };

            var loan = new LoanDetails
            {
                Id = Guid.NewGuid(),
                LoanNumber = loanNumber,
                CustomerId = customerId,
                Status = LoanStatus.Active,
                PrincipalRemaining = 400,
                Emis = new List<LoanEmi> { emi }
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(new Customer { Id = customerId, CreditScore = 650 });

            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.MakePaymentAsync(customerId, loanNumber, new MakeLoanPaymentRequestDto { Amount = 9999 }));

            Assert.AreEqual("Payment amount exceeds total outstanding loan amount.", ex.Message);
        }

        [TestMethod]
        public async Task MakePaymentAsync_PaysPenaltyThenInterestThenPrincipal_AndCreatesPayment()
        {
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var emi = new LoanEmi
            {
                DueDate = DateTime.UtcNow.AddMonths(-2),
                RemainingAmount = 500m,
                InterestComponent = 100m,
                PrincipalComponent = 400m,
                PenaltyAmount = 0m,
                PaidPenaltyAmount = 0m
            };

            var loan = new LoanDetails
            {
                Id = Guid.NewGuid(),
                LoanNumber = loanNumber,
                CustomerId = customerId,
                Status = LoanStatus.Active,
                PrincipalRemaining = 400m,
                Emis = new List<LoanEmi> { emi }
            };

            var customer = new Customer
            {
                Id = customerId,
                CreditScore = 650
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            LoanPayment? savedPayment = null;

            _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<LoanPayment>()))
                .Callback<LoanPayment>(p => savedPayment = p)
                .Returns(Task.CompletedTask);

            _paymentRepoMock.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var expectedPenalty = Math.Round(emi.RemainingAmount * 0.02m * 2, 2);
            var expectedTotalOutstanding = emi.RemainingAmount + expectedPenalty;

            var dto = new MakeLoanPaymentRequestDto
            {
                Amount = expectedTotalOutstanding
            };

            await _service.MakePaymentAsync(customerId, loanNumber, dto);

            Assert.IsNotNull(savedPayment);
            Assert.AreEqual(dto.Amount, savedPayment!.Amount);
            Assert.AreEqual(customerId, savedPayment.CustomerId);
            Assert.AreEqual(LoanPaymentStatus.Paid, savedPayment.Status);

            Assert.AreEqual(0m, emi.InterestComponent);
            Assert.AreEqual(0m, emi.PrincipalComponent);
            Assert.AreEqual(0m, emi.RemainingAmount);
            Assert.AreEqual(0m, emi.PenaltyAmount);
            Assert.IsNotNull(emi.PaidDate);

            Assert.AreEqual(0m, loan.PrincipalRemaining);
            Assert.AreEqual(LoanStatus.Closed, loan.Status);

            _paymentRepoMock.Verify(r => r.AddAsync(It.IsAny<LoanPayment>()), Times.Once);
            _paymentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }


        [TestMethod]
        public async Task MakePaymentAsync_PaysUpcomingEmi_IncreasesCreditScore()
        {
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var emi = new LoanEmi
            {
                DueDate = DateTime.UtcNow.AddDays(5), 
                RemainingAmount = 500,
                InterestComponent = 100,
                PrincipalComponent = 400,
                PenaltyAmount = 0
            };

            var loan = new LoanDetails
            {
                Id = Guid.NewGuid(),
                LoanNumber = loanNumber,
                CustomerId = customerId,
                Status = LoanStatus.Active,
                PrincipalRemaining = 400,
                Emis = new List<LoanEmi> { emi }
            };

            var customer = new Customer
            {
                Id = customerId,
                CreditScore = 650
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<LoanPayment>()))
                .Returns(Task.CompletedTask);

            _paymentRepoMock.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var dto = new MakeLoanPaymentRequestDto { Amount = 500 };

            await _service.MakePaymentAsync(customerId, loanNumber, dto);

            // Assert
            Assert.IsTrue(emi.IsPaid);
            Assert.AreEqual(LoanStatus.Closed, loan.Status);
            Assert.IsTrue(customer.CreditScore > 650); // should increase by +2
        }

        [TestMethod]
        public async Task MakePaymentAsync_PartialPayment_DoesNotMarkEmiPaid_AndLoanRemainsActive()
        {
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var emi = new LoanEmi
            {
                DueDate = DateTime.UtcNow.AddDays(5),
                RemainingAmount = 500,
                InterestComponent = 100,
                PrincipalComponent = 400,
                PenaltyAmount = 0
            };

            var loan = new LoanDetails
            {
                Id = Guid.NewGuid(),
                LoanNumber = loanNumber,
                CustomerId = customerId,
                Status = LoanStatus.Active,
                PrincipalRemaining = 400,
                Emis = new List<LoanEmi> { emi }
            };

            var customer = new Customer
            {
                Id = customerId,
                CreditScore = 650
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<LoanPayment>()))
                .Returns(Task.CompletedTask);

            _paymentRepoMock.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);
          
            var dto = new MakeLoanPaymentRequestDto { Amount = 50 };

            await _service.MakePaymentAsync(customerId, loanNumber, dto);

            // Assert
            Assert.IsFalse(emi.IsPaid);
            Assert.AreEqual(LoanStatus.Active, loan.Status);

            Assert.AreEqual(650, customer.CreditScore);
        }
    }
}
