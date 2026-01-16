using EasyLoan.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByPanAsync(string panNumber);
        Task UpdateAsync(Customer customer);
    }
}
