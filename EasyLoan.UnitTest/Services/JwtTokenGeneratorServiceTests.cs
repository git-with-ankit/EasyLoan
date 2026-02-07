using EasyLoan.Business.Services;
using EasyLoan.Models.Common.Enums;
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
                // NOTE:
                // HmacSha256 requires a sufficiently long key.
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
        public void GenerateToken_WhenCalled_ReturnsNonEmptyJwt_With3Parts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "user@test.com";
            var role = Role.Customer;

            // Act
            var token = _service.GenerateToken(userId, email, role);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(token));
            Assert.AreEqual(3, token.Split('.').Length);
        }

        [TestMethod]
        public void GenerateToken_WhenCalled_ContainsUserIdEmailAndRoleClaims()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "manager@test.com";
            var role = Role.Manager;

            // Act
            var token = _service.GenerateToken(userId, email, role);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Assert
            var nameIdClaim = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            var emailClaim = jwt.Claims.First(c => c.Type == ClaimTypes.Email);
            var roleClaim = jwt.Claims.First(c => c.Type == ClaimTypes.Role);

            Assert.AreEqual(userId.ToString(), nameIdClaim.Value);
            Assert.AreEqual(email, emailClaim.Value);
            Assert.AreEqual(role.ToString(), roleClaim.Value);
        }

        [TestMethod]
        public void GenerateToken_WhenCalled_SetsIssuerAndAudience()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "admin@test.com";
            var role = Role.Admin;

            // Act
            var token = _service.GenerateToken(userId, email, role);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            Assert.AreEqual("EasyLoan.TestIssuer", jwt.Issuer);
            Assert.AreEqual("EasyLoan.TestAudience", jwt.Audiences.First());
        }

        [TestMethod]
        public void GenerateToken_WhenCalled_SetsExpirationCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "user@test.com";
            var role = Role.Customer;

            // Act
            var token = _service.GenerateToken(userId, email, role);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.IsTrue(jwt.ValidTo > DateTime.UtcNow);
            Assert.IsTrue(jwt.ValidTo <= DateTime.UtcNow.AddMinutes(61));
        }

        [TestMethod]
        public void GenerateToken_WhenCalled_UsesHmacSha256()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "user@test.com";
            var role = Role.Customer;

            // Act
            var token = _service.GenerateToken(userId, email, role);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            Assert.AreEqual(SecurityAlgorithms.HmacSha256, jwt.Header.Alg);
        }

        [TestMethod]
        public void GenerateToken_WhenCalled_IncludesExactly3CustomClaims_ForCoverage()
        {
            var userId = Guid.NewGuid();
            var email = "user@test.com";
            var role = Role.Customer;

            var token = _service.GenerateToken(userId, email, role);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var customClaims = jwt.Claims
                .Where(c =>
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type == ClaimTypes.Email ||
                    c.Type == ClaimTypes.Role)
                .ToList();

            Assert.AreEqual(3, customClaims.Count);

            Assert.AreEqual(userId.ToString(),
                customClaims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            Assert.AreEqual(email,
                customClaims.First(c => c.Type == ClaimTypes.Email).Value);

            Assert.AreEqual(role.ToString(),
                customClaims.First(c => c.Type == ClaimTypes.Role).Value);
        }

    }
}
