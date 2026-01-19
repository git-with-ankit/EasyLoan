using EasyLoan.Dtos.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Interfaces
{
    public interface ILoanService
    {
        Task<List<LoanSummaryResponseDto>> GetAllCustomerLoansAsync(Guid customerId);
        Task<LoanDetailsResponseDto> GetLoanDetailsAsync(Guid customerId, string loanNumber);
    }
}
