using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Models;
using EasyLoan.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
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

        private static Customer CreateValidCustomer(
            string? email = null,
            string? pan = null)
        {
            return new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = email ?? $"user{Guid.NewGuid()}@test.com",
                PhoneNumber = "9876543210",
                Password = "HashedPassword@123",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                AnnualSalary = 500000,
                PanNumber = pan ?? "ABCDE1234F",
                CreditScore = 750,
                CreatedDate = DateTime.UtcNow
            };
        }

        [TestMethod]
        public async Task GetByEmailAsync_EmailExists_ReturnsCustomer()
        {
            // Arrange
            var customer = CreateValidCustomer(email: "test@test.com");
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
        public async Task GetByEmailAsync_EmailDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _repo.GetByEmailAsync("missing@test.com");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ExistsByEmailAsync_EmailExists_ReturnsTrue()
        {
            // Arrange
            var customer = CreateValidCustomer(email: "exists@test.com");
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repo.ExistsByEmailAsync("exists@test.com");

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task ExistsByEmailAsync_EmailDoesNotExist_ReturnsFalse()
        {
            // Act
            var exists = await _repo.ExistsByEmailAsync("absent@test.com");

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task ExistsByPanAsync_PanExists_ReturnsTrue()
        {
            // Arrange
            var customer = CreateValidCustomer(pan: "ABCDE9999F");
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repo.ExistsByPanAsync("ABCDE9999F");

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task ExistsByPanAsync_PanDoesNotExist_ReturnsFalse()
        {
            // Act
            var exists = await _repo.ExistsByPanAsync("ABCDE0000F");

            // Assert
            Assert.IsFalse(exists);
        }
    }
}
