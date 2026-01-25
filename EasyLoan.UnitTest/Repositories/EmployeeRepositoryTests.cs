using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Models;
using EasyLoan.DataAccess.Repositories;
using EasyLoan.Models.Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyLoan.UnitTest.Repositories
{
    [TestClass]
    public class EmployeeRepositoryTests
    {
        private EasyLoanDbContext _context = null!;
        private EmployeeRepository _repo = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EasyLoanDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new EasyLoanDbContext(options);
            _repo = new EmployeeRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private static Employee CreateValidEmployee(
            EmployeeRole role,
            string? email = null)
        {
            return new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Test Employee",
                Email = email ?? $"emp{Guid.NewGuid()}@test.com",
                PhoneNumber = "9876543210",
                Password = "HashedPassword@123",
                Role = role
            };
        }

        [TestMethod]
        public async Task GetAllWithDetailsAsync_ReturnsEmployees_WithAssignedLoanApplications()
        {
            // Arrange
            var employee = CreateValidEmployee(EmployeeRole.Manager);

            var application = new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = "APP-001",
                AssignedEmployeeId = employee.Id
            };

            employee.AssignedLoanApplications.Add(application);

            _context.Employees.Add(employee);
            _context.LoanApplications.Add(application);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetAllWithDetailsAsync();
            var list = result.ToList();

            // Assert
            Assert.AreEqual(1, list.Count);

            var loadedEmployee = list[0];

            // This PROVES Include() executed
            Assert.IsNotNull(loadedEmployee.AssignedLoanApplications);
            Assert.AreEqual(1, loadedEmployee.AssignedLoanApplications.Count);
        }

        [TestMethod]
        public async Task GetByEmailAsync_EmailExists_ReturnsEmployee()
        {
            // Arrange
            var employee = CreateValidEmployee(EmployeeRole.Admin, "admin@test.com");

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetByEmailAsync("admin@test.com");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(employee.Id, result.Id);
            Assert.AreEqual(EmployeeRole.Admin, result.Role);
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
        public async Task GetManagersAsync_ReturnsOnlyManagers()
        {
            // Arrange
            _context.Employees.AddRange(
                CreateValidEmployee(EmployeeRole.Manager, "manager@test.com"),
                CreateValidEmployee(EmployeeRole.Admin, "admin@test.com")
            );

            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetManagersAsync();
            var list = result.ToList();

            // Assert
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(EmployeeRole.Manager, list[0].Role);
        }

        [TestMethod]
        public async Task GetManagersAsync_NoManagers_ReturnsEmptyList()
        {
            // Arrange
            _context.Employees.Add(CreateValidEmployee(EmployeeRole.Admin));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetManagersAsync();

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task ExistsByEmailAsync_EmailExists_ReturnsTrue()
        {
            // Arrange
            var employee = CreateValidEmployee(EmployeeRole.Manager, "exists@test.com");

            _context.Employees.Add(employee);
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
            var exists = await _repo.ExistsByEmailAsync("nope@test.com");

            // Assert
            Assert.IsFalse(exists);
        }
    }
}
