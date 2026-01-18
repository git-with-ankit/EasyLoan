using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using System.Security.Claims;

namespace EasyLoan.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new AuthenticationFailedException(ErrorMessages.InvalidToken);
            }

            return userId;
        }
    }
}
