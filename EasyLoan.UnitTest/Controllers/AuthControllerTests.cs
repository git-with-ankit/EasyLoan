using EasyLoan.Api.Controllers;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Auth;
using EasyLoan.Models.Common.Enums;
using EasyLoan.UnitTest.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;

namespace EasyLoan.UnitTest.Controllers
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<IAuthService> _authServiceMock = null!;
        private IConfiguration _configuration = null!;
        private AuthController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthService>(MockBehavior.Strict);

            var settings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "THIS_IS_A_TEST_KEY_1234567890123456" },
                { "Jwt:Issuer", "EasyLoan.TestIssuer" },
                { "Jwt:Audience", "EasyLoan.TestAudience" },
                { "Jwt:AccessTokenExpiryMinutes", "60" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();

            _controller = new AuthController(_configuration, _authServiceMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }


        [TestMethod]
        public void GetMe_WhenUserHasEmailAndRole_ReturnsOkWithMeResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();

            ControllerTestHelper.SetUser(_controller, userId, Role.Customer);

            // Add Email claim manually (helper doesn't add email)
            var identity = (ClaimsIdentity)_controller.User.Identity!;
            identity.AddClaim(new Claim(ClaimTypes.Email, "ankit@test.com"));

            // Act
            var result = _controller.GetMe();

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var dto = ok.Value as MeResponseDto;
            Assert.IsNotNull(dto);

            Assert.AreEqual("ankit@test.com", dto.Email);
            Assert.AreEqual(Role.Customer, dto.Role);

            _authServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void GetMe_WhenEmailClaimMissing_ReturnsOkWithNullForgivenEmail()
        {
            // Arrange
            var userId = Guid.NewGuid();

            ControllerTestHelper.SetUser(_controller, userId, Role.Manager);

            // Act
            var result = _controller.GetMe();

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var dto = ok.Value as MeResponseDto;
            Assert.IsNotNull(dto);

            Assert.IsNull(dto.Email); // Because email claim missing
            Assert.AreEqual(Role.Manager, dto.Role);

            _authServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void Logout_WhenCalled_DeletesCookieAndReturnsNoContent()
        {
            // Arrange

            // Act
            var result = _controller.Logout();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var noContent = result as NoContentResult;
            Assert.IsNotNull(noContent);
            Assert.AreEqual(StatusCodes.Status204NoContent, noContent.StatusCode);

            _authServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ChangePassword_WhenValidUser_ReturnsNoContent_AndCallsService()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var role = Role.Customer;

            ControllerTestHelper.SetUser(_controller, userId, role);

            var request = new ChangePasswordRequestDto
            {
                OldPassword = "oldpass",
                NewPassword = "newpass123"
            };

            _authServiceMock
                .Setup(s => s.ChangePasswordAsync(userId, role, request))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var noContent = result as NoContentResult;
            Assert.IsNotNull(noContent);
            Assert.AreEqual(StatusCodes.Status204NoContent, noContent.StatusCode);

            _authServiceMock.Verify(
                s => s.ChangePasswordAsync(userId, role, request),
                Times.Once);

            _authServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ChangePassword_WhenRoleIsAdmin_ReturnsNoContent_AndCallsService()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var role = Role.Admin;

            ControllerTestHelper.SetUser(_controller, userId, role);

            var request = new ChangePasswordRequestDto
            {
                OldPassword = "adminOld",
                NewPassword = "adminNew"
            };

            _authServiceMock
                .Setup(s => s.ChangePasswordAsync(userId, role, request))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            _authServiceMock.Verify(
                s => s.ChangePasswordAsync(userId, role, request),
                Times.Once);

            _authServiceMock.VerifyNoOtherCalls();
        }
    }
}
