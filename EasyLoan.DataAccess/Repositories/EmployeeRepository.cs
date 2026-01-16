using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyLoan.DataAccess.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EasyLoanDbContext _context;

        public EmployeeRepository(EasyLoanDbContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByIdAsync(Guid id)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<List<Employee>> GetManagersAsync()
        {
            return await _context.Employees
                .Where(e => e.Role == EmployeeRole.Manager)
                .ToListAsync();
        }

        public async Task AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
