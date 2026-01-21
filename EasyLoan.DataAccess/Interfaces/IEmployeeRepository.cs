using EasyLoan.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<IEnumerable<Employee>> GetAllWithDetailsAsync();
        Task<Employee?> GetByEmailAsync(string email);
        Task<IEnumerable<Employee>> GetManagersAsync();
    }
}
