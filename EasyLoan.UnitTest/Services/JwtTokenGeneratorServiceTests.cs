using EasyLoan.Business.Enums;
using EasyLoan.Business.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    public class JwtTokenGeneratorServiceTests
    {
        private JwtTokenGeneratorService _service = null!;
        private IConfiguration _configuration = null!;

        [TestInitialize]
        public void Setup()
        {
            var settings = new Dictionary<string, string>
        {
            { "Jwt:Key", "THIS_IS_A_TEST_KEY_1234567890123456" },
            { "Jwt:Issuer", "EasyLoan.TestIssuer" },
            { "Jwt:Audience", "EasyLoan.TestAudience" },
            { "Jwt:AccessTokenExpiryMinutes", "60" }
        };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();

            _service = new JwtTokenGeneratorService(_configuration);
        }
        [TestMethod]
        public void GenerateToken_WhenCalled_ReturnsNonEmptyJwt()
        {
            // Act
            var token = _service.GenerateToken(Guid.NewGuid(), Role.Customer);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(token));
            Assert.AreEqual(3, token.Split('.').Length); // JWT has 3 parts header.payload.signature
        }
        [TestMethod]
        public void GenerateToken_WhenCalled_ContainsUserIdAndRoleClaims()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var role = Role.Manager;

            // Act
            var token = _service.GenerateToken(userId, role);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Assert
            var nameIdClaim = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = jwt.Claims.First(c => c.Type == ClaimTypes.Role);

            Assert.AreEqual(userId.ToString(), nameIdClaim.Value);
            Assert.AreEqual(role.ToString(), roleClaim.Value);
        }
        [TestMethod]
        public void GenerateToken_WhenCalled_SetsIssuerAndAudience()
        {
            // Act
            var token = _service.GenerateToken(Guid.NewGuid(), Role.Admin);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            Assert.AreEqual("EasyLoan.TestIssuer", jwt.Issuer);
            Assert.AreEqual("EasyLoan.TestAudience", jwt.Audiences.First());
        }
        [TestMethod]
        public void GenerateToken_WhenCalled_SetsExpirationCorrectly()
        {
            // Act
            var token = _service.GenerateToken(Guid.NewGuid(), Role.Customer);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            Assert.IsTrue(jwt.ValidTo > DateTime.UtcNow);
            Assert.IsTrue(jwt.ValidTo <= DateTime.UtcNow.AddMinutes(61));
        }

        [TestMethod]
        public void GenerateToken_WhenCalled_UsesHmacSha256()
        {
            // Act
            var token = _service.GenerateToken(Guid.NewGuid(), Role.Customer);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            Assert.AreEqual(SecurityAlgorithms.HmacSha256, jwt.Header.Alg);
        }
    }

}


