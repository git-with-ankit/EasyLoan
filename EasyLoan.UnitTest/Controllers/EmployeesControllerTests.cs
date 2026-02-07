using EasyLoan.Api.Controllers;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Employee;
using EasyLoan.Models.Common.Enums;
using EasyLoan.UnitTest.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace EasyLoan.UnitTest.Controllers
{
    [TestClass]
    public class EmployeesControllerTests
    {
        private Mock<IEmployeeService> _mockEmployeeService = null!;
        private Mock<IAuthService> _mockAuthService = null!;
        private EmployeesController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockEmployeeService = new Mock<IEmployeeService>(MockBehavior.Strict);
            _mockAuthService = new Mock<IAuthService>(MockBehavior.Strict);

            _controller = new EmployeesController(
                _mockEmployeeService.Object,
                _mockAuthService.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [TestMethod]
        public async Task GetProfile_WhenAuthorizedManager_ReturnsOkWithProfile()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            var profile = new EmployeeProfileResponseDto
            {
                Name = "John",
                Email = "john@test.com",
                PhoneNumber = "9999999999",
                Role = EmployeeRole.Manager
            };

            ControllerTestHelper.SetUser(_controller, employeeId, Role.Manager);

            _mockEmployeeService
                .Setup(s => s.GetProfileAsync(employeeId))
                .ReturnsAsync(profile);

            // Act
            var result = await _controller.GetProfile();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as EmployeeProfileResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(profile.Name, response.Name);
            Assert.AreEqual(profile.Email, response.Email);
            Assert.AreEqual(profile.PhoneNumber, response.PhoneNumber);
            Assert.AreEqual(profile.Role, response.Role);

            _mockEmployeeService.Verify(s => s.GetProfileAsync(employeeId), Times.Once);
            _mockEmployeeService.VerifyNoOtherCalls();
            _mockAuthService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task UpdateProfile_WhenAuthorizedAdmin_ReturnsOkWithUpdatedProfile()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            var request = new UpdateEmployeeProfileRequestDto
            {
                Name = "Jane Doe",
                PhoneNumber = "9876543210"
            };

            var updatedProfile = new EmployeeProfileResponseDto
            {
                Name = "Jane Doe",
                Email = "jane@test.com",
                PhoneNumber = "9876543210",
                Role = EmployeeRole.Admin
            };

            ControllerTestHelper.SetUser(_controller, employeeId, Role.Admin);

            _mockEmployeeService
                .Setup(s => s.UpdateProfileAsync(employeeId, request))
                .ReturnsAsync(updatedProfile);

            // Act
            var result = await _controller.UpdateProfile(request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as EmployeeProfileResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(updatedProfile.Name, response.Name);
            Assert.AreEqual(updatedProfile.Email, response.Email);
            Assert.AreEqual(updatedProfile.PhoneNumber, response.PhoneNumber);
            Assert.AreEqual(updatedProfile.Role, response.Role);

            _mockEmployeeService.Verify(s => s.UpdateProfileAsync(employeeId, request), Times.Once);
            _mockEmployeeService.VerifyNoOtherCalls();
            _mockAuthService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task Login_WhenValidCredentials_ReturnsNoContent_AndSetsCookie()
        {
            // Arrange
            var request = new EmployeeLoginRequestDto
            {
                Email = "manager@test.com",
                Password = "Password@123"
            };

            var expectedToken = "jwt-token";

            _mockAuthService
                .Setup(s => s.LoginEmployeeAsync(request))
                .ReturnsAsync(expectedToken);

            // Controller reads IConfiguration from HttpContext.RequestServices
            var settings = new Dictionary<string, string?>
            {
                { "Jwt:AccessTokenExpiryMinutes", "60" }
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);

            _controller.HttpContext.RequestServices = services.BuildServiceProvider();

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var noContent = result as NoContentResult;
            Assert.IsNotNull(noContent);
            Assert.AreEqual(StatusCodes.Status204NoContent, noContent.StatusCode);

            // Cookie is written to response headers
            var setCookieHeader = _controller.Response.Headers["Set-Cookie"].ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(setCookieHeader));
            Assert.IsTrue(setCookieHeader.Contains("easyloan_auth="));

            _mockAuthService.Verify(s => s.LoginEmployeeAsync(request), Times.Once);
            _mockAuthService.VerifyNoOtherCalls();
            _mockEmployeeService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task Login_WhenExpiryMinutesMissingInConfig_ThrowsException()
        {
            // Arrange
            var request = new EmployeeLoginRequestDto
            {
                Email = "manager@test.com",
                Password = "Password@123"
            };

            _mockAuthService
                .Setup(s => s.LoginEmployeeAsync(request))
                .ReturnsAsync("token");

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

            _controller.HttpContext.RequestServices = services.BuildServiceProvider();

            // Act + Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                _controller.Login(request));

            _mockAuthService.Verify(s => s.LoginEmployeeAsync(request), Times.Once);
            _mockAuthService.VerifyNoOtherCalls();
            _mockEmployeeService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task CreateManager_WhenAdmin_ReturnsCreatedAtActionWithManager()
        {
            // Arrange
            var adminId = Guid.NewGuid();

            var request = new CreateManagerRequestDto
            {
                Name = "New Manager",
                Email = "manager@test.com",
                PhoneNumber = "9999999999",
                Password = "Password@123"
            };

            var response = new RegisterManagerResponseDto
            {
                Name = "New Manager",
                Email = "manager@test.com",
                Role = EmployeeRole.Manager
            };

            ControllerTestHelper.SetUser(_controller, adminId, Role.Admin);

            _mockAuthService
                .Setup(s => s.RegisterManagerAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateManager(request);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(StatusCodes.Status201Created, createdResult.StatusCode);

            Assert.AreEqual(nameof(_controller.GetProfile), createdResult.ActionName);

            var value = createdResult.Value as RegisterManagerResponseDto;
            Assert.IsNotNull(value);

            Assert.AreEqual(response.Name, value.Name);
            Assert.AreEqual(response.Email, value.Email);
            Assert.AreEqual(EmployeeRole.Manager, value.Role);

            _mockAuthService.Verify(s => s.RegisterManagerAsync(request), Times.Once);
            _mockAuthService.VerifyNoOtherCalls();
            _mockEmployeeService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task AdminDashboard_WhenCalled_ReturnsOkWithDashboard()
        {
            // Arrange
            var dashboard = new AdminDashboardResponseDto
            {
                NumberOfLoanTypes = 5,
                NumberOfPendingApplications = 10,
                NumberOfApprovedApplications = 7,
                NumberOfRejectedApplications = 3,
                NumberOfManagers = 2
            };

            _mockEmployeeService
                .Setup(s => s.GetAdminDashboardAsync())
                .ReturnsAsync(dashboard);

            // Act
            var result = await _controller.AdminDashboard();

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var response = ok.Value as AdminDashboardResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(5, response.NumberOfLoanTypes);
            Assert.AreEqual(10, response.NumberOfPendingApplications);
            Assert.AreEqual(7, response.NumberOfApprovedApplications);
            Assert.AreEqual(3, response.NumberOfRejectedApplications);
            Assert.AreEqual(2, response.NumberOfManagers);

            _mockEmployeeService.Verify(s => s.GetAdminDashboardAsync(), Times.Once);
            _mockEmployeeService.VerifyNoOtherCalls();
            _mockAuthService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetProfile_WhenUserIdClaimMissing_ThrowsAuthenticationFailedException()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, Role.Manager.ToString())
            }, "TestAuth");

            _controller.HttpContext.User = new ClaimsPrincipal(identity);

            await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _controller.GetProfile());

            _mockEmployeeService.VerifyNoOtherCalls();
            _mockAuthService.VerifyNoOtherCalls();
        }

    }
}
