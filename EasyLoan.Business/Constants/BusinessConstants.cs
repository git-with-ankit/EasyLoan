using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Constants
{
    public static class BusinessConstants
    {
        public const decimal MaximumLoanAmount = 1000000m;
        public const int MinimumDaysRequiredForAnotherLoan = 15;
        public const int MaximumTenureInMonthsAllowed = 480;
    }
}
