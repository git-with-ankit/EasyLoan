using EasyLoan.Business.Interfaces;
using EasyLoan.Business.Services;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.Customer;
using EasyLoan.Dtos.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Authorize(Roles ="Admin")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) 
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register/customer")]
        [ProducesResponseType(typeof(ApiResponseDto<Guid>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)] 
        public async Task<ActionResult<ApiResponseDto<Guid>>> Register(
            RegisterCustomerRequestDto request)
        {
            var id = await _authService.RegisterCustomerAsync(request);
            return CreatedAtAction(actionName: "GetProfile",controllerName: "Customers" ,routeValues: new { },
                new ApiResponseDto<Guid> { Success = true, Data = id });
        }

        [AllowAnonymous]
        [HttpPost("login/customer")]
        [ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<string>>> Login(
            CustomerLoginRequestDto request)
        {
            var token = await _authService.LoginCustomerAsync(request);
            return Ok(new ApiResponseDto<string> { Success = true, Data = token });
        }

        [AllowAnonymous]
        [HttpPost("login/employee")]
        [ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<string>>> Login(EmployeeLoginRequestDto request)
        {
            var token = await _authService.LoginEmployeeAsync(request);
            return Ok(new ApiResponseDto<string> { Success = true, Data = token });
        }

        [HttpPost("register/manager")]
        [ProducesResponseType(typeof(ApiResponseDto<Guid>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<Guid>>> CreateManager(CreateEmployeeRequestDto request)
        {
            var id = await _authService.RegisterManagerAsync(request);
            //return CreatedAtAction(nameof(EmployeesController), new { }, new ApiResponseDto<Guid> { Success = true, Data = id });//EmployeeController.GetProfile
            return Ok(new ApiResponseDto<Guid> { Success = true, Data = id });
        }
    }
}
