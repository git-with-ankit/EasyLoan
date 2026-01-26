using EasyLoan.Api.Extensions;
using EasyLoan.Business.Interfaces;
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
        [ProducesResponseType(typeof(CreatedApplicationResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]   
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] 
        public async Task<ActionResult<CreatedApplicationResponseDto>> Create(CreateLoanApplicationRequestDto request)
        {
            var customerId = User.GetUserId();
            var application = await _service.CreateAsync(customerId, request);
            return CreatedAtAction(nameof(GetByApplicationNumber), new { applicationNumber = application.ApplicationNumber }, application);
        }

        [Authorize(Roles ="Admin,Manager,Customer")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LoanApplicationsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(   ProblemDetails), StatusCodes.Status404NotFound)] // Customer not found
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)] // Not owner
        public async Task<ActionResult<IEnumerable<LoanApplicationsResponseDto>>> GetApplications([FromQuery] LoanApplicationStatus status)
        {
            var userId = User.GetUserId();
            var userRole = User.GetRole();

            var applications = await _service.GetApplicationsAsync(userId, userRole, status);

            return Ok(applications);

        }

        [Authorize(Roles= "Customer")]
        [HttpGet("{applicationNumber}")]
        [ProducesResponseType(typeof(LoanApplicationDetailsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)] // App not found
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)] // Not owner
        public async Task<ActionResult<LoanApplicationDetailsResponseDto>> GetByApplicationNumber(string applicationNumber)
        {
            var details = await _service.GetByApplicationNumberAsync(applicationNumber);
            return Ok(details);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("{applicationNumber}/review")]
        [ProducesResponseType(typeof(LoanApplicationReviewResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)] 
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] 
        public async Task<ActionResult<LoanApplicationReviewResponseDto>> UpdateReview(string applicationNumber,ReviewLoanApplicationRequestDto request )
        {
            var managerId = User.GetUserId();
            var reviewedApplication = await _service.UpdateReviewAsync(applicationNumber, managerId, request);
            return Ok(reviewedApplication);
        }
        [Authorize(Roles = "Manager")]
        [HttpGet("{applicationNumber}/review")]
        [ProducesResponseType(typeof(LoanApplicationDetailsWithCustomerDataResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanApplicationDetailsWithCustomerDataResponseDto>> GetApplicationDetailsForReview(string applicationNumber )
        {
            var managerId = User.GetUserId();
            var applicationDetails = await _service.GetApplicationDetailsForReview(applicationNumber, managerId);
            return Ok(applicationDetails);
        }
    }

}
