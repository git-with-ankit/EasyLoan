using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Models;
using EasyLoan.DataAccess.Repositories;
using EasyLoan.Models.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.UnitTest.Repositories
{
    [TestClass]
    public class LoanPaymentRepositoryTests
    {
        private EasyLoanDbContext _context = null!;
        private LoanPaymentRepository _repo = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EasyLoanDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new EasyLoanDbContext(options);
            _repo = new LoanPaymentRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public async Task GetByLoanIdAsync_ExistingLoan_ReturnsPayments()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var p1 = new LoanPayment
            {
                Id = Guid.NewGuid(),
                LoanDetailsId = loanId,
                CustomerId = customerId,
                Amount = 5000,
                TransactionId = Guid.NewGuid(),
                Status = LoanPaymentStatus.Paid,
                PaymentDate = DateTime.UtcNow
            };

            var p2 = new LoanPayment
            {
                Id = Guid.NewGuid(),
                LoanDetailsId = loanId,
                CustomerId = customerId,
                Amount = 7000,
                TransactionId = Guid.NewGuid(),
                Status = LoanPaymentStatus.Paid,
                PaymentDate = DateTime.UtcNow
            };

            var otherPayment = new LoanPayment
            {
                Id = Guid.NewGuid(),
                LoanDetailsId = Guid.NewGuid(),
                CustomerId = customerId,
                Amount = 1000,
                TransactionId = Guid.NewGuid(),
                Status = LoanPaymentStatus.Paid,
                PaymentDate = DateTime.UtcNow
            };

            _context.LoanPayments.AddRange(p1, p2, otherPayment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetByLoanIdAsync(loanId);

            // Assert
            var list = result.ToList();
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.All(p => p.LoanDetailsId == loanId));
        }

        [TestMethod]
        public async Task GetByLoanIdAsync_NoPayments_ReturnsEmptyList()
        {
            // Act
            var result = await _repo.GetByLoanIdAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }
    }
}
