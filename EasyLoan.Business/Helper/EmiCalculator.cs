using EasyLoan.Dtos.LoanType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Helper
{
    public static class EmiCalculator
    {
        public static List<EmiScheduleItemResponseDto> GenerateSchedule(
            decimal principal,
            decimal annualInterestRate,
            int tenureInMonths,
            DateTime startDate)
        {
            var monthlyRate = annualInterestRate / 12 / 100;

            var emi = (principal * monthlyRate *
                      (decimal)Math.Pow(1 + (double)monthlyRate, tenureInMonths)) /
                      ((decimal)Math.Pow(1 + (double)monthlyRate, tenureInMonths) - 1);

            var remaining = principal;
            var schedule = new List<EmiScheduleItemResponseDto>();

            for (int i = 1; i <= tenureInMonths; i++)
            {
                var interest = remaining * monthlyRate;
                var principalComponent = emi - interest;

                if (principalComponent > remaining)
                    principalComponent = remaining;

                remaining -= principalComponent;

                schedule.Add(new EmiScheduleItemResponseDto
                {
                    EmiNumber = i,
                    DueDate = startDate.AddMonths(i),
                    InterestComponent = Math.Round(interest, 2),
                    PrincipalComponent = Math.Round(principalComponent, 2),
                    TotalEmiAmount = Math.Round(emi, 2),
                    PrincipalRemainingAfterPayment = Math.Round(remaining, 2)
                });
            }

            return schedule;
        }
    }

}
