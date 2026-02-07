using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Models;
using EasyLoan.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.UnitTest.Repositories
{
    [TestClass]
    public class CustomerRepositoryTests
    {
        private EasyLoanDbContext _context = null!;
        private CustomerRepository _repo = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EasyLoanDbContext>()
                .UseInMemoryDatabase($"EasyLoan_TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new EasyLoanDbContext(options);
            _repo = new CustomerRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private static Customer CreateValidCustomer(string email, string pan)
        {
            return new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = email,
                PhoneNumber = "9876543210",
                Password = "HashedPassword@123",
                DateOfBirth = new DateTime(1999, 1, 1),
                AnnualSalary = 500000,
                PanNumber = pan,
                CreditScore = 750,
                CreatedDate = DateTime.UtcNow
            };
        }

        [TestMethod]
        public async Task GetByEmailAsync_WhenEmailExists_ReturnsCustomer()
        {
            // Arrange
            var customer = CreateValidCustomer("test@test.com", "ABCDE1111F");
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetByEmailAsync("test@test.com");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(customer.Id, result.Id);
            Assert.AreEqual(customer.Email, result.Email);
        }

        [TestMethod]
        public async Task GetByEmailAsync_WhenEmailDoesNotExist_ReturnsNull()
        {
            // Arrange
            _context.Customers.Add(CreateValidCustomer("a@test.com", "ABCDE1111F"));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetByEmailAsync("missing@test.com");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetByEmailAsync_WhenEmailIsEmpty_ReturnsNull()
        {
            // Arrange
            _context.Customers.Add(CreateValidCustomer("exists@test.com", "ABCDE1111F"));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetByEmailAsync("");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ExistsByEmailAsync_WhenEmailExists_ReturnsTrue()
        {
            // Arrange
            _context.Customers.Add(CreateValidCustomer("exists@test.com", "ABCDE1111F"));
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repo.ExistsByEmailAsync("exists@test.com");

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task ExistsByEmailAsync_WhenEmailDoesNotExist_ReturnsFalse()
        {
            // Arrange
            _context.Customers.Add(CreateValidCustomer("a@test.com", "ABCDE1111F"));
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repo.ExistsByEmailAsync("absent@test.com");

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task ExistsByEmailAsync_WhenEmailIsEmpty_ReturnsFalse()
        {
            // Arrange
            _context.Customers.Add(CreateValidCustomer("a@test.com", "ABCDE1111F"));
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repo.ExistsByEmailAsync("");

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task ExistsByPanAsync_WhenPanExists_ReturnsTrue()
        {
            // Arrange
            _context.Customers.Add(CreateValidCustomer("a@test.com", "ABCDE9999F"));
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repo.ExistsByPanAsync("ABCDE9999F");

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task ExistsByPanAsync_WhenPanDoesNotExist_ReturnsFalse()
        {
            // Arrange
            _context.Customers.Add(CreateValidCustomer("a@test.com", "ABCDE9999F"));
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repo.ExistsByPanAsync("ABCDE0000F");

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task ExistsByPanAsync_WhenPanIsEmpty_ReturnsFalse()
        {
            // Arrange
            _context.Customers.Add(CreateValidCustomer("a@test.com", "ABCDE9999F"));
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repo.ExistsByPanAsync("");

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task GetByEmailAsync_WhenMultipleCustomersExist_ReturnsCorrectOne()
        {
            // Arrange
            var c1 = CreateValidCustomer("a@test.com", "ABCDE1111F");
            var c2 = CreateValidCustomer("b@test.com", "ABCDE2222F");
            var c3 = CreateValidCustomer("c@test.com", "ABCDE3333F");

            _context.Customers.AddRange(c1, c2, c3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetByEmailAsync("b@test.com");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(c2.Id, result.Id);
            Assert.AreEqual("b@test.com", result.Email);
        }

        [TestMethod]
        public async Task ExistsByEmailAsync_IsCaseSensitive_InMemoryProviderBehavior()
        {
            // Arrange
            _context.Customers.Add(CreateValidCustomer("Case@Test.com", "ABCDE1111F"));
            await _context.SaveChangesAsync();

            // Act
            var existsExact = await _repo.ExistsByEmailAsync("Case@Test.com");
            var existsDifferentCase = await _repo.ExistsByEmailAsync("case@test.com");

            // Assert
            Assert.IsTrue(existsExact);
            Assert.IsFalse(existsDifferentCase);
        }
    }
}
