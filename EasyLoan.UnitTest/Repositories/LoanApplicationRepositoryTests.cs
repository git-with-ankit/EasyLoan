using EasyLoan.DataAccess;
using EasyLoan.DataAccess.Models;
using EasyLoan.DataAccess.Repositories;
using EasyLoan.Models.Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                .UseInMemoryDatabase($"EasyLoan_TestDb_{Guid.NewGuid()}")
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

        private static Customer CreateCustomer(string? email = null, string? pan = null)
        {
            return new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Customer",
                Email = email ?? $"cust{Guid.NewGuid()}@test.com",
                PhoneNumber = "9876543210",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                AnnualSalary = 600000,
                PanNumber = pan ?? $"ABCDE{new Random().Next(1000, 9999)}F",
                Password = "hashed",
                CreditScore = 750,
                CreatedDate = DateTime.UtcNow
            };
        }

        private static LoanDetails CreateLoanDetails(Guid customerId, Guid loanTypeId)
        {
            return new LoanDetails
            {
                Id = Guid.NewGuid(),
                LoanNumber = $"LN-{Guid.NewGuid():N}".Substring(0, 11),
                CustomerId = customerId,
                LoanApplicationId = Guid.NewGuid(),
                LoanTypeId = loanTypeId,
                ApprovedByEmployeeId = Guid.NewGuid(),
                ApprovedAmount = 100000,
                PrincipalRemaining = 90000,
                TenureInMonths = 24,
                InterestRate = 8.5m,
                Status = LoanStatus.Active,
                CreatedDate = DateTime.UtcNow
            };
        }

        private static LoanApplication CreateLoanApplication(
            string applicationNumber,
            Guid customerId,
            Guid loanTypeId)
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

        [TestMethod]
        public async Task GetAllWithDetailsAsync_WhenApplicationsExist_ReturnsApplicationsWithLoanTypeIncluded()
        {
            var loanType = CreateLoanType("Home Loan");
            var customer = CreateCustomer();
            var app = CreateLoanApplication("LA-ABCDEFGH", customer.Id, loanType.Id);

            _context.LoanTypes.Add(loanType);
            _context.Customers.Add(customer);
            _context.LoanApplications.Add(app);
            await _context.SaveChangesAsync();

            var result = (await _repo.GetAllWithDetailsAsync()).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(result[0].LoanType);
            Assert.AreEqual("Home Loan", result[0].LoanType.Name);

            var entry = _context.Entry(result[0]);
            Assert.AreEqual(EntityState.Detached, entry.State);
        }

        [TestMethod]
        public async Task GetAllWithDetailsAsync_WhenNoApplicationsExist_ReturnsEmpty()
        {
            var result = await _repo.GetAllWithDetailsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetByApplicationNumberWithDetailsAsync_WhenExists_ReturnsApplicationWithLoanTypeCustomerAndCustomerLoans()
        {
            var loanType = CreateLoanType("Car Loan");
            var customer = CreateCustomer("cust@test.com", "ABCDE1234F");

            var loan1 = CreateLoanDetails(customer.Id, loanType.Id);
            var loan2 = CreateLoanDetails(customer.Id, loanType.Id);

            customer.Loans.Add(loan1);
            customer.Loans.Add(loan2);

            var app = CreateLoanApplication("LA-12345678", customer.Id, loanType.Id);

            _context.LoanTypes.Add(loanType);
            _context.Customers.Add(customer);
            _context.Loans.AddRange(loan1, loan2);
            _context.LoanApplications.Add(app);
            await _context.SaveChangesAsync();

            var result = await _repo.GetByApplicationNumberWithDetailsAsync("LA-12345678");

            Assert.IsNotNull(result);
            Assert.AreEqual("LA-12345678", result.ApplicationNumber);

            Assert.IsNotNull(result.LoanType);
            Assert.AreEqual("Car Loan", result.LoanType.Name);

            Assert.IsNotNull(result.Customer);
            Assert.AreEqual("cust@test.com", result.Customer.Email);

            Assert.IsNotNull(result.Customer.Loans);
            Assert.AreEqual(2, result.Customer.Loans.Count);
        }

        [TestMethod]
        public async Task GetByApplicationNumberWithDetailsAsync_WhenNotFound_ReturnsNull()
        {
            var result = await _repo.GetByApplicationNumberWithDetailsAsync("LA-NOTFOUND");
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetByApplicationNumberWithDetailsAsync_WhenMultipleExist_ReturnsCorrectOne()
        {
            var loanType = CreateLoanType("Personal Loan");
            var customer1 = CreateCustomer("c1@test.com", "ABCDE1111F");
            var customer2 = CreateCustomer("c2@test.com", "ABCDE2222F");

            var app1 = CreateLoanApplication("LA-AAAA1111", customer1.Id, loanType.Id);
            var app2 = CreateLoanApplication("LA-BBBB2222", customer2.Id, loanType.Id);

            _context.LoanTypes.Add(loanType);
            _context.Customers.AddRange(customer1, customer2);
            _context.LoanApplications.AddRange(app1, app2);
            await _context.SaveChangesAsync();

            var result = await _repo.GetByApplicationNumberWithDetailsAsync("LA-BBBB2222");

            Assert.IsNotNull(result);
            Assert.AreEqual("LA-BBBB2222", result.ApplicationNumber);
            Assert.AreEqual(customer2.Id, result.CustomerId);
        }

        [TestMethod]
        public async Task GetByCustomerIdWithDetailsAsync_WhenApplicationsExist_ReturnsOnlyThatCustomersApplicationsWithLoanType()
        {
            var loanType = CreateLoanType("Education Loan");

            var customerId = Guid.NewGuid();
            var otherCustomerId = Guid.NewGuid();

            var app1 = CreateLoanApplication("LA-11111111", customerId, loanType.Id);
            var app2 = CreateLoanApplication("LA-22222222", otherCustomerId, loanType.Id);

            _context.LoanTypes.Add(loanType);
            _context.LoanApplications.AddRange(app1, app2);
            await _context.SaveChangesAsync();

            var result = (await _repo.GetByCustomerIdWithDetailsAsync(customerId)).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("LA-11111111", result[0].ApplicationNumber);
            Assert.IsNotNull(result[0].LoanType);
            Assert.AreEqual("Education Loan", result[0].LoanType.Name);
        }

        [TestMethod]
        public async Task GetByCustomerIdWithDetailsAsync_WhenNoApplications_ReturnsEmpty()
        {
            var result = await _repo.GetByCustomerIdWithDetailsAsync(Guid.NewGuid());
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetByCustomerIdWithDetailsAsync_WhenMultipleForSameCustomer_ReturnsAllForThatCustomer()
        {
            var loanType = CreateLoanType("Home Loan");

            var customerId = Guid.NewGuid();

            var app1 = CreateLoanApplication("LA-33333333", customerId, loanType.Id);
            var app2 = CreateLoanApplication("LA-44444444", customerId, loanType.Id);

            _context.LoanTypes.Add(loanType);
            _context.LoanApplications.AddRange(app1, app2);
            await _context.SaveChangesAsync();

            var result = (await _repo.GetByCustomerIdWithDetailsAsync(customerId)).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(a => a.ApplicationNumber == "LA-33333333"));
            Assert.IsTrue(result.Any(a => a.ApplicationNumber == "LA-44444444"));

            Assert.IsTrue(result.All(a => a.LoanType != null));
        }
    }
}
