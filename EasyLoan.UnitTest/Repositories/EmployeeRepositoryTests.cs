using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Models;
using EasyLoan.DataAccess.Repositories;
using EasyLoan.Models.Common.Enums;
using Microsoft.EntityFrameworkCore;

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
                .UseInMemoryDatabase($"EasyLoan_TestDb_{Guid.NewGuid()}")
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

        private static Employee CreateValidEmployee(EmployeeRole role, string email)
        {
            return new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Test Employee",
                Email = email,
                PhoneNumber = "9876543210",
                Password = "HashedPassword@123",
                Role = role
            };
        }

        private static Customer CreateValidCustomer(string email, string pan)
        {
            return new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Test Customer",
                Email = email,
                PhoneNumber = "9999999999",
                Password = "HashedPassword@123",
                DateOfBirth = new DateTime(1999, 1, 1),
                AnnualSalary = 500000,
                PanNumber = pan,
                CreditScore = 750,
                CreatedDate = DateTime.UtcNow
            };
        }

        private static LoanType CreateValidLoanType(string name)
        {
            return new LoanType
            {
                Id = Guid.NewGuid(),
                Name = name,
                InterestRate = 7.5m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };
        }

        private static LoanApplication CreateValidLoanApplication(
            string applicationNumber,
            Guid customerId,
            Guid loanTypeId,
            Guid assignedEmployeeId,
            Guid approvedByEmployeeId)
        {
            return new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = applicationNumber,
                CustomerId = customerId,
                LoanTypeId = loanTypeId,
                AssignedEmployeeId = assignedEmployeeId,
                RequestedAmount = 500000,
                ApprovedAmount = 0,
                RequestedTenureInMonths = 240,
                Status = LoanApplicationStatus.Pending,
                ManagerComments = null,
                CreatedDate = DateTime.UtcNow,

            };
        }

        [TestMethod]
        public async Task GetAllWithDetailsAsync_ReturnsEmployees_WithAssignedLoanApplications()
        {
            var employee = CreateValidEmployee(EmployeeRole.Manager,"manager@test.com");

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Customer One",
                Email = $"cust{Guid.NewGuid()}@test.com",
                PhoneNumber = "9876543210",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                AnnualSalary = 500000,
                PanNumber = "ABCDE1234F",
                Password = "HashedPassword@123",
                CreditScore = 700,
                CreatedDate = DateTime.UtcNow
            };

            var loanType = new LoanType
            {
                Id = Guid.NewGuid(),
                Name = $"Home Loan {Guid.NewGuid()}",
                InterestRate = 8.5m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };

            var application = new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = "LA-ABCDEFGH",
                CustomerId = customer.Id,
                LoanTypeId = loanType.Id,
                AssignedEmployeeId = employee.Id,
                RequestedAmount = 500000,
                ApprovedAmount = 0,
                RequestedTenureInMonths = 240,
                Status = LoanApplicationStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            _context.LoanTypes.Add(loanType);
            _context.Employees.Add(employee);
            _context.LoanApplications.Add(application);

            await _context.SaveChangesAsync();

            var result = await _repo.GetAllWithDetailsAsync();
            var list = result.ToList();

            Assert.AreEqual(1, list.Count);

            var loadedEmployee = list[0];

            Assert.IsNotNull(loadedEmployee.AssignedLoanApplications);
            Assert.AreEqual(1, loadedEmployee.AssignedLoanApplications.Count);
        }


        [TestMethod]
        public async Task GetAllWithDetailsAsync_WhenNoEmployees_ReturnsEmpty()
        {
            // Act
            var result = await _repo.GetAllWithDetailsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetByEmailAsync_WhenEmailExists_ReturnsEmployee()
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
        public async Task GetByEmailAsync_WhenEmailDoesNotExist_ReturnsNull()
        {
            // Arrange
            _context.Employees.Add(CreateValidEmployee(EmployeeRole.Manager, "a@test.com"));
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
            _context.Employees.Add(CreateValidEmployee(EmployeeRole.Manager, "a@test.com"));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetByEmailAsync("");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetManagersAsync_WhenMixedRolesExist_ReturnsOnlyManagers()
        {
            // Arrange
            _context.Employees.AddRange(
                CreateValidEmployee(EmployeeRole.Manager, "manager@test.com"),
                CreateValidEmployee(EmployeeRole.Admin, "admin@test.com"),
                CreateValidEmployee(EmployeeRole.Manager, "manager2@test.com")
            );

            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetManagersAsync();
            var list = result.ToList();

            // Assert
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.All(e => e.Role == EmployeeRole.Manager));
        }

        [TestMethod]
        public async Task GetManagersAsync_WhenNoManagersExist_ReturnsEmpty()
        {
            // Arrange
            _context.Employees.AddRange(
                CreateValidEmployee(EmployeeRole.Admin, "admin@test.com"),
                CreateValidEmployee(EmployeeRole.Admin, "admin2@test.com")
            );

            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetManagersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task ExistsByEmailAsync_WhenEmailExists_ReturnsTrue()
        {
            // Arrange
            _context.Employees.Add(CreateValidEmployee(EmployeeRole.Manager, "exists@test.com"));
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
            _context.Employees.Add(CreateValidEmployee(EmployeeRole.Manager, "a@test.com"));
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repo.ExistsByEmailAsync("nope@test.com");

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task ExistsByEmailAsync_WhenEmailIsEmpty_ReturnsFalse()
        {
            // Arrange
            _context.Employees.Add(CreateValidEmployee(EmployeeRole.Manager, "a@test.com"));
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repo.ExistsByEmailAsync("");

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task ExistsByEmailAsync_IsCaseSensitive_InMemoryProviderBehavior()
        {
            // Arrange
            _context.Employees.Add(CreateValidEmployee(EmployeeRole.Manager, "Case@Test.com"));
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
