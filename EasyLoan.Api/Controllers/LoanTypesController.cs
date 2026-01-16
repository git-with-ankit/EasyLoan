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
        public async Task<ActionResult<ApiResponseDto<List<LoanTypeResponseDto>>>> GetAll()
        {
            var loanTypes = await _service.GetAllAsync();
            return Ok(new ApiResponseDto<List<LoanTypeResponseDto>>
            {
                Success = true,
                Data = loanTypes
            });
        }

        [HttpGet("{loanTypeId}")]
        public async Task<ActionResult<ApiResponseDto<LoanTypeResponseDto>>> GetById(Guid loanTypeId)
        {
            var loanType = await _service.GetByIdAsync(loanTypeId);
            return Ok(new ApiResponseDto<LoanTypeResponseDto>
            {
                Success = true,
                Data = loanType
            });
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<Guid>>> CreateLoanType(
            CreateLoanTypeRequestDto request)
        {
            var id = await _service.CreateLoanTypeAsync(request);
            return Ok(new ApiResponseDto<Guid> { Success = true, Data = id });
        }
        // Customer selects loanTypeId, amount, tenure
        //[Authorize(Roles ="Customer")]
        [HttpGet("{loanTypeId}/emi-preview")]
        public async Task<IActionResult> PreviewEmiPlan(
             Guid loanTypeId,
            [FromQuery] decimal amount,
            [FromQuery] int tenureInMonths)
        {
            var plan = await _service.PreviewEmiAsync(
                loanTypeId,
                amount,
                tenureInMonths);

            return Ok(new ApiResponseDto<List<EmiScheduleItemResponseDto>> { Success = true, Data = plan });
        }
    }
}
