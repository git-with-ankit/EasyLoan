using EasyLoan.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.Interfaces
{
    public interface ILoanApplicationRepository : IRepository<LoanApplication>
    {
        Task<LoanApplication?> GetByApplicationIdAsync(string applicationId);
        Task<List<LoanApplication>> GetByCustomerIdAsync(Guid customerId);
        Task UpdateAsync(LoanApplication application);
    }
}
