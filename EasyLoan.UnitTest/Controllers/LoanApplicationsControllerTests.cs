using EasyLoan.Api.Controllers;
using EasyLoan.Business.Enums;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.LoanApplication;
using EasyLoan.Models.Common.Enums;
using EasyLoan.UnitTest.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EasyLoan.UnitTest.Controllers
{
    [TestClass]
    public class LoanApplicationsControllerTests
    {
        private readonly Mock<ILoanApplicationService> _mockLoanApplicationService;
        private readonly LoanApplicationsController _controller;

        public LoanApplicationsControllerTests()
        {
            _mockLoanApplicationService = new Mock<ILoanApplicationService>();
            _controller = new LoanApplicationsController(_mockLoanApplicationService.Object);
        }
        [TestMethod]
        public async Task Create_WhenValidRequest_ReturnsCreatedWithApplicationNumber()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var request = new CreateLoanApplicationRequestDto
            {
                LoanTypeId = Guid.NewGuid(),
                RequestedAmount = 500000,
                RequestedTenureInMonths = 240
            };

            // ONLY fields we are certain exist
            var createdApplication = new CreatedApplicationResponseDto
            {
                ApplicationNumber = "APP-123456"
            };

            ControllerTestHelper.SetUser(
                _controller,
                customerId,
                Role.Customer);

            _mockLoanApplicationService
                .Setup(s => s.CreateAsync(customerId, request))
                .ReturnsAsync(createdApplication);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(StatusCodes.Status201Created, createdResult.StatusCode);

            Assert.AreEqual(
                nameof(_controller.GetByApplicationNumber),
                createdResult.ActionName);

            Assert.IsNotNull(createdResult.RouteValues);
            Assert.AreEqual(
                createdApplication.ApplicationNumber,
                createdResult.RouteValues["applicationNumber"]);

            var response = createdResult.Value as CreatedApplicationResponseDto;
            Assert.IsNotNull(response);
            Assert.AreEqual(
                createdApplication.ApplicationNumber,
                response.ApplicationNumber);
        }
        [TestMethod]
        public async Task GetApplications_WhenValidRequest_ReturnsOkWithApplications()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var role = Role.Customer; // your domain enum
            var status = LoanApplicationStatus.Pending;

            var applications = new List<LoanApplicationsResponseDto>
    {
        new LoanApplicationsResponseDto
        {
            ApplicationNumber = "APP-001",
            LoanTypeName = "Home Loan",
            RequestedAmount = 500000,
            TenureInMonths = 240,
            AssignedEmployeeId = Guid.NewGuid(),
            Status = LoanApplicationStatus.Pending,
            CreatedDate = DateTime.UtcNow
        }
    };

            ControllerTestHelper.SetUser(
                _controller,
                userId,
                role);

            _mockLoanApplicationService
                .Setup(s => s.GetApplicationsAsync(userId, role, status))
                .ReturnsAsync(applications);

            // Act
            var result = await _controller.GetApplications(status);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as IEnumerable<LoanApplicationsResponseDto>;
            Assert.IsNotNull(response);
            Assert.AreEqual(1, response.Count());

            Assert.AreEqual(
                applications[0].ApplicationNumber,
                response.First().ApplicationNumber);
        }
        [TestMethod]
        public async Task GetByApplicationNumber_WhenValidRequest_ReturnsOkWithDetails()
        {
            // Arrange
            var applicationNumber = "APP-123456";

            var details = new LoanApplicationDetailsResponseDto
            {
                ApplicationNumber = applicationNumber,
                CustomerName = "Rohit Sharma",
                LoanType = "Home Loan",
                RequestedAmount = 500000,
                //AppprovedAmount = 450000,
                InterestRate = 8.5m,
                AssignedEmployeeId = Guid.NewGuid(),
                RequestedTenureInMonths = 240,
                Status = LoanApplicationStatus.Pending,
                ManagerComments = null
            };

            _mockLoanApplicationService
                .Setup(s => s.GetByApplicationNumberAsync(applicationNumber))
                .ReturnsAsync(details);

            // Act
            var result = await _controller.GetByApplicationNumber(applicationNumber);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as LoanApplicationDetailsResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(details.ApplicationNumber, response.ApplicationNumber);
            Assert.AreEqual(details.CustomerName, response.CustomerName);
            Assert.AreEqual(details.LoanType, response.LoanType);
            Assert.AreEqual(details.RequestedAmount, response.RequestedAmount);
            //Assert.AreEqual(details.AppprovedAmount, response.AppprovedAmount);
            Assert.AreEqual(details.InterestRate, response.InterestRate);
            Assert.AreEqual(details.AssignedEmployeeId, response.AssignedEmployeeId);
            Assert.AreEqual(details.RequestedTenureInMonths, response.RequestedTenureInMonths);
            Assert.AreEqual(details.Status, response.Status);
            Assert.AreEqual(details.ManagerComments, response.ManagerComments);
        }
        [TestMethod]
        public async Task UpdateReview_WhenValidRequest_ReturnsOkWithReviewedApplication()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var applicationNumber = "APP-789456";

            var request = new ReviewLoanApplicationRequestDto
            {
                IsApproved = true,
                ApprovedAmount = 450000,
                ManagerComments = "Looks good"
            };

            var reviewedResponse = new LoanApplicationReviewResponseDto
            {
                ApplicationNumber = applicationNumber,
                ApprovedAmount = 450000,
                ManagerComments = "Looks good",
                Status = LoanApplicationStatus.Approved
            };

            ControllerTestHelper.SetUser(
                _controller,
                managerId,
                Role.Manager);

            _mockLoanApplicationService
                .Setup(s =>
                    s.UpdateReviewAsync(applicationNumber, managerId, request))
                .ReturnsAsync(reviewedResponse);

            // Act
            var result = await _controller.UpdateReview(applicationNumber, request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as LoanApplicationReviewResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(reviewedResponse.ApplicationNumber, response.ApplicationNumber);
            Assert.AreEqual(reviewedResponse.ApprovedAmount, response.ApprovedAmount);
            Assert.AreEqual(reviewedResponse.ManagerComments, response.ManagerComments);
            Assert.AreEqual(reviewedResponse.Status, response.Status);
        }
        [TestMethod]
        public async Task GetApplicationDetailsForReview_WhenValidRequest_ReturnsOkWithDetails()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var applicationNumber = "APP-456789";

            var details = new LoanApplicationDetailsWithCustomerDataResponseDto
            {
                ApplicationNumber = applicationNumber,
                CustomerName = "Suresh Kumar",
                AnnualSalaryOfCustomer = 1200000,
                PhoneNumber = "9876543210",
                CreditScore = 760,
                DateOfBirth = new DateTime(1992, 6, 12),
                PanNumber = "ABCDE1234F",
                LoanType = "Home Loan",
                RequestedAmount = 600000,
                //AppprovedAmount = 550000,
                InterestRate = 8.4m,
                RequestedTenureInMonths = 240,
                Status = LoanApplicationStatus.Pending,
                ManagerComments = null,
                TotalOngoingLoans = 1
            };

            ControllerTestHelper.SetUser(
                _controller,
                managerId,
                Role.Manager);

            _mockLoanApplicationService
                .Setup(s => s.GetApplicationDetailsForReview(applicationNumber, managerId))
                .ReturnsAsync(details);

            // Act
            var result = await _controller.GetApplicationDetailsForReview(applicationNumber);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var response =
                okResult.Value as LoanApplicationDetailsWithCustomerDataResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(details.ApplicationNumber, response.ApplicationNumber);
            Assert.AreEqual(details.CustomerName, response.CustomerName);
            Assert.AreEqual(details.AnnualSalaryOfCustomer, response.AnnualSalaryOfCustomer);
            Assert.AreEqual(details.PhoneNumber, response.PhoneNumber);
            Assert.AreEqual(details.CreditScore, response.CreditScore);
            Assert.AreEqual(details.DateOfBirth, response.DateOfBirth);
            Assert.AreEqual(details.PanNumber, response.PanNumber);
            Assert.AreEqual(details.LoanType, response.LoanType);
            Assert.AreEqual(details.RequestedAmount, response.RequestedAmount);
            //Assert.AreEqual(details.AppprovedAmount, response.AppprovedAmount);
            Assert.AreEqual(details.InterestRate, response.InterestRate);
            Assert.AreEqual(details.RequestedTenureInMonths, response.RequestedTenureInMonths);
            Assert.AreEqual(details.Status, response.Status);
            Assert.AreEqual(details.ManagerComments, response.ManagerComments);
            Assert.AreEqual(details.TotalOngoingLoans, response.TotalOngoingLoans);
        }


    }
}
