using EasyLoan.Api.Extensions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerservice;
        private readonly IAuthService _authService;

        public CustomersController(ICustomerService customerService, IAuthService authService)
        {
            _customerservice = customerService;
            _authService = authService;
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(CustomerProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CustomerProfileResponseDto>> GetProfile()
        {
            var customerId = User.GetUserId();
            var profile = await _customerservice.GetProfileAsync(customerId);
            return Ok(profile);
        }

        [Authorize(Roles = "Customer")]
        [HttpPatch("profile")]
        [ProducesResponseType(typeof(CustomerProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CustomerProfileResponseDto>> UpdateProfile(UpdateCustomerProfileRequestDto updateProfile)
        {
            var customerId = User.GetUserId();
            var customerProfile = await _customerservice.UpdateProfileAsync(customerId, updateProfile);
            return Ok(customerProfile);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(CustomerProfileResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CustomerProfileResponseDto>> Register(
            RegisterCustomerRequestDto request)
        {
            var customer = await _authService.RegisterCustomerAsync(request);
            return CreatedAtAction(nameof(GetProfile),  new { }, customer);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Login(
           CustomerLoginRequestDto request)
        {
            var token = await _authService.LoginCustomerAsync(request);
            
            // Set HTTP-only cookie
            Response.Cookies.Append("easyloan_auth", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(
                    int.Parse(HttpContext.RequestServices
                        .GetRequiredService<IConfiguration>()["Jwt:AccessTokenExpiryMinutes"]!))
            });
            
            return NoContent();
        }
    }
}
