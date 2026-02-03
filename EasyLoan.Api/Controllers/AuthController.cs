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
