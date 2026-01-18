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
        [ProducesResponseType(typeof(ApiResponseDto<List<LoanTypeResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto<List<LoanTypeResponseDto>>>> GetAllLoanTypes()
        {
            var loanTypes = await _service.GetAllAsync();
            return Ok(new ApiResponseDto<List<LoanTypeResponseDto>>
            {
                Success = true,
                Data = loanTypes
            });
        }

        [HttpGet("{loanTypeId}")]
        [Authorize(Roles = "Customer,Manager,Admin")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanTypeResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(typeof(ApiResponseDto<Guid>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<Guid>>> CreateLoanType(CreateLoanTypeRequestDto request)
        {
            var id = await _service.CreateLoanTypeAsync(request);
            return CreatedAtAction(nameof(GetLoanTypesById), new { loanTypeId = id },new ApiResponseDto<Guid> { Success = true, Data = id });
        }

        [Authorize(Roles = "Customer,Manager")]
        [HttpGet("{loanTypeId}/emi-plan")]
        [ProducesResponseType(typeof(ApiResponseDto<List<EmiScheduleItemResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ApiResponseDto<List<EmiScheduleItemResponseDto>>>> PreviewEmiPlan(Guid loanTypeId,[FromQuery] PreviewEmiQueryDto query)
        {
            var plan = await _service.PreviewEmiAsync(
                loanTypeId,
                query.Amount,
                query.TenureInMonths);

            return Ok(new ApiResponseDto<List<EmiScheduleItemResponseDto>>
            {
                Success = true,
                Data = plan
            });
        }

    }
}
