using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Models;
using EasyLoan.DataAccess.Repositories;
using EasyLoan.Models.Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                .UseInMemoryDatabase($"EasyLoan_TestDb_{Guid.NewGuid()}")
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

        private static LoanType CreateLoanType(string? name = null)
        {
            return new LoanType
            {
                Id = Guid.NewGuid(),
                Name = name ?? $"LoanType-{Guid.NewGuid()}",
                InterestRate = 8.5m,
                MinAmount = 10000,
                MaxTenureInMonths = 240
            };
        }

        private static LoanApplication CreateLoanApplication(Guid customerId, Guid loanTypeId, string applicationNumber)
        {
            return new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = applicationNumber,
                CustomerId = customerId,
                LoanTypeId = loanTypeId,
                AssignedEmployeeId = Guid.NewGuid(),
                RequestedAmount = 500000,
                ApprovedAmount = 450000,
                RequestedTenureInMonths = 240,
                Status = LoanApplicationStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };
        }

        private static LoanEmi CreateEmi(Guid loanDetailsId, int emiNumber, decimal remainingAmount)
        {
            return new LoanEmi
            {
                Id = Guid.NewGuid(),
                LoanDetailsId = loanDetailsId,
                EmiNumber = emiNumber,
                DueDate = DateTime.UtcNow.AddMonths(emiNumber),
                TotalAmount = 10000,
                RemainingAmount = remainingAmount,
                InterestComponent = 4000,
                PrincipalComponent = 6000,
                PenaltyAmount = 0,
                PaidPenaltyAmount = 0,
                PaidDate = null
            };
        }

        private static LoanDetails CreateLoan(
            Guid customerId,
            Guid loanTypeId,
            Guid applicationId,
            string loanNumber)
        {
            return new LoanDetails
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                LoanApplicationId = applicationId,
                LoanTypeId = loanTypeId,
                ApprovedByEmployeeId = Guid.NewGuid(),
                LoanNumber = loanNumber,
                ApprovedAmount = 500000,
                PrincipalRemaining = 490000,
                TenureInMonths = 240,
                InterestRate = 8.5m,
                Status = LoanStatus.Active,
                CreatedDate = DateTime.UtcNow
            };
        }

        [TestMethod]
        public async Task GetLoansByCustomerIdWithDetailsAsync_WhenLoansExist_ReturnsOnlyCustomerLoans_WithEmisIncluded()
        {
            var customerId = Guid.NewGuid();
            var otherCustomerId = Guid.NewGuid();

            var loanType = CreateLoanType("Home Loan");

            var app1 = CreateLoanApplication(customerId, loanType.Id, "LA-AAAA1111");
            var app2 = CreateLoanApplication(customerId, loanType.Id, "LA-BBBB2222");
            var appOther = CreateLoanApplication(otherCustomerId, loanType.Id, "LA-CCCC3333");

            var loan1 = CreateLoan(customerId, loanType.Id, app1.Id, "LN-11111111");
            var loan2 = CreateLoan(customerId, loanType.Id, app2.Id, "LN-22222222");
            var otherLoan = CreateLoan(otherCustomerId, loanType.Id, appOther.Id, "LN-33333333");

            var emi11 = CreateEmi(loan1.Id, 1, 10000);
            var emi12 = CreateEmi(loan1.Id, 2, 0);
            var emi21 = CreateEmi(loan2.Id, 1, 10000);

            loan1.Emis.Add(emi11);
            loan1.Emis.Add(emi12);
            loan2.Emis.Add(emi21);

            _context.LoanTypes.Add(loanType);
            _context.LoanApplications.AddRange(app1, app2, appOther);
            _context.Loans.AddRange(loan1, loan2, otherLoan);
            _context.LoanEmis.AddRange(emi11, emi12, emi21);
            await _context.SaveChangesAsync();

            var result = (await _repo.GetLoansByCustomerIdWithDetailsAsync(customerId)).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(l => l.CustomerId == customerId));

            var loadedLoan1 = result.Single(l => l.LoanNumber == "LN-11111111");
            Assert.IsNotNull(loadedLoan1.Emis);
            Assert.AreEqual(2, loadedLoan1.Emis.Count);

            var loadedLoan2 = result.Single(l => l.LoanNumber == "LN-22222222");
            Assert.IsNotNull(loadedLoan2.Emis);
            Assert.AreEqual(1, loadedLoan2.Emis.Count);
        }

        [TestMethod]
        public async Task GetLoansByCustomerIdWithDetailsAsync_WhenNoLoans_ReturnsEmpty()
        {
            var result = await _repo.GetLoansByCustomerIdWithDetailsAsync(Guid.NewGuid());
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetLoansByCustomerIdWithDetailsAsync_WhenCustomerHasLoansWithNoEmis_ReturnsLoansWithEmptyEmis()
        {
            var customerId = Guid.NewGuid();

            var loanType = CreateLoanType("Car Loan");
            var app = CreateLoanApplication(customerId, loanType.Id, "LA-DDDD4444");

            var loan = CreateLoan(customerId, loanType.Id, app.Id, "LN-44444444");

            _context.LoanTypes.Add(loanType);
            _context.LoanApplications.Add(app);
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var result = (await _repo.GetLoansByCustomerIdWithDetailsAsync(customerId)).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("LN-44444444", result[0].LoanNumber);
            Assert.IsNotNull(result[0].Emis);
            Assert.AreEqual(0, result[0].Emis.Count);
        }

        [TestMethod]
        public async Task GetByLoanNumberWithDetailsAsync_WhenExists_ReturnsLoan_WithLoanApplicationLoanTypeAndEmis()
        {
            var customerId = Guid.NewGuid();

            var loanType = CreateLoanType("Personal Loan");
            var app = CreateLoanApplication(customerId, loanType.Id, "LA-EEEE5555");

            var loan = CreateLoan(customerId, loanType.Id, app.Id, "LN-ABCDEFGH");

            var emi1 = CreateEmi(loan.Id, 1, 10000);
            var emi2 = CreateEmi(loan.Id, 2, 5000);

            loan.Emis.Add(emi1);
            loan.Emis.Add(emi2);

            _context.LoanTypes.Add(loanType);
            _context.LoanApplications.Add(app);
            _context.Loans.Add(loan);
            _context.LoanEmis.AddRange(emi1, emi2);
            await _context.SaveChangesAsync();

            var result = await _repo.GetByLoanNumberWithDetailsAsync("LN-ABCDEFGH");

            Assert.IsNotNull(result);
            Assert.AreEqual("LN-ABCDEFGH", result.LoanNumber);

            Assert.IsNotNull(result.LoanApplication);
            Assert.AreEqual(app.Id, result.LoanApplication.Id);

            Assert.IsNotNull(result.LoanType);
            Assert.AreEqual("Personal Loan", result.LoanType.Name);

            Assert.IsNotNull(result.Emis);
            Assert.AreEqual(2, result.Emis.Count);
            Assert.IsTrue(result.Emis.Any(e => e.EmiNumber == 1));
            Assert.IsTrue(result.Emis.Any(e => e.EmiNumber == 2));
        }

        [TestMethod]
        public async Task GetByLoanNumberWithDetailsAsync_WhenLoanExistsButNoEmis_ReturnsLoanWithEmptyEmis()
        {
            var customerId = Guid.NewGuid();

            var loanType = CreateLoanType("Home Loan");
            var app = CreateLoanApplication(customerId, loanType.Id, "LA-FFFF6666");

            var loan = CreateLoan(customerId, loanType.Id, app.Id, "LN-ZZZZ9999");

            _context.LoanTypes.Add(loanType);
            _context.LoanApplications.Add(app);
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var result = await _repo.GetByLoanNumberWithDetailsAsync("LN-ZZZZ9999");

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Emis);
            Assert.AreEqual(0, result.Emis.Count);
        }

        [TestMethod]
        public async Task GetByLoanNumberWithDetailsAsync_WhenNotFound_ReturnsNull()
        {
            var result = await _repo.GetByLoanNumberWithDetailsAsync("LN-NOTFOUND");
            Assert.IsNull(result);
        }
    }
}
