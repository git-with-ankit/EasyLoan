using EasyLoan.Api.Extensions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Business.Services;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.Customer;
using EasyLoan.Dtos.Employee;
using EasyLoan.Dtos.LoanApplication;
using EasyLoan.Dtos.LoanType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService service)
        {
            _employeeService = service;
        }

        [Authorize(Roles = "Manager,Admin")]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponseDto<CustomerProfileResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<EmployeeProfileResponseDto>>> GetProfile()
        {
            var employeeId = User.GetUserId();
            var profile = await _employeeService.GetProfileAsync(employeeId);
            return Ok(new ApiResponseDto<EmployeeProfileResponseDto> { Success = true, Data = profile });
        }

        [Authorize(Roles = "Manager,Admin")]
        [HttpPatch("profile")]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateProfile(UpdateEmployeeProfileRequestDto updateProfile)
        {
            var employeeId = User.GetUserId();
            await _employeeService.UpdateProfileAsync(employeeId, updateProfile);
            return Ok(new ApiResponseDto<bool> { Success = true, Data = true });
        }

        //[HttpPost("login")]
        //[ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
        //public async Task<ActionResult<ApiResponseDto<string>>> Login(EmployeeLoginRequestDto request)
        //{
        //    var token = await _employeeService.LoginAsync(request);
        //    return Ok(new ApiResponseDto<string> { Success = true, Data = token });
        //}

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
    }
}
//Implement get profile and update profile here