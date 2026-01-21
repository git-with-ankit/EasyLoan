using EasyLoan.Dtos.Common;
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
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<LoanTypeResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<LoanTypeResponseDto>>>> GetAllLoanTypes()
        {
            var loanTypes = await _service.GetAllAsync();
            return Ok(new ApiResponseDto<IEnumerable<LoanTypeResponseDto>>
            {
                Success = true,
                Data = loanTypes
            });
        }

        [HttpGet("{loanTypeId}")]
        [Authorize(Roles = "Customer,Manager,Admin")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanTypeResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<LoanTypeResponseDto>>> GetLoanTypesById(Guid loanTypeId)
        {
            var loanType = await _service.GetByIdAsync(loanTypeId);
            return Ok(new ApiResponseDto<LoanTypeResponseDto>
            {
                Success = true,
                Data = loanType
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<LoanTypeResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ApiResponseDto<LoanTypeResponseDto>>> CreateLoanType(LoanTypeRequestDto request)
        {
            var newLoanType = await _service.CreateLoanTypeAsync(request);
            return CreatedAtAction(nameof(GetLoanTypesById), new { loanTypeId = newLoanType.Id },new ApiResponseDto<LoanTypeResponseDto> { Success = true, Data = newLoanType });
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{loanTypeId}")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanTypeResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<LoanTypeResponseDto>>> CreateLoanType(Guid loanTypeId, LoanTypeRequestDto request)
        {
            var updatedLoanType = await _service.UpdateLoanTypeAsync(loanTypeId, request);
            return Ok(new ApiResponseDto<LoanTypeResponseDto> { Success = true, Data = updatedLoanType });
        }

        [Authorize(Roles = "Customer,Manager")]
        [HttpGet("{loanTypeId}/emi-plan")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<EmiScheduleItemResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<EmiScheduleItemResponseDto>>>> PreviewEmiPlan(Guid loanTypeId,[FromQuery] PreviewEmiQueryDto query)
        {
            var plan = await _service.PreviewEmiAsync(
                loanTypeId,
                query.Amount,
                query.TenureInMonths);

            return Ok(new ApiResponseDto<IEnumerable<EmiScheduleItemResponseDto>>
            {
                Success = true,
                Data = plan
            });
        }

    }
}
