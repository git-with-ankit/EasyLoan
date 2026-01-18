using EasyLoan.Api.Extensions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Business.Services;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.Loan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/loans")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _service;

        public LoansController(ILoanService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<List<LoanSummaryResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<List<LoanSummaryResponseDto>>>> GetAllCustomerLoans()
        {
            var customerId = User.GetUserId();
            var loans = await _service.GetAllCustomerLoansAsync(customerId);
            return Ok(new ApiResponseDto<List<LoanSummaryResponseDto>> { Success = true, Data = loans }) ;
        }

        ////[Authorize(Roles = "Customer")]
        //[HttpGet("customer/{customerId}/closed")]
        //public async Task<IActionResult> GetAllClosedLoans(Guid customerId)
        //{
        //    var loans = await _service.GetAllClosedLoansAsync(customerId);
        //    return Ok(loans);
        //}

        [Authorize(Roles = "Customer")]
        [HttpGet("{loanId}")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<LoanDetailsResponseDto>>> GetDetails(Guid loanId)
        {
            var customerId = User.GetUserId();
            var loan = await _service.GetLoanDetailsAsync(customerId, loanId);
            return Ok(new ApiResponseDto<LoanDetailsResponseDto> { Success = true, Data = loan });
        }
    }
}
