using EasyLoan.Dtos.LoanPayment;
using EasyLoan.Models.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Interfaces
{
    public interface ILoanPaymentService
    {
        Task<LoanPaymentResponseDto> MakePaymentAsync(Guid customerId,string loanNumber, MakeLoanPaymentRequestDto dto);
        Task<IEnumerable<LoanPaymentHistoryResponseDto>> GetPaymentsHistoryAsync(Guid customerId, string loanNumber);
        Task<IEnumerable<DueEmisResponseDto>> GetDueEmisAsync(Guid customerId, string loanNumber, EmiDueStatus status);
        Task<IEnumerable<LoanEmiGroupResponseDto>> GetAllDueEmisAsync(Guid customerId, EmiDueStatus status);
    }
}
