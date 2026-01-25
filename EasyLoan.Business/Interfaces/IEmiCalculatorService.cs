using EasyLoan.Dtos.LoanType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Interfaces
{
    public interface IEmiCalculatorService
    {
        IEnumerable<EmiScheduleItemResponseDto> GenerateSchedule(decimal principal,decimal annualInterestRate,int tenureInMonths,DateTime startDate);
    }
}
