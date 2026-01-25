using EasyLoan.Api.Controllers;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Employee;
using EasyLoan.Models.Common.Enums;
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
    [TestClass]
    public class EmployeesControllerTests
    {
        private readonly Mock<IEmployeeService> _mockEmployeeService;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly EmployeesController _controller;

        public EmployeesControllerTests()
        {
            _mockEmployeeService = new Mock<IEmployeeService>();
            _mockAuthService = new Mock<IAuthService>();
            _controller = new EmployeesController(_mockEmployeeService.Object, _mockAuthService.Object);
        }
        private static void SetEmployeeUser(ControllerBase controller, Guid employeeId, EmployeeRole role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employeeId.ToString()),
                new Claim(ClaimTypes.Role, role.ToString())
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
        public async Task GetProfile_WhenAuthorized_ReturnsOkWithProfile()
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

            SetEmployeeUser(_controller, employeeId, EmployeeRole.Manager);

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
        }
        [TestMethod]
        public async Task UpdateProfile_WhenValidRequest_ReturnsOkWithUpdatedProfile()
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
                Role = EmployeeRole.Manager
            };

            SetEmployeeUser(_controller, employeeId, EmployeeRole.Manager);

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
        }
        [TestMethod]
        public async Task Login_WhenValidRequest_ReturnsOkWithToken()
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
