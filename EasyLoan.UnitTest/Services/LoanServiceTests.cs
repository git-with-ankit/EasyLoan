using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Services;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Models.Common.Enums;
using Moq;

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
    public async Task GetLoanDetailsAsync_InvalidLoanNumber_ThrowsBusinessRuleViolation()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var invalidLoanNumber = "INVALID-123";

        // Act + Assert
        await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
            _service.GetLoanDetailsAsync(customerId, invalidLoanNumber));
    }
    [TestMethod]
    public async Task GetLoanDetailsAsync_LoanNotFound_ThrowsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var loanNumber = "LN-ABCDEFGH";

        _repoMock
            .Setup(r => r.GetByLoanNumberWithDetailsAsync(loanNumber))
            .ReturnsAsync((LoanDetails?)null);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            _service.GetLoanDetailsAsync(customerId, loanNumber));
    }
    [TestMethod]
    public async Task GetLoanDetailsAsync_NotLoanOwner_ThrowsForbidden()
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

        // Act + Assert
        await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
            _service.GetLoanDetailsAsync(customerId, loanNumber));
    }
    [TestMethod]
    public async Task GetLoanDetailsAsync_ValidRequest_ReturnsLoanDetails()
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
        Assert.AreEqual(500000, result.ApprovedAmount);
        Assert.AreEqual(350000, result.PrincipalRemaining);
        Assert.AreEqual(240, result.TenureInMonths);
        Assert.AreEqual(8.5m, result.InterestRate);
        Assert.AreEqual(LoanStatus.Active, result.Status);
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
        var result = await _service.GetCustomerLoansAsync(customerId, status);

        // Assert
        Assert.AreEqual(1, result.Count());

        var loan = result.First();
        Assert.AreEqual("LN-11111111", loan.LoanNumber);
        Assert.AreEqual(LoanStatus.Active, loan.Status);
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
        var result = await _service.GetCustomerLoansAsync(customerId, status);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
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