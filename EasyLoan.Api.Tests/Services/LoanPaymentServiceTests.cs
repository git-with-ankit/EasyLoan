using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Services;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.LoanPayment;
using EasyLoan.Models.Common.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    internal class LoanPaymentServiceTests
    {
        private Mock<ILoanDetailsRepository> _mockLoanRepo = null!;
        private Mock<ILoanPaymentRepository> _mockPaymentRepo = null!;
        private Mock<ICustomerRepository> _mockCustomerRepo = null!;
        private LoanPaymentService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLoanRepo = new Mock<ILoanDetailsRepository>();
            _mockPaymentRepo = new Mock<ILoanPaymentRepository>();
            _mockCustomerRepo = new Mock<ICustomerRepository>();
            _service = new LoanPaymentService(_mockPaymentRepo.Object, _mockLoanRepo.Object, _mockCustomerRepo.Object);
            
        }
        [TestMethod]
        public async Task GetPaymentsHistoryAsync_WhenLoanNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-001";

            _mockLoanRepo
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync((LoanDetails?)null);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
                () => _service.GetPaymentsHistoryAsync(customerId, loanNumber));

            Assert.AreEqual(ErrorMessages.LoanNotFound, ex.Message);

            _mockPaymentRepo.Verify(
                r => r.GetByLoanIdAsync(It.IsAny<Guid>()),
                Times.Never);
        }
        [TestMethod]
        public async Task GetPaymentsHistoryAsync_WhenCustomerIsNotOwner_ThrowsForbiddenException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-002";

            var loan = new LoanDetails
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid() // different customer
            };

            _mockLoanRepo
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<ForbiddenException>(
                () => _service.GetPaymentsHistoryAsync(customerId, loanNumber));

            Assert.AreEqual(ErrorMessages.AccessDenied, ex.Message);

            _mockPaymentRepo.Verify(
                r => r.GetByLoanIdAsync(It.IsAny<Guid>()),
                Times.Never);
        }
        [TestMethod]
        public async Task GetPaymentsHistoryAsync_WhenNoPaidPayments_ReturnsEmptyCollection()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-003";

            var loan = new LoanDetails
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId
            };

            var payments = new List<LoanPayment>
            {
                new LoanPayment
                {
                    Status = LoanPaymentStatus.Pending,
                    Amount = 5000
                }
            };

            _mockLoanRepo
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            _mockPaymentRepo
                .Setup(r => r.GetByLoanIdAsync(loan.Id))
                .ReturnsAsync(payments);

            // Act
            var result = await _service.GetPaymentsHistoryAsync(customerId, loanNumber);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }
        [TestMethod]
        public async Task GetPaymentsHistoryAsync_WhenPaidPaymentsExist_ReturnsOrderedPaymentHistory()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-004";

            var loan = new LoanDetails
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId
            };

            var payments = new List<LoanPayment>
            {
                new LoanPayment
                {
                    PaymentDate = DateTime.UtcNow.AddDays(-1),
                    Amount = 3000,
                    Status = LoanPaymentStatus.Paid
                },
                new LoanPayment
                {
                    PaymentDate = DateTime.UtcNow.AddDays(-10),
                    Amount = 2000,
                    Status = LoanPaymentStatus.Paid
                },
             new LoanPayment
                {
                    PaymentDate = DateTime.UtcNow,
                    Amount = 4000,
                    Status = LoanPaymentStatus.Pending
                }
            };

            _mockLoanRepo
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            _mockPaymentRepo
                .Setup(r => r.GetByLoanIdAsync(loan.Id))
                .ReturnsAsync(payments);

            // Act
            var result = await _service.GetPaymentsHistoryAsync(customerId, loanNumber);

            // Assert
            Assert.AreEqual(2, result.Count());

            var ordered = result.ToList();
            Assert.IsTrue(ordered[0].PaymentDate <= ordered[1].PaymentDate);
            Assert.IsTrue(ordered.All(p => p.Status == LoanPaymentStatus.Paid));
        }
        [TestMethod]
        public async Task MakePaymentAsync_LoanNotFound_ThrowsNotFoundException()
        {
            _mockLoanRepo
                .Setup(r => r.GetByLoanNumberWithDetailsAsync("LN-1"))
                .ReturnsAsync((LoanDetails?)null);

            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.MakePaymentAsync(Guid.NewGuid(), "LN-1", new MakeLoanPaymentRequestDto { Amount = 1000 }));
        }
        [TestMethod]
        public async Task MakePaymentAsync_NotLoanOwner_ThrowsForbiddenException()
        {
            var loan = new LoanDetails { CustomerId = Guid.NewGuid(), Status = LoanStatus.Active };

            _mockLoanRepo
                .Setup(r => r.GetByLoanNumberWithDetailsAsync("LN-2"))
                .ReturnsAsync(loan);

            await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.MakePaymentAsync(Guid.NewGuid(), "LN-2", new MakeLoanPaymentRequestDto { Amount = 1000 }));
        }
        [TestMethod]
        public async Task MakePaymentAsync_LoanNotActive_ThrowsBusinessRuleViolationException()
        {
            var customerId = Guid.NewGuid();

            var loan = new LoanDetails
            {
                CustomerId = customerId,
                Status = LoanStatus.Closed
            };

            _mockLoanRepo
                .Setup(r => r.GetByLoanNumberWithDetailsAsync("LN-3"))
                .ReturnsAsync(loan);

            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.MakePaymentAsync(customerId, "LN-3", new MakeLoanPaymentRequestDto { Amount = 1000 }));
        }
        [TestMethod]
        public async Task MakePaymentAsync_CustomerNotFound_ThrowsNotFoundException()
        {
            var customerId = Guid.NewGuid();

            var loan = new LoanDetails
            {
                CustomerId = customerId,
                Status = LoanStatus.Active,
                Emis = new List<LoanEmi>()
            };

            _mockLoanRepo
                .Setup(r => r.GetByLoanNumberWithDetailsAsync("LN-4"))
                .ReturnsAsync(loan);

            _mockCustomerRepo
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.MakePaymentAsync(customerId, "LN-4", new MakeLoanPaymentRequestDto { Amount = 1000 }));
        }
        [TestMethod]
        public async Task MakePaymentAsync_NoUnpaidEmis_ThrowsBusinessRuleViolationException()
        {
            var customerId = Guid.NewGuid();

            var loan = new LoanDetails
            {
                CustomerId = customerId,
                Status = LoanStatus.Active,
                Emis = new List<LoanEmi>
        {
            new LoanEmi { RemainingAmount = 0 }
        }
            };

            _mockLoanRepo
                .Setup(r => r.GetByLoanNumberWithDetailsAsync("LN-5"))
                .ReturnsAsync(loan);

            _mockCustomerRepo
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(new Customer());

            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.MakePaymentAsync(customerId, "LN-5", new MakeLoanPaymentRequestDto { Amount = 1000 }));
        }
        [TestMethod]
        public async Task MakePaymentAsync_ValidPayment_PersistsAndReturnsResponse()
        {
            var customerId = Guid.NewGuid();

            var emi = new LoanEmi
            {
                RemainingAmount = 5000,
                TotalAmount = 5000,
                DueDate = DateTime.UtcNow
            };

            var loan = new LoanDetails
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Status = LoanStatus.Active,
                PrincipalRemaining = 5000,
                InterestRate = 12,
                Emis = new List<LoanEmi> { emi }
            };

            var customer = new Customer { CreditScore = 600 };

            _mockLoanRepo
                .Setup(r => r.GetByLoanNumberWithDetailsAsync("LN-6"))
                .ReturnsAsync(loan);

            _mockCustomerRepo
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _mockPaymentRepo
                .Setup(r => r.AddAsync(It.IsAny<LoanPayment>()))
                .Returns(Task.CompletedTask);

            //_mockCustomerRepo
            //    .Setup(r => r.UpdateAsync(customer))
            //    .Returns(Task.CompletedTask);

            //_mockLoanRepo
            //    .Setup(r => r.UpdateAsync(loan))
            //    .Returns(Task.CompletedTask);

            _mockPaymentRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _service.MakePaymentAsync(
                customerId,
                "LN-6",
                new MakeLoanPaymentRequestDto { Amount = 5000 });

            Assert.IsNotNull(result);
            Assert.AreEqual(5000, result.Amount);
            Assert.AreNotEqual(Guid.Empty, result.TransactionId);

            _mockPaymentRepo.Verify(r => r.AddAsync(It.IsAny<LoanPayment>()), Times.Once);
            //_mockLoanRepo.Verify(r => r.UpdateAsync(loan), Times.Once);
            //_mockCustomerRepo.Verify(r => r.UpdateAsync(customer), Times.Once);
        }

    }
}
