using EasyLoan.Dtos.Loan;
using EasyLoan.Dtos.LoanType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/loan-types")]
    public class LoanTypesController : ControllerBase
    {
        private readonly ILoanTypeService _service;

        public LoanTypesController(ILoanTypeService service)
        {
            _service = service;
        }

        
        [HttpGet]
        [Authorize(Roles = "Customer,Manager,Admin")]
        [ProducesResponseType(typeof(IEnumerable<LoanTypeResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<LoanTypeResponseDto>>> GetAllLoanTypes()
        {
            var loanTypes = await _service.GetAllAsync();
            return Ok(loanTypes);
        }

        [HttpGet("{loanTypeId}")]
        [Authorize(Roles = "Customer,Manager,Admin")]
        [ProducesResponseType(typeof(LoanTypeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanTypeResponseDto>> GetLoanTypesById(Guid loanTypeId)
        {
            var loanType = await _service.GetByIdAsync(loanTypeId);
            return Ok(loanType);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(LoanTypeResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<LoanTypeResponseDto>> CreateLoanType(LoanTypeRequestDto request)
        {
            var newLoanType = await _service.CreateLoanTypeAsync(request);
            return CreatedAtAction(nameof(GetLoanTypesById), new { loanTypeId = newLoanType.Id }, newLoanType);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{loanTypeId}")]
        [ProducesResponseType(typeof(LoanTypeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LoanTypeResponseDto>> CreateLoanType(Guid loanTypeId, UpdateLoanTypeRequestDto request)
        {
            var updatedLoanType = await _service.UpdateLoanTypeAsync(loanTypeId, request);
            return Ok(updatedLoanType);
        }

        [Authorize(Roles = "Customer,Manager")]
        [HttpGet("{loanTypeId}/emi-plan")]
        [ProducesResponseType(typeof(IEnumerable<EmiScheduleItemResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<IEnumerable<EmiScheduleItemResponseDto>>> PreviewEmiPlan(Guid loanTypeId,[FromQuery] PreviewEmiQueryDto query)
        {
            var plan = await _service.PreviewEmiAsync(
                loanTypeId,
                query.Amount,
                query.TenureInMonths);

            return Ok(plan);
        }

    }
}
