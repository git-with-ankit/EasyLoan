using EasyLoan.Api.Controllers;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Customer;
using EasyLoan.Models.Common.Enums;
using EasyLoan.UnitTest.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EasyLoan.UnitTest.Controllers
{
    [TestClass]
    public class CustomersControllerTests
    {
        private Mock<ICustomerService> _mockCustomerService = null!;
        private Mock<IAuthService> _mockAuthService = null!;
        private CustomersController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockCustomerService = new Mock<ICustomerService>(MockBehavior.Strict);
            _mockAuthService = new Mock<IAuthService>(MockBehavior.Strict);

            _controller = new CustomersController(
                _mockCustomerService.Object,
                _mockAuthService.Object);

            // Controller MUST have HttpContext for cookies and RequestServices
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [TestMethod]
        public async Task GetProfile_WhenAuthorizedCustomer_ReturnsOkWithProfile()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var profile = new CustomerProfileResponseDto
            {
                Name = "Ravi Kumar",
                Email = "ravi@test.com",
                DateOfBirth = new DateTime(1995, 5, 20),
                PhoneNumber = "9876543210",
                AnnualSalary = 1200000,
                PanNumber = "ABCDE1234F",
                CreditScore = 750
            };

            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            _mockCustomerService
                .Setup(s => s.GetProfileAsync(customerId))
                .ReturnsAsync(profile);

            // Act
            var result = await _controller.GetProfile();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as CustomerProfileResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(profile.Name, response.Name);
            Assert.AreEqual(profile.Email, response.Email);
            Assert.AreEqual(profile.DateOfBirth, response.DateOfBirth);
            Assert.AreEqual(profile.PhoneNumber, response.PhoneNumber);
            Assert.AreEqual(profile.AnnualSalary, response.AnnualSalary);
            Assert.AreEqual(profile.PanNumber, response.PanNumber);
            Assert.AreEqual(profile.CreditScore, response.CreditScore);

            _mockCustomerService.Verify(s => s.GetProfileAsync(customerId), Times.Once);
            _mockCustomerService.VerifyNoOtherCalls();
            _mockAuthService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task UpdateProfile_WhenAuthorizedCustomer_ReturnsOkWithUpdatedProfile()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var request = new UpdateCustomerProfileRequestDto
            {
                Name = "Rahul Sharma",
                PhoneNumber = "9876543210",
                AnnualSalary = 1500000
            };

            var updatedProfile = new CustomerProfileResponseDto
            {
                Name = "Rahul Sharma",
                Email = "rahul@test.com",
                DateOfBirth = new DateTime(1994, 3, 15),
                PhoneNumber = "9876543210",
                AnnualSalary = 1500000,
                PanNumber = "ABCDE1234F",
                CreditScore = 780
            };

            ControllerTestHelper.SetUser(_controller, customerId, Role.Customer);

            _mockCustomerService
                .Setup(s => s.UpdateProfileAsync(customerId, request))
                .ReturnsAsync(updatedProfile);

            // Act
            var result = await _controller.UpdateProfile(request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as CustomerProfileResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(updatedProfile.Name, response.Name);
            Assert.AreEqual(updatedProfile.Email, response.Email);
            Assert.AreEqual(updatedProfile.DateOfBirth, response.DateOfBirth);
            Assert.AreEqual(updatedProfile.PhoneNumber, response.PhoneNumber);
            Assert.AreEqual(updatedProfile.AnnualSalary, response.AnnualSalary);
            Assert.AreEqual(updatedProfile.PanNumber, response.PanNumber);
            Assert.AreEqual(updatedProfile.CreditScore, response.CreditScore);

            _mockCustomerService.Verify(
                s => s.UpdateProfileAsync(customerId, request),
                Times.Once);

            _mockCustomerService.VerifyNoOtherCalls();
            _mockAuthService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task Register_WhenValidRequest_ReturnsCreatedWithCustomerProfile()
        {
            // Arrange
            var request = new RegisterCustomerRequestDto
            {
                Name = "Amit Verma",
                Email = "amit@test.com",
                PhoneNumber = "9876543210",
                PanNumber = "ABCDE1234F",
                AnnualSalary = 1200000,
                DateOfBirth = new DateTime(1996, 4, 10),
                Password = "Password@123"
            };

            var createdCustomer = new CustomerProfileResponseDto
            {
                Name = "Amit Verma",
                Email = "amit@test.com",
                PhoneNumber = "9876543210",
                PanNumber = "ABCDE1234F",
                AnnualSalary = 1200000,
                DateOfBirth = new DateTime(1996, 4, 10),
                CreditScore = 750
            };

            _mockAuthService
                .Setup(s => s.RegisterCustomerAsync(request))
                .ReturnsAsync(createdCustomer);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(StatusCodes.Status201Created, createdResult.StatusCode);

            Assert.AreEqual(nameof(_controller.GetProfile), createdResult.ActionName);

            var response = createdResult.Value as CustomerProfileResponseDto;
            Assert.IsNotNull(response);

            Assert.AreEqual(createdCustomer.Name, response.Name);
            Assert.AreEqual(createdCustomer.Email, response.Email);
            Assert.AreEqual(createdCustomer.PhoneNumber, response.PhoneNumber);
            Assert.AreEqual(createdCustomer.PanNumber, response.PanNumber);
            Assert.AreEqual(createdCustomer.AnnualSalary, response.AnnualSalary);
            Assert.AreEqual(createdCustomer.DateOfBirth, response.DateOfBirth);
            Assert.AreEqual(createdCustomer.CreditScore, response.CreditScore);

            _mockAuthService.Verify(s => s.RegisterCustomerAsync(request), Times.Once);
            _mockAuthService.VerifyNoOtherCalls();
            _mockCustomerService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task Login_WhenValidCredentials_ReturnsNoContent_AndSetsCookie()
        {
            // Arrange
            var request = new CustomerLoginRequestDto
            {
                Email = "customer@test.com",
                Password = "Password@123"
            };

            var expectedToken = "customer-jwt-token";

            _mockAuthService
                .Setup(s => s.LoginCustomerAsync(request))
                .ReturnsAsync(expectedToken);

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

            var setCookieHeader = _controller.Response.Headers["Set-Cookie"].ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(setCookieHeader));
            Assert.IsTrue(setCookieHeader.Contains("easyloan_auth="));

            _mockAuthService.Verify(s => s.LoginCustomerAsync(request), Times.Once);
            _mockAuthService.VerifyNoOtherCalls();
            _mockCustomerService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task Login_WhenExpiryMinutesMissingInConfig_ThrowsException()
        {
            // Arrange
            var request = new CustomerLoginRequestDto
            {
                Email = "customer@test.com",
                Password = "Password@123"
            };

            _mockAuthService
                .Setup(s => s.LoginCustomerAsync(request))
                .ReturnsAsync("token");

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

            _controller.HttpContext.RequestServices = services.BuildServiceProvider();

            // Act + Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                _controller.Login(request));

            _mockAuthService.Verify(s => s.LoginCustomerAsync(request), Times.Once);
            _mockAuthService.VerifyNoOtherCalls();
            _mockCustomerService.VerifyNoOtherCalls();
        }


        [TestMethod]
        public async Task GetProfile_WhenUserIdClaimMissing_ThrowsException()
        {
            // Arrange

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var identity = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, Role.Customer.ToString())
            }, "TestAuth");

            _controller.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(identity);

            await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _controller.GetProfile());

            _mockCustomerService.VerifyNoOtherCalls();
            _mockAuthService.VerifyNoOtherCalls();
        }
    }
}
