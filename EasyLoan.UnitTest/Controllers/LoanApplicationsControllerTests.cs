using EasyLoan.Api.Controllers;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Common;
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
        private Mock<ILoanApplicationService> _mockLoanApplicationService = null!;
        private LoanApplicationsController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLoanApplicationService = new Mock<ILoanApplicationService>(MockBehavior.Strict);
            _controller = new LoanApplicationsController(_mockLoanApplicationService.Object);
        }

        [TestMethod]
        public async Task Create_WhenValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var request = new CreateLoanApplicationRequestDto
            {
                LoanTypeId = Guid.NewGuid(),
                RequestedAmount = 500000,
                RequestedTenureInMonths = 240
            };

            var createdApplication = new CreatedApplicationResponseDto
            {
                ApplicationNumber = "LA-ABCDEFGH",
                Status = LoanApplicationStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };

            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            _mockLoanApplicationService
                .Setup(s => s.CreateAsync(customerId, request))
                .ReturnsAsync(createdApplication);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var created = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(created);

            Assert.AreEqual(StatusCodes.Status201Created, created.StatusCode);
            Assert.AreEqual(nameof(_controller.GetByApplicationNumber), created.ActionName);

            Assert.IsNotNull(created.RouteValues);
            Assert.AreEqual("LA-ABCDEFGH", created.RouteValues["applicationNumber"]);

            var response = created.Value as CreatedApplicationResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(createdApplication.ApplicationNumber, response.ApplicationNumber);
            Assert.AreEqual(createdApplication.Status, response.Status);

            _mockLoanApplicationService.Verify(s => s.CreateAsync(customerId, request), Times.Once);
            _mockLoanApplicationService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetApplications_WhenPageNumberLessThan1_ReturnsBadRequest()
        {
            // Arrange
            ControllerTestHelper.SetUser(_controller, Guid.NewGuid(), Role.Customer);

            // Act
            var result = await _controller.GetApplications(
                status: LoanApplicationStatus.Pending,
                pageNumber: 0,
                pageSize: 10);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
            Assert.AreEqual("Page number must be greater than 0", badRequest.Value);

            _mockLoanApplicationService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetApplications_WhenPageSizeLessThan1_ReturnsBadRequest()
        {
            // Arrange
            ControllerTestHelper.SetUser(_controller, Guid.NewGuid(), Role.Customer);

            // Act
            var result = await _controller.GetApplications(
                status: LoanApplicationStatus.Pending,
                pageNumber: 1,
                pageSize: 0);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
            Assert.AreEqual("Page size must be between 1 and 100", badRequest.Value);

            _mockLoanApplicationService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetApplications_WhenPageSizeGreaterThan100_ReturnsBadRequest()
        {
            // Arrange
            ControllerTestHelper.SetUser(_controller, Guid.NewGuid(), Role.Customer);

            // Act
            var result = await _controller.GetApplications(
                status: LoanApplicationStatus.Pending,
                pageNumber: 1,
                pageSize: 101);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
            Assert.AreEqual("Page size must be between 1 and 100", badRequest.Value);

            _mockLoanApplicationService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetApplications_WhenValidRequest_ReturnsOkWithPagedResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var role = Role.Customer;
            var status = LoanApplicationStatus.Pending;

            var pagedResponse = new PagedResponseDto<LoanApplicationsResponseDto>
            {
                Items = new List<LoanApplicationsResponseDto>
                {
                    new LoanApplicationsResponseDto
                    {
                        ApplicationNumber = "LA-ABCDEFGH",
                        LoanTypeName = "Home Loan",
                        RequestedAmount = 500000,
                        TenureInMonths = 240,
                        AssignedEmployeeId = Guid.NewGuid(),
                        Status = LoanApplicationStatus.Pending,
                        CreatedDate = DateTime.UtcNow
                    }
                },
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1,
                TotalPages = 1
            };

            ControllerTestHelper.SetUser(_controller, userId, role);

            _mockLoanApplicationService
                .Setup(s => s.GetApplicationsAsync(userId, role, status, 1, 10))
                .ReturnsAsync(pagedResponse);

            // Act
            var result = await _controller.GetApplications(status, pageNumber: 1, pageSize: 10);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var response = ok.Value as PagedResponseDto<LoanApplicationsResponseDto>;
            Assert.IsNotNull(response);

            Assert.AreEqual(1, response.Items.Count());
            Assert.AreEqual("LA-ABCDEFGH", response.Items.First().ApplicationNumber);

            _mockLoanApplicationService.Verify(
                s => s.GetApplicationsAsync(userId, role, status, 1, 10),
                Times.Once);

            _mockLoanApplicationService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetByApplicationNumber_WhenValidRequest_ReturnsOkWithDetails()
        {
            // Arrange
            var applicationNumber = "LA-ABCDEFGH";

            var details = new LoanApplicationDetailsResponseDto
            {
                ApplicationNumber = applicationNumber,
                CustomerName = "Rohit Sharma",
                LoanType = "Home Loan",
                RequestedAmount = 500000,
                InterestRate = 8.5m,
                ApprovedAmount = 0,
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
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var response = ok.Value as LoanApplicationDetailsResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(details.ApplicationNumber, response.ApplicationNumber);
            Assert.AreEqual(details.CustomerName, response.CustomerName);
            Assert.AreEqual(details.LoanType, response.LoanType);
            Assert.AreEqual(details.RequestedAmount, response.RequestedAmount);
            Assert.AreEqual(details.ApprovedAmount, response.ApprovedAmount);
            Assert.AreEqual(details.InterestRate, response.InterestRate);
            Assert.AreEqual(details.AssignedEmployeeId, response.AssignedEmployeeId);
            Assert.AreEqual(details.RequestedTenureInMonths, response.RequestedTenureInMonths);
            Assert.AreEqual(details.Status, response.Status);
            Assert.AreEqual(details.ManagerComments, response.ManagerComments);

            _mockLoanApplicationService.Verify(s => s.GetByApplicationNumberAsync(applicationNumber), Times.Once);
            _mockLoanApplicationService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task UpdateReview_WhenValidRequest_ReturnsOkWithReviewedApplication()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var applicationNumber = "LA-ABCDEFGH";

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

            ControllerTestHelper.SetUser(_controller, managerId, Role.Manager);

            _mockLoanApplicationService
                .Setup(s => s.UpdateReviewAsync(applicationNumber, managerId, request))
                .ReturnsAsync(reviewedResponse);

            // Act
            var result = await _controller.UpdateReview(applicationNumber, request);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var response = ok.Value as LoanApplicationReviewResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(reviewedResponse.ApplicationNumber, response.ApplicationNumber);
            Assert.AreEqual(reviewedResponse.ApprovedAmount, response.ApprovedAmount);
            Assert.AreEqual(reviewedResponse.ManagerComments, response.ManagerComments);
            Assert.AreEqual(reviewedResponse.Status, response.Status);

            _mockLoanApplicationService.Verify(
                s => s.UpdateReviewAsync(applicationNumber, managerId, request),
                Times.Once);

            _mockLoanApplicationService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetApplicationDetailsForReview_WhenValidRequestAsManager_ReturnsOk()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var applicationNumber = "LA-ABCDEFGH";
            var role = Role.Manager;

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
                ApprovedAmount = 0,
                InterestRate = 8.4m,
                RequestedTenureInMonths = 240,
                Status = LoanApplicationStatus.Pending,
                ManagerComments = null,
                TotalOngoingLoans = 1
            };

            ControllerTestHelper.SetUser(_controller, managerId, role);

            _mockLoanApplicationService
                .Setup(s => s.GetApplicationDetailsForReview(applicationNumber, managerId, role))
                .ReturnsAsync(details);

            // Act
            var result = await _controller.GetApplicationDetailsForReview(applicationNumber);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var response = ok.Value as LoanApplicationDetailsWithCustomerDataResponseDto;
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
            Assert.AreEqual(details.ApprovedAmount, response.ApprovedAmount);
            Assert.AreEqual(details.InterestRate, response.InterestRate);
            Assert.AreEqual(details.RequestedTenureInMonths, response.RequestedTenureInMonths);
            Assert.AreEqual(details.Status, response.Status);
            Assert.AreEqual(details.ManagerComments, response.ManagerComments);
            Assert.AreEqual(details.TotalOngoingLoans, response.TotalOngoingLoans);

            _mockLoanApplicationService.Verify(
                s => s.GetApplicationDetailsForReview(applicationNumber, managerId, role),
                Times.Once);

            _mockLoanApplicationService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetApplicationDetailsForReview_WhenValidRequestAsAdmin_ReturnsOk()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var applicationNumber = "LA-ABCDEFGH";
            var role = Role.Admin;

            var details = new LoanApplicationDetailsWithCustomerDataResponseDto
            {
                ApplicationNumber = applicationNumber,
                CustomerName = "Admin View Customer",
                AnnualSalaryOfCustomer = 900000,
                PhoneNumber = "9000000000",
                CreditScore = 720,
                DateOfBirth = new DateTime(1995, 1, 1),
                PanNumber = "ABCDE1234F",
                LoanType = "Car Loan",
                RequestedAmount = 300000,
                ApprovedAmount = 0,
                InterestRate = 9.1m,
                RequestedTenureInMonths = 60,
                Status = LoanApplicationStatus.Pending,
                ManagerComments = null,
                TotalOngoingLoans = 0
            };

            ControllerTestHelper.SetUser(_controller, adminId, role);

            _mockLoanApplicationService
                .Setup(s => s.GetApplicationDetailsForReview(applicationNumber, adminId, role))
                .ReturnsAsync(details);

            // Act
            var result = await _controller.GetApplicationDetailsForReview(applicationNumber);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var response = ok.Value as LoanApplicationDetailsWithCustomerDataResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(details.ApplicationNumber, response.ApplicationNumber);
            Assert.AreEqual(details.CustomerName, response.CustomerName);

            _mockLoanApplicationService.Verify(
                s => s.GetApplicationDetailsForReview(applicationNumber, adminId, role),
                Times.Once);

            _mockLoanApplicationService.VerifyNoOtherCalls();
        }
    }
}
