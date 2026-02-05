//using EasyLoan.Business.Interfaces;
//using EasyLoan.Dtos.Common;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace EasyLoan.Api.Controllers
//{
//    [ApiController]
//    [Route("api/auth")]
//    public class AuthController : Controller
//    {
//        private readonly ICustomerService _customerservice;
//        private readonly IAuthService _authService;

//        public AuthController(ICustomerService customerService, IAuthService authService)
//        {
//            _authService = authService;
//        }

//        [AllowAnonymous]
//        [HttpPost("register")]
//        [ProducesResponseType(typeof(CustomerProfileResponseDto), StatusCodes.Status201Created)]
//        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
//        public async Task<ActionResult<CustomerProfileResponseDto>> Register(
//            RegisterCustomerRequestDto request)
//        {
//            var customer = await _authService.RegisterCustomerAsync(request);
//            return CreatedAtAction(nameof(GetProfile), new { }, customer);
//        }

//        [AllowAnonymous]
//        [HttpPost("login")]
//        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
//        public async Task<ActionResult<string>> Login(
//           LoginRequestDto request)
//        {
//            var token = await _authService.LoginCustomerAsync(request);
//            return Ok(token);
//        }
//    }
//}

using EasyLoan.Api.Extensions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthController(IConfiguration configuration, IAuthService authService)
        {
            _configuration = configuration;
            _authService = authService;
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(MeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public ActionResult<MeResponseDto> GetMe()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.GetRole();

            return Ok(new MeResponseDto
            {
                Email = email!,
                Role = role!
            });
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public ActionResult Logout()
        {
            Response.Cookies.Delete("easyloan_auth");
            return NoContent();
        }

        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ChangePassword( ChangePasswordRequestDto request)
        {
            var userId = User.GetUserId();
            var role = User.GetRole();

            await _authService.ChangePasswordAsync(userId, role, request);

            return NoContent();
        }
    }
}
