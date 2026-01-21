using EasyLoan.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.Interfaces
{
    public interface ILoanPaymentRepository : IRepository<LoanPayment>
    {
        Task<IEnumerable<LoanPayment>> GetByLoanIdAsync(Guid loanId);
    }
}
