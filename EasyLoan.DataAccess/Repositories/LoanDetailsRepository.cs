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
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<List<LoanDetails>> GetAllAsync()
        {
            return await _context.Loans.ToListAsync();
        }

        public async Task<List<LoanDetails>> GetLoansByCustomerIdAsync(Guid customerId)
        {
            return await _context.Loans
                .Where(l => l.CustomerId == customerId)
                .ToListAsync();
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
