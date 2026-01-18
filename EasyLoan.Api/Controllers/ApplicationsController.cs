using EasyLoan.Api.Extensions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.LoanApplication;
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
            var applicationId = await _service.CreateAsync(customerId, request);
            return CreatedAtAction(nameof(GetByApplicationId), new { applicationId }, new ApiResponseDto<string> { Success = true, Data = applicationId });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<List<LoanApplicationListItemResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)] // Customer not found
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)] // Not owner
        public async Task<ActionResult<ApiResponseDto<List<LoanApplicationListItemResponseDto>>>> GetCustomerApplications()
        {
            var customerId = User.GetUserId();
            var applications = await _service.GetCustomerApplicationsAsync(customerId);
            return Ok(new ApiResponseDto<List<LoanApplicationListItemResponseDto>> { Success = true, Data = applications });
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("assigned")]
        [ProducesResponseType(typeof(ApiResponseDto<List<LoanApplicationListItemResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)] 
        public async Task<ActionResult<ApiResponseDto<List<LoanApplicationListItemResponseDto>>>> GetAssignedApplications()
        {
            var managerId = User.GetUserId();
            var assignedApplications = await _service.GetAssignedApplicationsAsync(managerId);
            return Ok(new ApiResponseDto<List<LoanApplicationListItemResponseDto>> { Success = true, Data = assignedApplications });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        [ProducesResponseType(typeof(ApiResponseDto<List<LoanApplicationListItemResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto<List<LoanApplicationListItemResponseDto>>>> GetAllPendingApplications()
        {
            var assignedApplications = await _service.GetAllPendingApplicationsAsync();
            return Ok(new ApiResponseDto<List<LoanApplicationListItemResponseDto>> { Success = true, Data = assignedApplications });
        }

        [Authorize(Roles= "Customer")]
        [HttpGet("{applicationId}")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanApplicationDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)] // App not found
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)] // Not owner
        public async Task<ActionResult<ApiResponseDto<LoanApplicationDetailsResponseDto>>> GetByApplicationId(string applicationId)
        {
            var details = await _service.GetByApplicationIdAsync(applicationId);
            return Ok(new ApiResponseDto<LoanApplicationDetailsResponseDto> { Success = true, Data = details });
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("{applicationId}/review")]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)] // App not found
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)] // Not assigned manager
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)] // Invalid approval
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateReview(string applicationId,ReviewLoanApplicationRequestDto request )
        {
            await _service.UpdateReviewAsync(applicationId, request);
            return Ok(new ApiResponseDto<bool> { Success = true, Data = true });
        }
        [Authorize(Roles = "Manager")]
        [HttpGet("{applicationId}/review")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanApplicationDetailsWithCustomerDataResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<LoanApplicationDetailsWithCustomerDataResponseDto>>> GetApplicationDetailsForReview(string applicationId )
        {
            var applicationDetails = await _service.GetApplicationDetailsForReview(applicationId);
            return Ok(new ApiResponseDto<LoanApplicationDetailsWithCustomerDataResponseDto> { Success = true, Data = applicationDetails });
        }
    }

}
