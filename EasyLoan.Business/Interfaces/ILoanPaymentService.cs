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
        Task MakePaymentAsync(Guid customerId,string loanNumber, MakeLoanPaymentRequestDto dto);
        Task<List<LoanPaymentHistoryResponseDto>> GetPaymentsHistoryAsync(Guid customerId, string loanNumber);
        Task<List<DueEmisResponseDto>> GetDueEmisAsync(Guid customerId, string loanNumber, EmiDueStatus status);
        Task<List<List<DueEmisResponseDto>>> GetAllDueEmisAsync(Guid customerId, EmiDueStatus status);
    }
}
