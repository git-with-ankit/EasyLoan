using EasyLoan.Dtos.LoanPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Interfaces
{
    public interface ILoanPaymentService
    {
        Task MakePaymentAsync(Guid customerId, MakeLoanPaymentRequestDto dto);
        Task<List<LoanPaymentHistoryResponseDto>> GetPaymentHistoryAsync(Guid customerId, LoanPaymentHistoryRequestDto dto);
        Task<NextEmiPaymentResponseDto> GetNextPaymentAsync(Guid customerId, Guid loanId);
    }
}
