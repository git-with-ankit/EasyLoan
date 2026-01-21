using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using EasyLoan.Models.Common.Enums;

namespace EasyLoan.DataAccess.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(EasyLoanDbContext context) : base(context) { }

        //public async Task<Employee?> GetByIdAsync(Guid id)
        //{
        //    return await _context.Employees
        //        .FirstOrDefaultAsync(e => e.Id == id);
        //}

        public async Task<IEnumerable<Employee>> GetAllWithDetailsAsync()
        {
            return await _context.Employees.Include(e => e.AssignedLoanApplications).ToListAsync();
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<IEnumerable<Employee>> GetManagersAsync()
        {
            return await _context.Employees
                .Where(e => e.Role == EmployeeRole.Manager)
                .ToListAsync();
        }

        //public Task UpdateAsync(Employee employee)
        //{
        //    _context.Employees.Update(employee);
        //    return Task.CompletedTask;
        //}

        //public async Task AddAsync(Employee employee)
        //{
        //    await _context.Employees.AddAsync(employee);
        //}

        //public async Task SaveChangesAsync()
        //{
        //    await _context.SaveChangesAsync();
        //}
    }
}
