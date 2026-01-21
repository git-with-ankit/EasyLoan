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
        [ProducesResponseType(typeof(ApiResponseDto<CreatedApplicationResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)]   
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status422UnprocessableEntity)] 
        public async Task<ActionResult<ApiResponseDto<CreatedApplicationResponseDto>>> Create(CreateLoanApplicationRequestDto request)
        {
            var customerId = User.GetUserId();
            var application = await _service.CreateAsync(customerId, request);
            return CreatedAtAction(nameof(GetByApplicationNumber), new { applicationNumber = application.ApplicationNumber }, new ApiResponseDto<CreatedApplicationResponseDto> { Success = true, Data = application });
        }

        //[Authorize(Roles = "Customer")]
        //[HttpGet]
        //[ProducesResponseType(typeof(ApiResponseDto<IEnumerable<LoanApplicationsResponseDto>>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)] // Customer not found
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)] // Not owner
        //public async Task<ActionResult<ApiResponseDto<IEnumerable<LoanApplicationsResponseDto>>>> GetCustomerApplications()
        //{
        //    var customerId = User.GetUserId();
        //    var applications = await _service.GetCustomerApplicationsAsync(customerId);
        //    return Ok(new ApiResponseDto<IEnumerable<LoanApplicationsResponseDto>> { Success = true, Data = applications });
        //}

        //[Authorize(Roles = "Manager")]
        //[HttpGet("assigned")]
        //[ProducesResponseType(typeof(ApiResponseDto<IEnumerable<LoanApplicationsResponseDto>>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)] 
        //public async Task<ActionResult<ApiResponseDto<IEnumerable<LoanApplicationsResponseDto>>>> GetAssignedApplications()
        //{
        //    var managerId = User.GetUserId();
        //    var assignedApplications = await _service.GetAssignedApplicationsAsync(managerId);
        //    return Ok(new ApiResponseDto<IEnumerable<LoanApplicationsResponseDto>> { Success = true, Data = assignedApplications });
        //}
        //[Authorize(Roles = "Admin")]
        //[HttpGet("pending")]
        //[ProducesResponseType(typeof(ApiResponseDto<IEnumerable<LoanApplicationsResponseDto>>), StatusCodes.Status200OK)]
        //public async Task<ActionResult<ApiResponseDto<IEnumerable<LoanApplicationsAdminResponseDto>>>> GetAllPendingApplications()
        //{
        //    var assignedApplications = await _service.GetAllPendingApplicationsAsync();
        //    return Ok(new ApiResponseDto<IEnumerable<LoanApplicationsAdminResponseDto>> { Success = true, Data = assignedApplications });
        //}
        [Authorize(Roles ="Admin,Manager,Customer")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<LoanApplicationsResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)] // Customer not found
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)] // Not owner
        public async Task<ActionResult<ApiResponseDto<IEnumerable<LoanApplicationsResponseDto>>>> GetApplications([FromQuery] LoanApplicationStatus status)
        {
            var userId = User.GetUserId();
            var userRole = User.GetRole();

            var applications = await _service.GetApplicationsAsync(userId, userRole, status);

            return Ok(new ApiResponseDto<IEnumerable<LoanApplicationsResponseDto>> { Success = true, Data = applications });

        }


        [Authorize(Roles= "Customer")]
        [HttpGet("{applicationNumber}")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanApplicationDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)] // App not found
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)] // Not owner
        public async Task<ActionResult<ApiResponseDto<LoanApplicationDetailsResponseDto>>> GetByApplicationNumber(string applicationNumber)
        {
            var details = await _service.GetByApplicationNumberAsync(applicationNumber);
            return Ok(new ApiResponseDto<LoanApplicationDetailsResponseDto> { Success = true, Data = details });
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("{applicationNumber}/review")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanApplicationReviewResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)] 
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status422UnprocessableEntity)] 
        public async Task<ActionResult<ApiResponseDto<LoanApplicationReviewResponseDto>>> UpdateReview(string applicationNumber,ReviewLoanApplicationRequestDto request )
        {
            var managerId = User.GetUserId();
            var reviewedApplication = await _service.UpdateReviewAsync(applicationNumber, managerId, request);
            return Ok(new ApiResponseDto<LoanApplicationReviewResponseDto> { Success = true, Data = reviewedApplication });
        }
        [Authorize(Roles = "Manager")]
        [HttpGet("{applicationNumber}/review")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanApplicationDetailsWithCustomerDataResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<LoanApplicationDetailsWithCustomerDataResponseDto>>> GetApplicationDetailsForReview(string applicationNumber )
        {
            var managerId = User.GetUserId();
            var applicationDetails = await _service.GetApplicationDetailsForReview(applicationNumber, managerId);
            return Ok(new ApiResponseDto<LoanApplicationDetailsWithCustomerDataResponseDto> { Success = true, Data = applicationDetails });
        }
    }

}
