using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.DataAccess.Repositories
{
    public class LoanDetailsRepository : ILoanDetailsRepository
    {
        private readonly EasyLoanDbContext _context;

        public LoanDetailsRepository(EasyLoanDbContext context)
        {
            _context = context;
        }

        public async Task<LoanDetails?> GetByIdAsync(Guid id)
        {
            return await _context.Loans
                     .Include(l => l.LoanApplication)
                     .Include(l => l.LoanType)
                     .Include(l => l.LoanPayments)
                     .Include(l => l.Emis)
                     .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<LoanDetails>> GetAllAsync()
        {
            return await _context.Loans.ToListAsync();
        }

        public async Task<IEnumerable<LoanDetails>> GetLoansByCustomerIdAsync(Guid customerId)
        {
            return await _context.Loans
                .Where(l => l.CustomerId == customerId)
                .Include(l => l.LoanApplication)
                .Include(l => l.LoanType)
                .Include(l => l.Emis)
                .ToListAsync();
        }

        public async Task<LoanDetails?> GetByLoanNumberAsync(string loanNumber)
        {
            return await _context.Loans
                .Include(l => l.LoanApplication)
                .Include(l => l.LoanType)
                .Include(l => l.Emis)
                .FirstOrDefaultAsync(l => l.LoanNumber == loanNumber);
        }

        public async Task AddAsync(LoanDetails loan)
        {
            await _context.Loans.AddAsync(loan);
        }

        public Task UpdateAsync(LoanDetails loan)
        {
            _context.Loans.Update(loan);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
