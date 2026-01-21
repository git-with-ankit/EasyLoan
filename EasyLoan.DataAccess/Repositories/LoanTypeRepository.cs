using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.DataAccess.Repositories
{
    public class LoanTypeRepository : GenericRepository<LoanType>, ILoanTypeRepository
    {
        public LoanTypeRepository(EasyLoanDbContext context) : base(context) { }

        //public async Task<LoanType?> GetByIdAsync(Guid id)
        //{
        //    return await _context.LoanTypes
        //        .FirstOrDefaultAsync(l => l.Id == id);
        //}

        //public async Task<IEnumerable<LoanType>> GetAllAsync()
        //{
        //    return await _context.LoanTypes.ToListAsync();
        //}

        //public async Task AddAsync(LoanType loanType)
        //{
        //    await _context.LoanTypes.AddAsync(loanType);
        //}

        //public Task UpdateAsync(LoanType loanType)
        //{
        //    _context.LoanTypes.Update(loanType);
        //    return Task.CompletedTask;
        //}

        //public async Task SaveChangesAsync()
        //{
        //    await _context.SaveChangesAsync();
        //}
    }
}
