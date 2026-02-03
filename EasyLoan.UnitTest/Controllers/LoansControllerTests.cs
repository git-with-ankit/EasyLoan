using EasyLoan.Api.Controllers;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Loan;
using EasyLoan.Dtos.LoanPayment;
using EasyLoan.Models.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;
using EasyLoan.UnitTest.Helpers;
using EasyLoan.Business.Enums;

namespace EasyLoan.UnitTest.Controllers
{
    public class LoansControllerTests
    {
        private readonly Mock<ILoanService> _mockLoanService;
        private readonly Mock<ILoanPaymentService> _mockPaymentService;
        private readonly LoansController _controller;

        public LoansControllerTests()
        {
            _mockLoanService = new Mock<ILoanService>();
            _mockPaymentService = new Mock<ILoanPaymentService>();
            _controller = new LoansController(_mockLoanService.Object, _mockPaymentService.Object);
        }
    //    private void ControllerTestHelper.SetUser(Guid customerId)
    //    {
    //        var claims = new List<Claim>
    //{
    //    new Claim(ClaimTypes.NameIdentifier, customerId.ToString()),
    //    new Claim(ClaimTypes.Role, "Customer")
    //};

    //        _controller.ControllerContext = new ControllerContext
    //        {
    //            HttpContext = new DefaultHttpContext
    //            {
    //                User = new ClaimsPrincipal(
    //                    new ClaimsIdentity(claims, "TestAuth")
    //                )
    //            }
    //        };
    //    }

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
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var data = okResult.Value as IEnumerable<LoanSummaryResponseDto>;
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data.Count());
        }

        [TestMethod]
        public async Task GetDetails_ValidLoanNumberForCustomer_ReturnsOkWithLoanDetails()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var loanNumber = "LN-001";

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
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var data = okResult.Value as LoanDetailsResponseDto;
            Assert.IsNotNull(data);
            Assert.AreEqual(loanNumber, data.LoanNumber);
        }

        [TestMethod]
        public async Task MakePayment_ValidRequestForCustomer_ReturnsOkWithPaymentResponse()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var loanNumber = "LN-001";
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
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var data = okResult.Value as LoanPaymentResponseDto;
            Assert.IsNotNull(data);
            Assert.AreEqual(10000, data.Amount);
        }

        [TestMethod]
        public async Task GetAllDueEmisAsync_OverdueStatusForCustomer_ReturnsOkWithGroupedEmis()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller,customerId,Role.Customer);

            var status = EmiDueStatus.Overdue;

            var emis = new List<List<DueEmisResponseDto>>
            {
                new List<DueEmisResponseDto>
                {
                    new DueEmisResponseDto
                    {
                        DueDate = DateTime.UtcNow.AddMonths(1),
                        EmiAmount = 5000,
                        InterestComponent = 2000,
                        PrincipalComponent = 3000,
                        PrincipalRemainingAfterPayment = 470000,
                        RemainingEmiAmount = 495000
                    },
                    new DueEmisResponseDto
                    {
                        DueDate = DateTime.UtcNow.AddMonths(2),
                        EmiAmount = 5000,
                        InterestComponent = 1950,
                        PrincipalComponent = 3050,
                        PrincipalRemainingAfterPayment = 466950,
                        RemainingEmiAmount = 490000
                    }
                },
                new List<DueEmisResponseDto>
                {
                    new DueEmisResponseDto
                    {
                        DueDate = DateTime.UtcNow.AddMonths(1),
                        EmiAmount = 8000,
                        InterestComponent = 3000,
                        PrincipalComponent = 5000,
                        PrincipalRemainingAfterPayment = 780000,
                        RemainingEmiAmount = 792000
                    }
                }
            };

            //_mockPaymentService
            //    .Setup(s => s.GetAllDueEmisAsync(customerId, status))
            //    .ReturnsAsync(emis);

            // Act
            var result = await _controller.GetAllDueEmisAsync(status);

            // Assert
            Assert.IsNotNull(result);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var data = okResult.Value as IEnumerable<IEnumerable<DueEmisResponseDto>>;
            Assert.IsNotNull(data);

            Assert.AreEqual(2, data.Count());              // two loan groups
            Assert.AreEqual(2, data.First().Count());      // first loan has two EMIs
            Assert.AreEqual(5000, data.First().First().EmiAmount);
        }
        [TestMethod]
        public async Task GetAllDueEmisAsync_UpcomingStatusForCustomer_ReturnsOkWithGroupedEmis()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var status = EmiDueStatus.Upcoming;

            var emis = new List<List<DueEmisResponseDto>>
    {
        new List<DueEmisResponseDto>
        {
            new DueEmisResponseDto
            {
                DueDate = DateTime.UtcNow.AddMonths(1),
                EmiAmount = 5000,
                InterestComponent = 2000,
                PrincipalComponent = 3000,
                PrincipalRemainingAfterPayment = 470000,
                RemainingEmiAmount = 495000
            },
            new DueEmisResponseDto
            {
                DueDate = DateTime.UtcNow.AddMonths(2),
                EmiAmount = 5000,
                InterestComponent = 1950,
                PrincipalComponent = 3050,
                PrincipalRemainingAfterPayment = 466950,
                RemainingEmiAmount = 490000
            }
        },
        new List<DueEmisResponseDto>
        {
            new DueEmisResponseDto
            {
                DueDate = DateTime.UtcNow.AddMonths(1),
                EmiAmount = 8000,
                InterestComponent = 3000,
                PrincipalComponent = 5000,
                PrincipalRemainingAfterPayment = 780000,
                RemainingEmiAmount = 792000
            }
        }
    };

            //_mockPaymentService
            //    .Setup(s => s.GetAllDueEmisAsync(customerId, status))
            //    .ReturnsAsync(emis);

            // Act
            var result = await _controller.GetAllDueEmisAsync(status);

            // Assert
            Assert.IsNotNull(result);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var data = okResult.Value as IEnumerable<IEnumerable<DueEmisResponseDto>>;
            Assert.IsNotNull(data);

            Assert.AreEqual(2, data.Count());              // two loan groups
            Assert.AreEqual(2, data.First().Count());      // first loan has two EMIs
            Assert.AreEqual(5000, data.First().First().EmiAmount);
        }

        [TestMethod]
        public async Task GetDueEmisAsync_OverdueStatusForLoan_ReturnsOkWithEmis()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);
            var loanNumber = "LN-001";
            var status = EmiDueStatus.Overdue;

            var emis = new List<DueEmisResponseDto>
            {
                new DueEmisResponseDto
                {
                    DueDate = DateTime.UtcNow.AddMonths(-1),
                    EmiAmount = 5200
                }
            };

            _mockPaymentService
                .Setup(s => s.GetDueEmisAsync(customerId, loanNumber, status))
                .ReturnsAsync(emis);

            // Act
            var result = await _controller.GetDueEmisAsync(loanNumber, status);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var data = okResult.Value as IEnumerable<DueEmisResponseDto>;
            Assert.IsNotNull(data);
        }
        [TestMethod]
        public async Task GetDueEmisAsync_UpcomingStatusForLoan_ReturnsOkWithEmis()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var loanNumber = "LN-001";
            var status = EmiDueStatus.Upcoming;

            var emis = new List<DueEmisResponseDto>
            {
                new DueEmisResponseDto
                {
                    DueDate = DateTime.UtcNow.AddMonths(-1),
                    EmiAmount = 5200
                }
            };

            _mockPaymentService
                .Setup(s => s.GetDueEmisAsync(customerId, loanNumber, status))
                .ReturnsAsync(emis);

            // Act
            var result = await _controller.GetDueEmisAsync(loanNumber, status);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var data = okResult.Value as IEnumerable<DueEmisResponseDto>;
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public async Task GetPaymentsHistory_ValidLoanNumberForCustomer_ReturnsOkWithPaymentHistory()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            var loanNumber = "LN-001";

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
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var data = okResult.Value as IEnumerable<LoanPaymentHistoryResponseDto>;
            Assert.IsNotNull(data);
        }
    }
}
