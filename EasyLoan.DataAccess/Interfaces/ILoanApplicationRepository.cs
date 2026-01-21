using EasyLoan.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.Interfaces
{
    public interface ILoanApplicationRepository : IGenericRepository<LoanApplication>
    {
        Task<IEnumerable<LoanApplication>> GetAllWithDetailsAsync();
        Task<LoanApplication?> GetByApplicationNumberWithDetailsAsync(string applicationNumber);
        Task<IEnumerable<LoanApplication>> GetByCustomerIdWithDetailsAsync(Guid customerId);
    }
}
