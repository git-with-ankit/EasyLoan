using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.DataAccess.Repositories
{
    public class LoanPaymentRepository : ILoanPaymentRepository
    {
        private readonly EasyLoanDbContext _context;

        public LoanPaymentRepository(EasyLoanDbContext context)
        {
            _context = context;
        }

        public async Task<LoanPayment?> GetByIdAsync(Guid id)
        {
            return await _context.LoanPayments
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<LoanPayment>> GetAllAsync()
        {
            return await _context.LoanPayments.ToListAsync();
        }

        public async Task<List<LoanPayment>> GetByLoanIdAsync(Guid loanId)
        {
            return await _context.LoanPayments
                .Where(p => p.LoanDetailsId == loanId)
                .ToListAsync();
        }

        public async Task AddAsync(LoanPayment payment)
        {
            await _context.LoanPayments.AddAsync(payment);
        }

        public Task UpdateAsync(LoanPayment payment)
        {
            _context.LoanPayments.Update(payment);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
