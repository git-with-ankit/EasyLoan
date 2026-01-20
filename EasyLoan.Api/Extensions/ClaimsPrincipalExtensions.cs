using EasyLoan.Business.Constants;
using EasyLoan.Business.Enums;
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

        public static Role GetRole(this ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrWhiteSpace(roleClaim))
            {
                throw new AuthenticationFailedException(ErrorMessages.InvalidToken);
            }

            if (!Enum.TryParse<Role>(roleClaim, ignoreCase: true, out var role))
            {
                throw new AuthenticationFailedException(ErrorMessages.InvalidToken);
            }

            return role;
        }
    }
}
