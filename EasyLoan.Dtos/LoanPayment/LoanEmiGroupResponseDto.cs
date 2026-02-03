using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.LoanPayment
{
    public class LoanEmiGroupResponseDto
    {
        public string LoanNumber { get; set; } = string.Empty;
        public IEnumerable<DueEmisResponseDto> Emis { get; set; } = new List<DueEmisResponseDto>();
    }
}
