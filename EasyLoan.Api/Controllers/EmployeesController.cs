using EasyLoan.Api.Extensions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.Customer;
using EasyLoan.Dtos.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IAuthService _authService;

        public EmployeesController(IEmployeeService service , IAuthService authService)
        {
            _employeeService = service;
            _authService = authService;
        }

        [Authorize(Roles = "Manager,Admin")]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponseDto<CustomerProfileResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<EmployeeProfileResponseDto>>> GetProfile()
        {
            var employeeId = User.GetUserId();
            var profile = await _employeeService.GetProfileAsync(employeeId);
            return Ok(new ApiResponseDto<EmployeeProfileResponseDto> { Success = true, Data = profile });
        }

        [Authorize(Roles = "Manager,Admin")]
        [HttpPatch("profile")]
        [ProducesResponseType(typeof(ApiResponseDto<EmployeeProfileResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<EmployeeProfileResponseDto>>> UpdateProfile(UpdateEmployeeProfileRequestDto updateProfile)
        {
            var employeeId = User.GetUserId();
            var employeeProfile = await _employeeService.UpdateProfileAsync(employeeId, updateProfile);
            return Ok(new ApiResponseDto<EmployeeProfileResponseDto> { Success = true, Data = employeeProfile });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto<string>>> Login(EmployeeLoginRequestDto request)
        {
            var token = await _authService.LoginEmployeeAsync(request);
            return Ok(new ApiResponseDto<string> { Success = true, Data = token });
        }

        ////[Authorize(Roles ="Admin")]
        //[HttpPost]
        //[ProducesResponseType(typeof(ApiResponseDto<Guid>), StatusCodes.Status201Created)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        //public async Task<ActionResult<ApiResponseDto<Guid>>> CreateManager(CreateEmployeeRequestDto request)
        //{
        //    var id = await _employeeService.CreateManagerAsync(request);
        //    return Ok(new ApiResponseDto<Guid> { Success = true, Data = id });
        //}

        [Authorize(Roles ="Admin")]
        [HttpPost("manager/register")]
        [ProducesResponseType(typeof(ApiResponseDto<RegisterManagerResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<RegisterManagerResponseDto>>> CreateManager(CreateManagerRequestDto request)
        {
            var manager = await _authService.RegisterManagerAsync(request);
            return CreatedAtAction(nameof(GetProfile), new { }, new ApiResponseDto<RegisterManagerResponseDto> { Success = true, Data = manager });
        }
    }
}
