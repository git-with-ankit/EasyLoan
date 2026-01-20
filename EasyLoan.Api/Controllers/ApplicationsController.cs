using EasyLoan.Api.Extensions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.LoanApplication;
using EasyLoan.Models.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/loan-applications")]
    public class LoanApplicationsController : ControllerBase
    {
        private readonly ILoanApplicationService _service;

        public LoanApplicationsController(ILoanApplicationService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<string>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]   
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]   
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)] 
        public async Task<ActionResult<ApiResponseDto<string>>> Create(CreateLoanApplicationRequestDto request)
        {
            var customerId = User.GetUserId();
            var applicationNumber = await _service.CreateAsync(customerId, request);
            return CreatedAtAction(nameof(GetByApplicationNumber), new { applicationNumber }, new ApiResponseDto<string> { Success = true, Data = applicationNumber });
        }

        //[Authorize(Roles = "Customer")]
        //[HttpGet]
        //[ProducesResponseType(typeof(ApiResponseDto<List<LoanApplicationsResponseDto>>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)] // Customer not found
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)] // Not owner
        //public async Task<ActionResult<ApiResponseDto<List<LoanApplicationsResponseDto>>>> GetCustomerApplications()
        //{
        //    var customerId = User.GetUserId();
        //    var applications = await _service.GetCustomerApplicationsAsync(customerId);
        //    return Ok(new ApiResponseDto<List<LoanApplicationsResponseDto>> { Success = true, Data = applications });
        //}

        //[Authorize(Roles = "Manager")]
        //[HttpGet("assigned")]
        //[ProducesResponseType(typeof(ApiResponseDto<List<LoanApplicationsResponseDto>>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)] 
        //public async Task<ActionResult<ApiResponseDto<List<LoanApplicationsResponseDto>>>> GetAssignedApplications()
        //{
        //    var managerId = User.GetUserId();
        //    var assignedApplications = await _service.GetAssignedApplicationsAsync(managerId);
        //    return Ok(new ApiResponseDto<List<LoanApplicationsResponseDto>> { Success = true, Data = assignedApplications });
        //}
        //[Authorize(Roles = "Admin")]
        //[HttpGet("pending")]
        //[ProducesResponseType(typeof(ApiResponseDto<List<LoanApplicationsResponseDto>>), StatusCodes.Status200OK)]
        //public async Task<ActionResult<ApiResponseDto<List<LoanApplicationsAdminResponseDto>>>> GetAllPendingApplications()
        //{
        //    var assignedApplications = await _service.GetAllPendingApplicationsAsync();
        //    return Ok(new ApiResponseDto<List<LoanApplicationsAdminResponseDto>> { Success = true, Data = assignedApplications });
        //}
        [Authorize(Roles ="Admin,Manager,Customer")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<LoanApplicationsAdminResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<LoanApplicationsResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)] // Customer not found
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)] // Not owner
        public async Task<ActionResult<ApiResponseDto<IEnumerable<object>>>> GetApplications([FromQuery] LoanApplicationStatus status)
        {
            var userId = User.GetUserId();
            var userRole = User.GetRole();

            var applications = await _service.GetApplicationsAsync(userId, userRole, status);

            return Ok(new ApiResponseDto<IEnumerable<object>> { Success = true, Data = applications });

        }


        [Authorize(Roles= "Customer")]
        [HttpGet("{applicationNumber}")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanApplicationDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)] // App not found
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)] // Not owner
        public async Task<ActionResult<ApiResponseDto<LoanApplicationDetailsResponseDto>>> GetByApplicationNumber(string applicationNumber)
        {
            var details = await _service.GetByApplicationNumberAsync(applicationNumber);
            return Ok(new ApiResponseDto<LoanApplicationDetailsResponseDto> { Success = true, Data = details });
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("{applicationNumber}/review")]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)] // App not found
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)] // Not assigned manager
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)] // Invalid approval
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateReview(string applicationNumber,ReviewLoanApplicationRequestDto request )
        {
            var managerId = User.GetUserId();
            await _service.UpdateReviewAsync(applicationNumber, managerId, request);
            return Ok(new ApiResponseDto<bool> { Success = true, Data = true });
        }
        [Authorize(Roles = "Manager")]
        [HttpGet("{applicationNumber}/review")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanApplicationDetailsWithCustomerDataResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<LoanApplicationDetailsWithCustomerDataResponseDto>>> GetApplicationDetailsForReview(string applicationNumber )
        {
            var managerId = User.GetUserId();
            var applicationDetails = await _service.GetApplicationDetailsForReview(applicationNumber, managerId);
            return Ok(new ApiResponseDto<LoanApplicationDetailsWithCustomerDataResponseDto> { Success = true, Data = applicationDetails });
        }
    }

}
