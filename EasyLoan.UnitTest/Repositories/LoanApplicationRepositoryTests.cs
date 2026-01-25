using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Models;
using EasyLoan.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EasyLoan.UnitTest.Repositories
{
    [TestClass]
    public class LoanApplicationRepositoryTests
    {
        private EasyLoanDbContext _context = null!;
        private LoanApplicationRepository _repo = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EasyLoanDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new EasyLoanDbContext(options);
            _repo = new LoanApplicationRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public async Task GetAllWithDetailsAsync_ReturnsApplications_WithLoanTypeIncluded()
        {
            // Arrange
            var loanType = new LoanType
            {
                Id = Guid.NewGuid(),
                Name = "Home Loan",
                InterestRate = 8.5m
            };

            var application = new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = "LA-ABCDEFGH",
                LoanTypeId = loanType.Id,
                LoanType = loanType,
                CustomerId = Guid.NewGuid(),
                AssignedEmployeeId = Guid.NewGuid(),
                RequestedAmount = 500000,
                RequestedTenureInMonths = 240,
                Status = Models.Common.Enums.LoanApplicationStatus.Pending
            };

            _context.LoanTypes.Add(loanType);
            _context.LoanApplications.Add(application);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetAllWithDetailsAsync();

            // Assert
            var list = result.ToList();
            Assert.AreEqual(1, list.Count);
            Assert.IsNotNull(list[0].LoanType);
            Assert.AreEqual("Home Loan", list[0].LoanType.Name);
        }

        [TestMethod]
        public async Task GetByApplicationNumberWithDetailsAsync_ExistingApplication_ReturnsWithCustomerAndLoanType()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Customer One",
                Email = "cust@test.com",
                PhoneNumber = "9876543210",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                AnnualSalary = 600000,
                PanNumber = "ABCDE1234F",
                Password = "hashed"
            };

            var loanType = new LoanType
            {
                Id = Guid.NewGuid(),
                Name = "Car Loan",
                InterestRate = 9.5m
            };

            var application = new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = "LA-12345678",
                CustomerId = customer.Id,
                Customer = customer,
                LoanTypeId = loanType.Id,
                LoanType = loanType,
                AssignedEmployeeId = Guid.NewGuid(),
                RequestedAmount = 400000,
                RequestedTenureInMonths = 60,
                Status = Models.Common.Enums.LoanApplicationStatus.Pending
            };

            _context.Customers.Add(customer);
            _context.LoanTypes.Add(loanType);
            _context.LoanApplications.Add(application);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetByApplicationNumberWithDetailsAsync("LA-12345678");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Customer);
            Assert.IsNotNull(result.LoanType);
            Assert.AreEqual("Customer One", result.Customer.Name);
            Assert.AreEqual("Car Loan", result.LoanType.Name);
        }

        [TestMethod]
        public async Task GetByApplicationNumberWithDetailsAsync_NotFound_ReturnsNull()
        {
            // Act
            var result = await _repo.GetByApplicationNumberWithDetailsAsync("LA-NOTFOUND");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetByCustomerIdWithDetailsAsync_ReturnsOnlyApplicationsForThatCustomer()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var loanType = new LoanType
            {
                Id = Guid.NewGuid(),
                Name = "Personal Loan",
                InterestRate = 10.5m
            };

            var app1 = new LoanApplication
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                ApplicationNumber = "LA-11111111",
                LoanType = loanType,
                AssignedEmployeeId = Guid.NewGuid(),
                RequestedAmount = 200000,
                RequestedTenureInMonths = 36,
                Status = Models.Common.Enums.LoanApplicationStatus.Pending
            };

            var app2 = new LoanApplication
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(), // other customer
                ApplicationNumber = "LA-22222222",
                LoanType = loanType,
                AssignedEmployeeId = Guid.NewGuid(),
                RequestedAmount = 300000,
                RequestedTenureInMonths = 48,
                Status = Models.Common.Enums.LoanApplicationStatus.Pending
            };

            _context.LoanTypes.Add(loanType);
            _context.LoanApplications.AddRange(app1, app2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetByCustomerIdWithDetailsAsync(customerId);

            // Assert
            var list = result.ToList();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("LA-11111111", list[0].ApplicationNumber);
            Assert.IsNotNull(list[0].LoanType);
        }

        [TestMethod]
        public async Task GetByCustomerIdWithDetailsAsync_NoApplications_ReturnsEmptyList()
        {
            // Act
            var result = await _repo.GetByCustomerIdWithDetailsAsync(Guid.NewGuid());

            // Assert
            Assert.AreEqual(0, result.Count());
        }
    }
}
