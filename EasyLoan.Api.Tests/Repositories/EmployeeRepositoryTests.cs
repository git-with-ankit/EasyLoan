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

        [TestMethod]
        public async Task GetAllWithDetailsAsync_ReturnsEmployees_WithAssignedLoanApplications()
        {
            // Arrange
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Manager One",
                Email = "manager@test.com",
                Role = EmployeeRole.Manager
            };

            var loanApp = new LoanApplication
            {
                Id = Guid.NewGuid(),
                AssignedEmployeeId = employee.Id,
                Status = LoanApplicationStatus.Pending
            };

            employee.AssignedLoanApplications = new List<LoanApplication> { loanApp };

            _context.Employees.Add(employee);
            _context.LoanApplications.Add(loanApp);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetAllWithDetailsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());

            var fetchedEmployee = result.First();
            Assert.AreEqual(employee.Id, fetchedEmployee.Id);
            Assert.IsNotNull(fetchedEmployee.AssignedLoanApplications);
            Assert.AreEqual(1, fetchedEmployee.AssignedLoanApplications.Count);
        }

    }
}
