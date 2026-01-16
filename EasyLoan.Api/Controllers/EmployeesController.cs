using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.Employee;
using EasyLoan.Dtos.LoanApplication;
using EasyLoan.Dtos.LoanType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService service)
        {
            _employeeService = service;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponseDto<string>>> Login(EmployeeLoginRequestDto request)
        {
            var token = await _employeeService.LoginAsync(request);
            return Ok(new ApiResponseDto<string> { Success = true, Data = token });
        }

        ////[Authorize(Roles = "Manager")]
        //[HttpGet("assigned-loan-applications/{managerId}")]
        //public async Task<ActionResult<ApiResponseDto<List<AssignedLoanApplicationsResponseDto>>>>GetAssignedLoanApplications(Guid managerId)
        //{
        //    //var apps = await _employeeService.GetAssignedApplicationsAsync(User.GetUserId());
        //    var applications = await _employeeService.GetAssignedApplicationsAsync(managerId);
        //    return Ok(new ApiResponseDto<List<AssignedLoanApplicationsResponseDto>> { Success = true, Data = applications });
        //}

        //[Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<Guid>>> CreateManager(CreateEmployeeRequestDto request)
        {
            var id = await _employeeService.CreateManagerAsync(request);
            return Ok(new ApiResponseDto<Guid> { Success = true, Data = id });
        }

        //[Authorize(Roles ="Admin")]
        //[HttpPost("loan-types")]
        //public async Task<ActionResult<ApiResponseDto<Guid>>> CreateLoanType(
        //    CreateLoanTypeRequestDto request)
        //{
        //    var id = await _employeeService.CreateLoanTypeAsync(request);
        //    return Ok(new ApiResponseDto<Guid> { Success = true, Data = id });
        //}

        //[HttpPut("loan-types/{loanTypeId}")]//TODO : How to implement patch
        //public async Task<ActionResult<ApiResponseDto<bool>>> UpdateLoanType(
        //    Guid loanTypeId, UpdateLoanTypeRequestDto request)
        //{
        //    await _employeeService.UpdateLoanTypeAsync(loanTypeId, request);
        //    return Ok(new ApiResponseDto<bool> { Success = true, Data = true });
        //}

        //[HttpGet("loan-types")]
        //public async Task<ActionResult<ApiResponseDto<List<LoanTypeResponseDto>>>> GetLoanTypes()
        //{
        //    var types = await _employeeService.GetLoanTypesAsync();
        //    return Ok(new ApiResponseDto<List<LoanTypeResponseDto>> { Success = true, Data = types });
        //}

        //[HttpGet("applications")]
        //public async Task<ActionResult<ApiResponseDto<List<LoanApplicationListItemResponseDto>>>> Applications()
        //{
        //    var applications = await _employeeService.GetPendingApplications();
        //    return Ok(new ApiResponseDto<List<LoanApplicationListItemResponseDto>> { Success = true, Data = applications });
        //}
    }
}
