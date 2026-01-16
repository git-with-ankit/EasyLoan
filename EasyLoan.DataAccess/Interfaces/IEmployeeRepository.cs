using EasyLoan.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.Interfaces
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<Employee?> GetByEmailAsync(string email);
        Task<List<Employee>> GetManagersAsync();
    }
}
