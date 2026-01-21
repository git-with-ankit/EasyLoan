using EasyLoan.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.Interfaces
{
    public interface ILoanDetailsRepository : IGenericRepository<LoanDetails>
    {
        Task<IEnumerable<LoanDetails>> GetLoansByCustomerIdWithDetailsAsync(Guid customerId);
        Task<LoanDetails?> GetByLoanNumberWithDetailsAsync(string loanNumber);
    }
}
