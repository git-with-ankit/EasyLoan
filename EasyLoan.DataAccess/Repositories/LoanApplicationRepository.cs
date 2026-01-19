using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.DataAccess.Repositories
{
    public class LoanApplicationRepository : ILoanApplicationRepository
    {
        private readonly EasyLoanDbContext _context;

        public LoanApplicationRepository(EasyLoanDbContext context)
        {
            _context = context;
        }

        public async Task<LoanApplication?> GetByIdAsync(Guid id)
        {
            return await _context.LoanApplications
                .Include(a => a.Customer)
                .Include(a => a.LoanType)
                .Include(a => a.ApprovedByEmployee)
                .Include(a => a.LoanDetails)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<LoanApplication>> GetAllAsync()
        {
            return await _context.LoanApplications.Include(a => a.LoanType)
                    .Include(a => a.Customer).Include(a => a.ApprovedByEmployee).Include(a => a.LoanDetails).ToListAsync();
        }

        public async Task<LoanApplication?> GetByApplicationNumberAsync(string applicationNumber)
        {
            return await _context.LoanApplications.Include(a => a.LoanType)
                    .Include(a => a.Customer).Include(a => a.ApprovedByEmployee).Include(a => a.LoanDetails)
                .FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);
        }

        public async Task<List<LoanApplication>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.LoanApplications
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.LoanType)
                .Include(a => a.LoanDetails)
                .ToListAsync();
        }

        public async Task AddAsync(LoanApplication application)
        {
            await _context.LoanApplications.AddAsync(application);
        }

        public Task UpdateAsync(LoanApplication application)
        {
            _context.LoanApplications.Update(application);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
