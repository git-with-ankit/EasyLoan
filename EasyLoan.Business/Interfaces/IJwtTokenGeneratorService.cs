using EasyLoan.Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Interfaces
{
    public interface IJwtTokenGeneratorService
    {
        string GenerateToken(Guid userId, Role role);
    }
}
