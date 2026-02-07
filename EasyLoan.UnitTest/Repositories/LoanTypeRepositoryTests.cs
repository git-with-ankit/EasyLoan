using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Models;
using EasyLoan.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.UnitTest.Repositories
{
    [TestClass]
    public class LoanTypeRepositoryTests
    {
        private EasyLoanDbContext _context = null!;
        private LoanTypeRepository _repo = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EasyLoanDbContext>()
                .UseInMemoryDatabase($"EasyLoan_TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new EasyLoanDbContext(options);
            _repo = new LoanTypeRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private static LoanType CreateValidLoanType(string? name = null)
        {
            return new LoanType
            {
                Id = Guid.NewGuid(),
                Name = name ?? $"LoanType-{Guid.NewGuid()}",
                InterestRate = 8.5m,
                MinAmount = 50000m,
                MaxTenureInMonths = 240
            };
        }

        [TestMethod]
        public async Task AddAsync_WhenCalled_PersistsLoanType()
        {
            var entity = CreateValidLoanType("Home Loan");

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            var saved = await _context.LoanTypes.FindAsync(entity.Id);

            Assert.IsNotNull(saved);
            Assert.AreEqual(entity.Id, saved.Id);
            Assert.AreEqual("Home Loan", saved.Name);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenExists_ReturnsEntity()
        {
            var entity = CreateValidLoanType("Car Loan");

            _context.LoanTypes.Add(entity);
            await _context.SaveChangesAsync();

            var result = await _repo.GetByIdAsync(entity.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(entity.Id, result.Id);
            Assert.AreEqual("Car Loan", result.Name);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
        {
            var result = await _repo.GetByIdAsync(Guid.NewGuid());

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenMultipleExist_ReturnsAll_AsNoTracking()
        {
            var t1 = CreateValidLoanType("Education Loan");
            var t2 = CreateValidLoanType("Personal Loan");

            _context.LoanTypes.AddRange(t1, t2);
            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();

            var result = (await _repo.GetAllAsync()).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(x => x.Name == "Education Loan"));
            Assert.IsTrue(result.Any(x => x.Name == "Personal Loan"));

            Assert.IsTrue(result.All(x => _context.Entry(x).State == EntityState.Detached));
        }

        [TestMethod]
        public async Task UpdateAsync_WhenCalled_UpdatesEntity()
        {
            var entity = CreateValidLoanType("Original Loan");

            _context.LoanTypes.Add(entity);
            await _context.SaveChangesAsync();

            entity.InterestRate = 9.25m;
            entity.MinAmount = 100000m;
            entity.MaxTenureInMonths = 180;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();

            var updated = await _context.LoanTypes.FindAsync(entity.Id);

            Assert.IsNotNull(updated);
            Assert.AreEqual("Original Loan", updated.Name);
            Assert.AreEqual(9.25m, updated.InterestRate);
            Assert.AreEqual(100000m, updated.MinAmount);
            Assert.AreEqual(180, updated.MaxTenureInMonths);
        }

        [TestMethod]
        public async Task SaveChangesAsync_WhenCalledWithoutChanges_DoesNotThrow()
        {
            await _repo.SaveChangesAsync();

            Assert.AreEqual(0, await _context.LoanTypes.CountAsync());
        }
    }
}
