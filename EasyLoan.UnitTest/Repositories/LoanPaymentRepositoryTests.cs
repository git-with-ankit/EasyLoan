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
                .UseInMemoryDatabase($"EasyLoan_TestDb_{Guid.NewGuid()}")
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

        private static LoanPayment CreatePayment(
            Guid loanId,
            Guid customerId,
            decimal amount,
            LoanPaymentStatus status)
        {
            return new LoanPayment
            {
                Id = Guid.NewGuid(),
                LoanDetailsId = loanId,
                CustomerId = customerId,
                Amount = amount,
                TransactionId = Guid.NewGuid(),
                Status = status,
                PaymentDate = DateTime.UtcNow
            };
        }

        [TestMethod]
        public async Task GetByLoanIdAsync_WhenPaymentsExist_ReturnsOnlyMatchingLoanPayments()
        {
            var loanId = Guid.NewGuid();
            var otherLoanId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var p1 = CreatePayment(loanId, customerId, 5000, LoanPaymentStatus.Paid);
            var p2 = CreatePayment(loanId, customerId, 7000, LoanPaymentStatus.Paid);
            var otherPayment = CreatePayment(otherLoanId, customerId, 1000, LoanPaymentStatus.Paid);

            _context.LoanPayments.AddRange(p1, p2, otherPayment);
            await _context.SaveChangesAsync();

            var result = (await _repo.GetByLoanIdAsync(loanId)).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(p => p.LoanDetailsId == loanId));
        }

        [TestMethod]
        public async Task GetByLoanIdAsync_WhenNoPaymentsExist_ReturnsEmpty()
        {
            var result = await _repo.GetByLoanIdAsync(Guid.NewGuid());

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetByLoanIdAsync_WhenPaymentsExist_ReturnsAsNoTrackingEntities()
        {
            var loanId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var payment = CreatePayment(loanId, customerId, 2500, LoanPaymentStatus.Paid);

            _context.LoanPayments.Add(payment);
            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();

            var result = (await _repo.GetByLoanIdAsync(loanId)).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(EntityState.Detached, _context.Entry(result[0]).State);
        }
    }
}
