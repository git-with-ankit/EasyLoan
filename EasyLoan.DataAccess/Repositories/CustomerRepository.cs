using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.DataAccess.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(EasyLoanDbContext context) : base(context) { }

        //public async Task<Customer?> GetByIdWithDetailsAsync(Guid id)
        //{
        //    return await _context.Customers
        //        .Include(c => c.Loans)
        //        .ThenInclude(l => l.LoanPayments)
        //        .Include(c => c.LoanApplications)
        //        .FirstOrDefaultAsync(c => c.Id == id);
        //}

        //public async Task<IEnumerable<Customer>> GetAllAsync()
        //{
        //    return await _context.Customers.ToListAsync();
        //}

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Customers
                .AnyAsync(c => c.Email == email);
        }

        public async Task<bool> ExistsByPanAsync(string panNumber)
        {
            return await _context.Customers
                .AnyAsync(c => c.PanNumber == panNumber);
        }

        //public async Task AddAsync(Customer customer)
        //{
        //    await _context.Customers.AddAsync(customer);
        //}

        //public Task UpdateAsync(Customer customer)
        //{
        //    _context.Customers.Update(customer);
        //    return Task.CompletedTask;
        //}

        //public async Task SaveChangesAsync()
        //{
        //    await _context.SaveChangesAsync();
        //}
    }
}
