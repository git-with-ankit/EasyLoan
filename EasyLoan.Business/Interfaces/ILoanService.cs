using EasyLoan.Dtos.Loan;
using EasyLoan.Models.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Interfaces
{
    public interface ILoanService
    {
        Task<IEnumerable<LoanSummaryResponseDto>> GetCustomerLoansAsync(Guid customerId, LoanStatus status);
        Task<LoanDetailsResponseDto> GetLoanDetailsAsync(Guid customerId, string loanNumber);
    }
}
