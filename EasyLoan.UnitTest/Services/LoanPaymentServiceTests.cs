using EasyLoan.Business.Services;
using EasyLoan.Business.Exceptions;
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
                _customerRepoMock.Object);
        }

        [TestMethod]
        public async Task GetPaymentsHistoryAsync_InvalidLoanNumber_ThrowsBusinessRuleViolation()
        {
            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.GetPaymentsHistoryAsync(Guid.NewGuid(), "INVALID"));
        }

        [TestMethod]
        public async Task GetPaymentsHistoryAsync_LoanNotFound_ThrowsNotFound()
        {
            _loanRepoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((LoanDetails?)null);

            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetPaymentsHistoryAsync(Guid.NewGuid(), "LN-ABCDEFGH"));
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

            await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.GetPaymentsHistoryAsync(Guid.NewGuid(), "LN-ABCDEFGH"));
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
                new() { Status = LoanPaymentStatus.Pending, Amount = 3000 },
                new() { Status = LoanPaymentStatus.Paid, Amount = 1000, PaymentDate = DateTime.UtcNow.AddDays(-3) }
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                         .ReturnsAsync(loan);

            _paymentRepoMock.Setup(r => r.GetByLoanIdAsync(loanId))
                            .ReturnsAsync(payments);

            var result = (await _service.GetPaymentsHistoryAsync(customerId, "LN-ABCDEFGH")).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0].PaymentDate < result[1].PaymentDate);
        }

        [TestMethod]
        public async Task GetDueEmisAsync_InvalidLoanNumber_ThrowsBusinessRuleViolation()
        {
            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.GetDueEmisAsync(Guid.NewGuid(), "INVALID", EmiDueStatus.Upcoming));
        }

        [TestMethod]
        public async Task GetDueEmisAsync_LoanNotFound_ThrowsNotFound()
        {
            _loanRepoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((LoanDetails?)null);

            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetDueEmisAsync(Guid.NewGuid(), "LN-ABCDEFGH", EmiDueStatus.Upcoming));
        }

        [TestMethod]
        public async Task GetDueEmisAsync_LoanNotActive_ThrowsBusinessRuleViolation()
        {
            var loan = new LoanDetails
            {
                CustomerId = Guid.NewGuid(),
                Status = LoanStatus.Closed
            };

            _loanRepoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(loan);

            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.GetDueEmisAsync(loan.CustomerId, "LN-ABCDEFGH", EmiDueStatus.Upcoming));
        }

        [TestMethod]
        public async Task GetDueEmisAsync_Overdue_ReturnsOnlyPastEmis()
        {
            var customerId = Guid.NewGuid();

            var loan = new LoanDetails
            {
                CustomerId = customerId,
                Status = LoanStatus.Active,
                InterestRate = 12,
                PrincipalRemaining = 10000,
                Emis = new List<LoanEmi>
                {
                    new() { DueDate = DateTime.UtcNow.AddDays(-5), RemainingAmount = 2000 },
                    new() { DueDate = DateTime.UtcNow.AddDays(5), RemainingAmount = 2000 }
                }
            };

            _loanRepoMock.Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                         .ReturnsAsync(loan);

            var result = (await _service.GetDueEmisAsync(customerId, "LN-ABCDEFGH", EmiDueStatus.Overdue)).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].DueDate < DateTime.UtcNow);
        }

        [TestMethod]
        public async Task GetAllDueEmisAsync_NoLoans_ThrowsNotFound()
        {
            _loanRepoMock
                .Setup(r => r.GetLoansByCustomerIdWithDetailsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Enumerable.Empty<LoanDetails>());

            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetAllDueEmisAsync(Guid.NewGuid(), EmiDueStatus.Upcoming));
        }

        [TestMethod]
        public async Task GetAllDueEmisAsync_ActiveLoans_ReturnsGroupedResults()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var loan1 = new LoanDetails
            {
                LoanNumber = "LN-11111111",
                CustomerId = customerId,
                Status = LoanStatus.Active,
                InterestRate = 12,
                PrincipalRemaining = 5000,
                Emis = new List<LoanEmi>
                {
                    new LoanEmi
                    {
                        DueDate = DateTime.UtcNow.AddDays(5),
                        RemainingAmount = 1000
                    }
                }
            };

            var loan2 = new LoanDetails
            {
                LoanNumber = "LN-22222222",
                CustomerId = customerId,
                Status = LoanStatus.Active,
                InterestRate = 12,
                PrincipalRemaining = 6000,
                Emis = new List<LoanEmi>
                {
                    new LoanEmi
                    {
                        DueDate = DateTime.UtcNow.AddDays(7),
                        RemainingAmount = 1500
                    }
                }
            };

            _loanRepoMock
                .Setup(r => r.GetLoansByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(new[] { loan1, loan2 });

            _loanRepoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((string ln) =>
                    ln == loan1.LoanNumber ? loan1 :
                    ln == loan2.LoanNumber ? loan2 : null);

            // Act
            var result = (await _service.GetAllDueEmisAsync(customerId, EmiDueStatus.Upcoming))
                .ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(group => group.Any()));
        }


        //[TestMethod]
        //public async Task MakePaymentAsync_AllEmisPaid_ClosesLoan()
        //{
        //    // Arrange
        //    var customerId = Guid.NewGuid();
        //    var loanNumber = "LN-ABCDEFGH";

        //    var emi = new LoanEmi
        //    {
        //        DueDate = DateTime.UtcNow.AddDays(1),
        //        RemainingAmount = 500
        //    };

        //    var loan = new LoanDetails
        //    {
        //        Id = Guid.NewGuid(),
        //        LoanNumber = loanNumber,
        //        CustomerId = customerId,
        //        Status = LoanStatus.Active,
        //        InterestRate = 10,
        //        PrincipalRemaining = 500,
        //        Emis = new List<LoanEmi> { emi }
        //    };

        //    var customer = new Customer
        //    {
        //        Id = customerId,
        //        CreditScore = 650
        //    };

        //    _loanRepoMock
        //        .Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
        //        .ReturnsAsync(loan);

        //    _customerRepoMock
        //        .Setup(r => r.GetByIdAsync(customerId))
        //        .ReturnsAsync(customer);

        //    _paymentRepoMock
        //        .Setup(r => r.AddAsync(It.IsAny<LoanPayment>()))
        //        .Returns(Task.CompletedTask);

        //    _paymentRepoMock
        //        .Setup(r => r.SaveChangesAsync())
        //        .Returns(Task.CompletedTask);

        //    var dto = new MakeLoanPaymentRequestDto
        //    {
        //        Amount = 500 // EXACT outstanding
        //    };

        //    // Act
        //    await _service.MakePaymentAsync(customerId, loanNumber, dto);

        //    // Assert
        //    Assert.AreEqual(LoanStatus.Closed, loan.Status);
        //    Assert.AreEqual(0, loan.PrincipalRemaining);
        //}
    }
}
