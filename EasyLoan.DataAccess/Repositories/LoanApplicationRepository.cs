using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.DataAccess.Repositories
{
    public class LoanApplicationRepository : GenericRepository<LoanApplication>, ILoanApplicationRepository
    {
        public LoanApplicationRepository(EasyLoanDbContext context) : base(context) { }

        //public async Task<LoanApplication?> GetByIdAsync(Guid id)
        //{
        //    return await _context.LoanApplications
        //        .Include(a => a.Customer)
        //        .Include(a => a.LoanType)
        //        .Include(a => a.ApprovedByEmployee)
        //        .Include(a => a.LoanDetails)
        //        .FirstOrDefaultAsync(a => a.Id == id);
        //}

        public async Task<IEnumerable<LoanApplication>> GetAllWithDetailsAsync()
        {
            return await _context.LoanApplications.AsNoTracking().Include(a => a.LoanType).ToListAsync();
        }

        public async Task<LoanApplication?> GetByApplicationNumberWithDetailsAsync(string applicationNumber)
        {
            return await _context.LoanApplications.Include(a => a.LoanType).Include(a => a.Customer).FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);
            //.Include(a => a.ApprovedByEmployee).Include(a => a.LoanDetails)
        }

        public async Task<IEnumerable<LoanApplication>> GetByCustomerIdWithDetailsAsync(Guid customerId)
        {
            return await _context.LoanApplications
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.LoanType)
                .ToListAsync();
            //.Include(a => a.LoanDetails)
        }

        //public async Task AddAsync(LoanApplication application)
        //{
        //    await _context.LoanApplications.AddAsync(application);
        //}

        //public Task UpdateAsync(LoanApplication application)
        //{
        //    _context.LoanApplications.Update(application);
        //    return Task.CompletedTask;
        //}

        //public async Task SaveChangesAsync()
        //{
        //    await _context.SaveChangesAsync();
        //}
    }
}
