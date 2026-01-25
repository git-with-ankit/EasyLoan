using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Models;
using EasyLoan.DataAccess.Repositories;
using EasyLoan.Models.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.UnitTest.Repositories
{
    [TestClass]
    public class LoanDetailsRepositoryTests
    {
        private EasyLoanDbContext _context = null!;
        private LoanDetailsRepository _repo = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EasyLoanDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new EasyLoanDbContext(options);
            _repo = new LoanDetailsRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public async Task GetLoansByCustomerIdWithDetailsAsync_ExistingCustomer_ReturnsLoans()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var loan1 = new LoanDetails
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                LoanApplicationId = Guid.NewGuid(),
                LoanTypeId = Guid.NewGuid(),
                ApprovedByEmployeeId = Guid.NewGuid(),
                LoanNumber = "LN-11111111",
                ApprovedAmount = 500000,
                PrincipalRemaining = 400000,
                TenureInMonths = 240,
                InterestRate = 8.5m,
                Status = LoanStatus.Active,
                CreatedDate = DateTime.UtcNow
            };

            var loan2 = new LoanDetails
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                LoanApplicationId = Guid.NewGuid(),
                LoanTypeId = Guid.NewGuid(),
                ApprovedByEmployeeId = Guid.NewGuid(),
                LoanNumber = "LN-22222222",
                ApprovedAmount = 300000,
                PrincipalRemaining = 250000,
                TenureInMonths = 180,
                InterestRate = 9.0m,
                Status = LoanStatus.Active,
                CreatedDate = DateTime.UtcNow
            };

            var otherLoan = new LoanDetails
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid(),
                LoanTypeId = Guid.NewGuid(),
                ApprovedByEmployeeId = Guid.NewGuid(),
                LoanNumber = "LN-33333333",
                ApprovedAmount = 200000,
                PrincipalRemaining = 150000,
                TenureInMonths = 120,
                InterestRate = 10.0m,
                Status = LoanStatus.Active,
                CreatedDate = DateTime.UtcNow
            };

            _context.Loans.AddRange(loan1, loan2, otherLoan);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetLoansByCustomerIdWithDetailsAsync(customerId);

            // Assert
            var list = result.ToList();
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.All(l => l.CustomerId == customerId));
        }

        [TestMethod]
        public async Task GetLoansByCustomerIdWithDetailsAsync_NoLoans_ReturnsEmptyList()
        {
            // Act
            var result = await _repo.GetLoansByCustomerIdWithDetailsAsync(Guid.NewGuid());

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetByLoanNumberWithDetailsAsync_Exists_ReturnsLoanWithIncludes()
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
                CustomerId = Guid.NewGuid(),
                LoanTypeId = loanType.Id,
                AssignedEmployeeId = Guid.NewGuid(),
                RequestedAmount = 500000,
                RequestedTenureInMonths = 240,
                Status = LoanApplicationStatus.Pending
            };

            var emi = new LoanEmi
            {
                Id = Guid.NewGuid(),
                EmiNumber = 1,
                RemainingAmount = 10000,
                DueDate = DateTime.UtcNow.AddMonths(1)
            };

            var loan = new LoanDetails
            {
                Id = Guid.NewGuid(),
                LoanNumber = "LN-ABCDEFGH",
                CustomerId = Guid.NewGuid(),
                LoanApplicationId = application.Id,
                LoanTypeId = loanType.Id,
                ApprovedByEmployeeId = Guid.NewGuid(),
                ApprovedAmount = 500000,
                PrincipalRemaining = 490000,
                TenureInMonths = 240,
                InterestRate = 8.5m,
                Status = LoanStatus.Active,
                CreatedDate = DateTime.UtcNow,
                LoanType = loanType,
                LoanApplication = application,
                Emis = new List<LoanEmi> { emi }
            };

            _context.LoanTypes.Add(loanType);
            _context.LoanApplications.Add(application);
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetByLoanNumberWithDetailsAsync("LN-ABCDEFGH");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.LoanType);
            Assert.IsNotNull(result.LoanApplication);
            Assert.IsNotNull(result.Emis);
            Assert.AreEqual(1, result.Emis.Count);
            Assert.AreEqual(1, result.Emis.First().EmiNumber);
        }

        [TestMethod]
        public async Task GetByLoanNumberWithDetailsAsync_NotFound_ReturnsNull()
        {
            // Act
            var result = await _repo.GetByLoanNumberWithDetailsAsync("LN-NOTFOUND");

            // Assert
            Assert.IsNull(result);
        }
    }
}
