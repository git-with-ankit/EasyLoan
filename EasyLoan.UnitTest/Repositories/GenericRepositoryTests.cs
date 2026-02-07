using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Models;
using EasyLoan.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.UnitTest.Repositories
{
    [TestClass]
    public class GenericRepositoryTests
    {
        private EasyLoanDbContext _context = null!;
        private GenericRepository<Customer> _repo = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EasyLoanDbContext>()
                .UseInMemoryDatabase($"EasyLoan_TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new EasyLoanDbContext(options);
            _repo = new GenericRepository<Customer>(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private static Customer CreateValidCustomer(string? email = null, string? pan = null)
        {
            return new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = email ?? $"user{Guid.NewGuid()}@test.com",
                PhoneNumber = "9876543210",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                AnnualSalary = 500000,
                PanNumber = pan ?? $"ABCDE{new Random().Next(1000, 9999)}F",
                Password = "HashedPassword123!",
                CreditScore = 750,
                CreatedDate = DateTime.UtcNow
            };
        }

        [TestMethod]
        public async Task AddAsync_WhenCalled_DoesNotPersistUntilSaveChanges()
        {
            // Arrange
            var customer = CreateValidCustomer();

            // Act
            await _repo.AddAsync(customer);

            // Assert (not yet saved)
            Assert.AreEqual(0, await _context.Customers.CountAsync());

            // Act
            await _repo.SaveChangesAsync();

            // Assert (saved now)
            Assert.AreEqual(1, await _context.Customers.CountAsync());
        }

        [TestMethod]
        public async Task AddAsync_WhenCalled_PersistsEntityAfterSaveChanges()
        {
            // Arrange
            var customer = CreateValidCustomer();

            // Act
            await _repo.AddAsync(customer);
            await _repo.SaveChangesAsync();

            // Assert
            var result = await _context.Customers.FindAsync(customer.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(customer.Email, result.Email);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenEntityExists_ReturnsEntity()
        {
            // Arrange
            var customer = CreateValidCustomer();
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetByIdAsync(customer.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(customer.Id, result.Id);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenEntityDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _repo.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenEntityAlreadyTracked_ReturnsSameInstance()
        {
            // Arrange
            var customer = CreateValidCustomer();
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Load tracked entity into context
            var tracked = await _context.Customers.FirstAsync(c => c.Id == customer.Id);

            // Act
            var result = await _repo.GetByIdAsync(customer.Id);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreSame(tracked, result);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenEntitiesExist_ReturnsAll()
        {
            // Arrange
            _context.Customers.AddRange(
                CreateValidCustomer(),
                CreateValidCustomer()
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetAllAsync();

            // Assert
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetAllAsync_WhenNoEntitiesExist_ReturnsEmptyCollection()
        {
            // Act
            var result = await _repo.GetAllAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetAllAsync_ReturnsEntitiesAsNoTracking()
        {
            // Arrange
            var customer = CreateValidCustomer();
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Act
            var result = (await _repo.GetAllAsync()).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);

            var entry = _context.Entry(result[0]);
            Assert.AreEqual(EntityState.Detached, entry.State);
        }

        [TestMethod]
        public async Task UpdateAsync_WhenCalled_UpdatesEntity()
        {
            // Arrange
            var customer = CreateValidCustomer();
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            customer.Name = "Updated Name";

            // Act
            await _repo.UpdateAsync(customer);
            await _repo.SaveChangesAsync();

            // Assert
            var updated = await _context.Customers.FindAsync(customer.Id);
            Assert.IsNotNull(updated);
            Assert.AreEqual("Updated Name", updated.Name);
        }

        [TestMethod]
        public async Task UpdateAsync_WhenEntityDetached_StillUpdatesEntity()
        {
            // Arrange
            var customer = CreateValidCustomer();
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();

            customer.Name = "Detached Update";

            // Act
            await _repo.UpdateAsync(customer);
            await _repo.SaveChangesAsync();

            // Assert
            var updated = await _context.Customers.FindAsync(customer.Id);
            Assert.IsNotNull(updated);
            Assert.AreEqual("Detached Update", updated.Name);
        }

        [TestMethod]
        public async Task SaveChangesAsync_PersistsPendingChanges()
        {
            // Arrange
            var customer = CreateValidCustomer();
            await _repo.AddAsync(customer);

            // Act
            await _repo.SaveChangesAsync();

            // Assert
            Assert.AreEqual(1, await _context.Customers.CountAsync());
        }
    }
}
