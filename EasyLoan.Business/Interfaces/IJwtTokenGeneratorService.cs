using EasyLoan.Models.Common.Enums;

namespace EasyLoan.Business.Interfaces
{
    public interface IJwtTokenGeneratorService
    {
        string GenerateToken(Guid userId, string email, Role role);
    }
}
