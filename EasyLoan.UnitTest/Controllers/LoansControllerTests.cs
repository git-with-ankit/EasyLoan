using EasyLoan.Api.Controllers;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Loan;
using EasyLoan.Dtos.LoanPayment;
using EasyLoan.Models.Common.Enums;
using EasyLoan.UnitTest.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace EasyLoan.UnitTest.Controllers
{
    [TestClass]
    public class LoansControllerTests
    {
        private Mock<ILoanService> _mockLoanService = null!;
        private Mock<ILoanPaymentService> _mockPaymentService = null!;
        private LoansController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLoanService = new Mock<ILoanService>(MockBehavior.Strict);
            _mockPaymentService = new Mock<ILoanPaymentService>(MockBehavior.Strict);

            _controller = new LoansController(
                _mockLoanService.Object,
                _mockPaymentService.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [TestMethod]
        public async Task GetCustomerLoans_ValidStatusForCustomer_ReturnsOkWithLoans()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var status = LoanStatus.Active;

            var loans = new List<LoanSummaryResponseDto>
            {
                new LoanSummaryResponseDto
                {
                    LoanNumber = "LN-001",
                    PrincipalRemaining = 500000,
                    InterestRate = 7.5m,
                    Status = LoanStatus.Active
                }
            };

            _mockLoanService
                .Setup(s => s.GetCustomerLoansAsync(customerId, status))
                .ReturnsAsync(loans);

            // Act
            var result = await _controller.GetCustomerLoans(status);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var data = ok.Value as IEnumerable<LoanSummaryResponseDto>;
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data.Count());

            _mockLoanService.Verify(s => s.GetCustomerLoansAsync(customerId, status), Times.Once);
            _mockLoanService.VerifyNoOtherCalls();
            _mockPaymentService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetDetails_ValidLoanNumberForCustomer_ReturnsOkWithLoanDetails()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var loanNumber = "LN-ABCDEFGH";

            var loan = new LoanDetailsResponseDto
            {
                LoanNumber = loanNumber,
                LoanType = "Home Loan",
                ApprovedAmount = 1000000,
                PrincipalRemaining = 800000,
                TenureInMonths = 240,
                InterestRate = 7.5m,
                Status = LoanStatus.Active
            };

            _mockLoanService
                .Setup(s => s.GetLoanDetailsAsync(customerId, loanNumber))
                .ReturnsAsync(loan);

            // Act
            var result = await _controller.GetDetails(loanNumber);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var data = ok.Value as LoanDetailsResponseDto;
            Assert.IsNotNull(data);
            Assert.AreEqual(loanNumber, data.LoanNumber);

            _mockLoanService.Verify(s => s.GetLoanDetailsAsync(customerId, loanNumber), Times.Once);
            _mockLoanService.VerifyNoOtherCalls();
            _mockPaymentService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task MakePayment_ValidRequestForCustomer_ReturnsOkWithPaymentResponse()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var loanNumber = "LN-ABCDEFGH";
            var request = new MakeLoanPaymentRequestDto { Amount = 10000 };

            var response = new LoanPaymentResponseDto
            {
                Amount = 10000,
                TransactionId = Guid.NewGuid(),
                PaymentDate = DateTime.UtcNow
            };

            _mockPaymentService
                .Setup(s => s.MakePaymentAsync(customerId, loanNumber, request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.MakePayment(loanNumber, request);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var data = ok.Value as LoanPaymentResponseDto;
            Assert.IsNotNull(data);
            Assert.AreEqual(10000, data.Amount);

            _mockPaymentService.Verify(
                s => s.MakePaymentAsync(customerId, loanNumber, request),
                Times.Once);

            _mockPaymentService.VerifyNoOtherCalls();
            _mockLoanService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetAllDueEmisAsync_UpcomingStatus_ReturnsOkWithLoanGroups()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var status = EmiDueStatus.Upcoming;

            var groups = new List<LoanEmiGroupResponseDto>
            {
                new LoanEmiGroupResponseDto
                {
                    LoanNumber = "LN-11111111",
                    Emis = new List<DueEmisResponseDto>
                    {
                        new DueEmisResponseDto
                        {
                            DueDate = DateTime.UtcNow.AddDays(10),
                            EmiAmount = 5000,
                            InterestComponent = 2000,
                            PrincipalComponent = 3000,
                            RemainingEmiAmount = 5000,
                            PenaltyAmount = 0
                        }
                    }
                },
                new LoanEmiGroupResponseDto
                {
                    LoanNumber = "LN-22222222",
                    Emis = new List<DueEmisResponseDto>
                    {
                        new DueEmisResponseDto
                        {
                            DueDate = DateTime.UtcNow.AddDays(20),
                            EmiAmount = 8000,
                            InterestComponent = 3000,
                            PrincipalComponent = 5000,
                            RemainingEmiAmount = 8000,
                            PenaltyAmount = 0
                        }
                    }
                }
            };

            _mockPaymentService
                .Setup(s => s.GetAllDueEmisAsync(customerId, status))
                .ReturnsAsync(groups);

            // Act
            var result = await _controller.GetAllDueEmisAsync(status);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var data = ok.Value as IEnumerable<LoanEmiGroupResponseDto>;
            Assert.IsNotNull(data);

            var list = data.ToList();
            Assert.AreEqual(2, list.Count);

            Assert.AreEqual("LN-11111111", list[0].LoanNumber);
            Assert.AreEqual(1, list[0].Emis.Count());

            _mockPaymentService.Verify(s => s.GetAllDueEmisAsync(customerId, status), Times.Once);
            _mockPaymentService.VerifyNoOtherCalls();
            _mockLoanService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetAllDueEmisAsync_OverdueStatus_ReturnsOkWithLoanGroups()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var status = EmiDueStatus.Overdue;

            var groups = new List<LoanEmiGroupResponseDto>
            {
                new LoanEmiGroupResponseDto
                {
                    LoanNumber = "LN-33333333",
                    Emis = new List<DueEmisResponseDto>
                    {
                        new DueEmisResponseDto
                        {
                            DueDate = DateTime.UtcNow.AddMonths(-1),
                            EmiAmount = 5200,
                            InterestComponent = 2000,
                            PrincipalComponent = 3000,
                            RemainingEmiAmount = 5000,
                            PenaltyAmount = 200
                        }
                    }
                }
            };

            _mockPaymentService
                .Setup(s => s.GetAllDueEmisAsync(customerId, status))
                .ReturnsAsync(groups);

            // Act
            var result = await _controller.GetAllDueEmisAsync(status);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var data = ok.Value as IEnumerable<LoanEmiGroupResponseDto>;
            Assert.IsNotNull(data);

            var list = data.ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("LN-33333333", list[0].LoanNumber);

            var emi = list[0].Emis.Single();
            Assert.IsTrue(emi.PenaltyAmount > 0);

            _mockPaymentService.Verify(s => s.GetAllDueEmisAsync(customerId, status), Times.Once);
            _mockPaymentService.VerifyNoOtherCalls();
            _mockLoanService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetDueEmisAsync_Overdue_ReturnsOkWithEmis()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var loanNumber = "LN-ABCDEFGH";
            var status = EmiDueStatus.Overdue;

            var emis = new List<DueEmisResponseDto>
            {
                new DueEmisResponseDto
                {
                    DueDate = DateTime.UtcNow.AddMonths(-1),
                    EmiAmount = 5200,
                    PenaltyAmount = 200
                }
            };

            _mockPaymentService
                .Setup(s => s.GetDueEmisAsync(customerId, loanNumber, status))
                .ReturnsAsync(emis);

            // Act
            var result = await _controller.GetDueEmisAsync(loanNumber, status);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var data = ok.Value as IEnumerable<DueEmisResponseDto>;
            Assert.IsNotNull(data);

            var list = data.ToList();
            Assert.AreEqual(1, list.Count);
            Assert.IsTrue(list[0].PenaltyAmount > 0);

            _mockPaymentService.Verify(
                s => s.GetDueEmisAsync(customerId, loanNumber, status),
                Times.Once);

            _mockPaymentService.VerifyNoOtherCalls();
            _mockLoanService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetDueEmisAsync_Upcoming_ReturnsOkWithEmis()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var loanNumber = "LN-ABCDEFGH";
            var status = EmiDueStatus.Upcoming;

            var emis = new List<DueEmisResponseDto>
            {
                new DueEmisResponseDto
                {
                    DueDate = DateTime.UtcNow.AddDays(10),
                    EmiAmount = 5000,
                    PenaltyAmount = 0
                }
            };

            _mockPaymentService
                .Setup(s => s.GetDueEmisAsync(customerId, loanNumber, status))
                .ReturnsAsync(emis);

            // Act
            var result = await _controller.GetDueEmisAsync(loanNumber, status);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var data = ok.Value as IEnumerable<DueEmisResponseDto>;
            Assert.IsNotNull(data);

            var list = data.ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(0, list[0].PenaltyAmount);

            _mockPaymentService.Verify(
                s => s.GetDueEmisAsync(customerId, loanNumber, status),
                Times.Once);

            _mockPaymentService.VerifyNoOtherCalls();
            _mockLoanService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetPaymentsHistory_ValidLoanNumber_ReturnsOkWithHistory()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var loanNumber = "LN-ABCDEFGH";

            var history = new List<LoanPaymentHistoryResponseDto>
            {
                new LoanPaymentHistoryResponseDto
                {
                    Amount = 10000,
                    PaymentDate = DateTime.UtcNow,
                    Status = LoanPaymentStatus.Paid
                }
            };

            _mockPaymentService
                .Setup(s => s.GetPaymentsHistoryAsync(customerId, loanNumber))
                .ReturnsAsync(history);

            // Act
            var result = await _controller.GetPaymentsHistory(loanNumber);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var data = ok.Value as IEnumerable<LoanPaymentHistoryResponseDto>;
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data.Count());

            _mockPaymentService.Verify(
                s => s.GetPaymentsHistoryAsync(customerId, loanNumber),
                Times.Once);

            _mockPaymentService.VerifyNoOtherCalls();
            _mockLoanService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetCustomerLoans_WhenUserIdMissing_ThrowsAuthenticationFailedException()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var identity = new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.Role, Role.Customer.ToString())
    }, "TestAuth");

            _controller.HttpContext.User = new ClaimsPrincipal(identity);

            await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _controller.GetCustomerLoans(LoanStatus.Active));

            _mockLoanService.VerifyNoOtherCalls();
            _mockPaymentService.VerifyNoOtherCalls();
        }

    }
}
