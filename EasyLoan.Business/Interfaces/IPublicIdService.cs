using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Interfaces
{
    public interface IPublicIdService
    {
        string GenerateLoanNumber();
        string GenerateApplicationNumber();
    }
}
