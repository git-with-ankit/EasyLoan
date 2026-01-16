using EasyLoan.Business.Interfaces;
using EasyLoan.Business.Services;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.Loan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _service;

        public LoansController(ILoanService service)
        {
            _service = service;
        }

        //[Authorize(Roles = "Customer")]
        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetAllCustomerLoans(Guid customerId)
        {
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
        [HttpGet("details/{loanId}")]
        public async Task<ActionResult<ApiResponseDto<LoanDetailsResponseDto>>> GetDetails(Guid loanId)
        {
            var customerId = new Guid();//TODO : Get the customer id
            var loan = await _service.GetLoanDetailsAsync(customerId, loanId);
            return Ok(new ApiResponseDto<LoanDetailsResponseDto> { Success = true, Data = loan });
        }
    }
}
