using EasyLoan.Api.Extensions;
using EasyLoan.Business.Interfaces;
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
        [ProducesResponseType(typeof(CustomerProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<EmployeeProfileResponseDto>> GetProfile()
        {
            var employeeId = User.GetUserId();
            var profile = await _employeeService.GetProfileAsync(employeeId);
            return Ok(profile);
        }

        [Authorize(Roles = "Manager,Admin")]
        [HttpPatch("profile")]
        [ProducesResponseType(typeof(EmployeeProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<EmployeeProfileResponseDto>> UpdateProfile(UpdateEmployeeProfileRequestDto updateProfile)
        {
            var employeeId = User.GetUserId();
            var employeeProfile = await _employeeService.UpdateProfileAsync(employeeId, updateProfile);
            return Ok(employeeProfile);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> Login(EmployeeLoginRequestDto request)
        {
            var token = await _authService.LoginEmployeeAsync(request);
            return Ok(token);
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
        [ProducesResponseType(typeof(RegisterManagerResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegisterManagerResponseDto>> CreateManager(CreateManagerRequestDto request)
        {
            var manager = await _authService.RegisterManagerAsync(request);
            return CreatedAtAction(nameof(GetProfile), new { }, manager);
        }
    }
}
