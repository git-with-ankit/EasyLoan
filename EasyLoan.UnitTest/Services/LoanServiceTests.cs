using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Services;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Models.Common.Enums;
using Moq;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    public class LoanServiceTests
    {
        private Mock<ILoanDetailsRepository> _repoMock = null!;
        private LoanService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _repoMock = new Mock<ILoanDetailsRepository>();
            _service = new LoanService(_repoMock.Object);
        }

        [TestMethod]
        public async Task GetLoanDetailsAsync_InvalidLoanNumber_ThrowsBusinessRuleViolation_WithCorrectMessage()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var invalidLoanNumber = "INVALID-123";

            // Act
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.GetLoanDetailsAsync(customerId, invalidLoanNumber));

            // Assert
            Assert.AreEqual(ErrorMessages.WrongFormatForLoanNumber, ex.Message);

            _repoMock.Verify(r => r.GetByLoanNumberWithDetailsAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task GetLoanDetailsAsync_LoanNotFound_ThrowsNotFound_WithCorrectMessage()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            _repoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync((LoanDetails?)null);

            // Act
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetLoanDetailsAsync(customerId, loanNumber));

            // Assert
            Assert.AreEqual(ErrorMessages.LoanNotFound, ex.Message);

            _repoMock.Verify(r => r.GetByLoanNumberWithDetailsAsync(loanNumber), Times.Once);
        }

        [TestMethod]
        public async Task GetLoanDetailsAsync_NotLoanOwner_ThrowsForbidden_WithCorrectMessage()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var otherCustomerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var loan = new LoanDetails
            {
                LoanNumber = loanNumber,
                CustomerId = otherCustomerId
            };

            _repoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            // Act
            var ex = await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.GetLoanDetailsAsync(customerId, loanNumber));

            // Assert
            Assert.AreEqual(ErrorMessages.AccessDenied, ex.Message);

            _repoMock.Verify(r => r.GetByLoanNumberWithDetailsAsync(loanNumber), Times.Once);
        }

        [TestMethod]
        public async Task GetLoanDetailsAsync_ValidRequest_ReturnsLoanDetails_AndMapsCorrectly()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var loanNumber = "LN-ABCDEFGH";

            var loan = new LoanDetails
            {
                LoanNumber = loanNumber,
                CustomerId = customerId,
                ApprovedAmount = 500000,
                PrincipalRemaining = 350000,
                TenureInMonths = 240,
                InterestRate = 8.5m,
                Status = LoanStatus.Active,
                LoanType = new LoanType
                {
                    Name = "Home Loan"
                }
            };

            _repoMock
                .Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
                .ReturnsAsync(loan);

            // Act
            var result = await _service.GetLoanDetailsAsync(customerId, loanNumber);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(loanNumber, result.LoanNumber);
            Assert.AreEqual("Home Loan", result.LoanType);
            Assert.AreEqual(loan.ApprovedAmount, result.ApprovedAmount);
            Assert.AreEqual(loan.PrincipalRemaining, result.PrincipalRemaining);
            Assert.AreEqual(loan.TenureInMonths, result.TenureInMonths);
            Assert.AreEqual(loan.InterestRate, result.InterestRate);
            Assert.AreEqual(LoanStatus.Active, result.Status);

            _repoMock.Verify(r => r.GetByLoanNumberWithDetailsAsync(loanNumber), Times.Once);
        }

        [TestMethod]
        public async Task GetCustomerLoansAsync_WhenRepoReturnsEmpty_ReturnsEmpty()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _repoMock
                .Setup(r => r.GetLoansByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(new List<LoanDetails>());

            // Act
            var result = (await _service.GetCustomerLoansAsync(customerId, LoanStatus.Active)).ToList();

            // Assert
            Assert.AreEqual(0, result.Count);

            _repoMock.Verify(r => r.GetLoansByCustomerIdWithDetailsAsync(customerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCustomerLoansAsync_WithMixedStatuses_ReturnsOnlyMatchingStatus()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var status = LoanStatus.Active;

            var loans = new List<LoanDetails>
            {
                new()
                {
                    LoanNumber = "LN-11111111",
                    Status = LoanStatus.Active,
                    PrincipalRemaining = 200000,
                    InterestRate = 8.5m
                },
                new()
                {
                    LoanNumber = "LN-22222222",
                    Status = LoanStatus.Closed,
                    PrincipalRemaining = 0,
                    InterestRate = 9.0m
                }
            };

            _repoMock
                .Setup(r => r.GetLoansByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(loans);

            // Act
            var result = (await _service.GetCustomerLoansAsync(customerId, status)).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);

            var loan = result.Single();
            Assert.AreEqual("LN-11111111", loan.LoanNumber);
            Assert.AreEqual(LoanStatus.Active, loan.Status);
            Assert.AreEqual(200000, loan.PrincipalRemaining);
            Assert.AreEqual(8.5m, loan.InterestRate);

            _repoMock.Verify(r => r.GetLoansByCustomerIdWithDetailsAsync(customerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCustomerLoansAsync_MultipleMatchingLoans_ReturnsAllMatching()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var status = LoanStatus.Active;

            var loans = new List<LoanDetails>
            {
                new()
                {
                    LoanNumber = "LN-11111111",
                    Status = LoanStatus.Active,
                    PrincipalRemaining = 200000,
                    InterestRate = 8.5m
                },
                new()
                {
                    LoanNumber = "LN-33333333",
                    Status = LoanStatus.Active,
                    PrincipalRemaining = 150000,
                    InterestRate = 7.5m
                },
                new()
                {
                    LoanNumber = "LN-22222222",
                    Status = LoanStatus.Closed,
                    PrincipalRemaining = 0,
                    InterestRate = 9.0m
                }
            };

            _repoMock
                .Setup(r => r.GetLoansByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(loans);

            // Act
            var result = (await _service.GetCustomerLoansAsync(customerId, status)).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(r => r.Status == LoanStatus.Active));

            var loanNumbers = result.Select(x => x.LoanNumber).ToList();
            CollectionAssert.Contains(loanNumbers, "LN-11111111");
            CollectionAssert.Contains(loanNumbers, "LN-33333333");
        }

        [TestMethod]
        public async Task GetCustomerLoansAsync_NoMatchingStatus_ReturnsEmpty()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var status = LoanStatus.Active;

            var loans = new List<LoanDetails>
            {
                new()
                {
                    LoanNumber = "LN-33333333",
                    Status = LoanStatus.Closed,
                    PrincipalRemaining = 0,
                    InterestRate = 9.0m
                }
            };

            _repoMock
                .Setup(r => r.GetLoansByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(loans);

            // Act
            var result = (await _service.GetCustomerLoansAsync(customerId, status)).ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetCustomerLoansAsync_ValidLoan_MapsCorrectlyToDto()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var status = LoanStatus.Active;

            var loan = new LoanDetails
            {
                LoanNumber = "LN-44444444",
                Status = LoanStatus.Active,
                PrincipalRemaining = 150000,
                InterestRate = 7.25m
            };

            _repoMock
                .Setup(r => r.GetLoansByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(new List<LoanDetails> { loan });

            // Act
            var result = await _service.GetCustomerLoansAsync(customerId, status);

            // Assert
            var dto = result.Single();

            Assert.AreEqual(loan.LoanNumber, dto.LoanNumber);
            Assert.AreEqual(loan.PrincipalRemaining, dto.PrincipalRemaining);
            Assert.AreEqual(loan.InterestRate, dto.InterestRate);
            Assert.AreEqual(loan.Status, dto.Status);
        }
    }
}
