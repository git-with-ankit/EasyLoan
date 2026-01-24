using EasyLoan.Api.Controllers;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Customer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.UnitTest.Controllers
{
    public class CustomersControllerTests
    {
        private Mock<ICustomerService> _mockCustomerService;
        private Mock<IAuthService> _mockAuthService;
        private CustomersController _controller;

        public CustomersControllerTests()
        {
            _mockCustomerService = new Mock<ICustomerService>();
            _mockAuthService = new Mock<IAuthService>();
            _controller = new CustomersController(_mockCustomerService.Object, _mockAuthService.Object)
        }
        private static void ControllerTestHelper.SetUser(ControllerBase controller, Guid customerId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customerId.ToString()),
                new Claim(ClaimTypes.Role, "Customer")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
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

            ControllerTestHelper.SetUser(_controller, customerId);

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

            ControllerTestHelper.SetUser(_controller, customerId);

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
        }
        [TestMethod]
        public async Task Login_WhenValidCustomerCredentials_ReturnsOkWithToken()
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

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var token = okResult.Value as string;
            Assert.IsNotNull(token);
            Assert.AreEqual(expectedToken, token);
        }

    }
}
