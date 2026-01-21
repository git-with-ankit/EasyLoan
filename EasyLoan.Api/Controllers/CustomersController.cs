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

        //[HttpPost("register")]
        //[ProducesResponseType(typeof(ApiResponseDto<Guid>), StatusCodes.Status201Created)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)] // DTO validation
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)] // Email/PAN exists
        //public async Task<ActionResult<ApiResponseDto<Guid>>> Register(
        //    RegisterCustomerRequestDto request)
        //{
        //    var id = await _customerservice.RegisterAsync(request);
        //    return CreatedAtAction(nameof(GetProfile), new { id }, new ApiResponseDto<Guid> { Success = true, Data = id });
        //}

        //[HttpPost("login")]
        //[ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
        //public async Task<ActionResult<ApiResponseDto<string>>> Login(
        //    CustomerLoginRequestDto request)
        //{
        //    var token = await _customerservice.LoginAsync(request);
        //    return Ok(new ApiResponseDto<string> { Success = true, Data = token });//TODO : Return status code 201
        //}

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

        //[Authorize(Roles = "Customer")]
        //[HttpGet("portfolio")]
        //[ProducesResponseType(typeof(ApiResponseDto<CustomerDashboardResponseDto>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        //public async Task<ActionResult<ApiResponseDto<CustomerDashboardResponseDto>>> GeFinancialOverview()//TODO : Work on this
        //{
        //    var customerId = User.GetUserId();
        //    var summary = await _customerservice.GeFinancialOverviewAsync(customerId);
        //    return Ok(new ApiResponseDto<CustomerDashboardResponseDto> { Success = true, Data = summary });
        //}

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
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> Login(
           CustomerLoginRequestDto request)
        {
            var token = await _authService.LoginCustomerAsync(request);
            return Ok(token);
        }
    }
}
