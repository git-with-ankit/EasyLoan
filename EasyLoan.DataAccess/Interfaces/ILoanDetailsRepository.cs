using EasyLoan.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.Interfaces
{
    public interface ILoanDetailsRepository : IRepository<LoanDetails>
    {
        Task<List<LoanDetails>> GetLoansByCustomerIdAsync(Guid customerId);
        Task<LoanDetails?> GetByLoanNumberAsync(string loanNumber);
    }
}
