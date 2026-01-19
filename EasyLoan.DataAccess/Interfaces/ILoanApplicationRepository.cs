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
        Task<LoanApplication?> GetByApplicationNumberAsync(string applicationNumber);
        Task<List<LoanApplication>> GetByCustomerIdAsync(Guid customerId);
    }
}
