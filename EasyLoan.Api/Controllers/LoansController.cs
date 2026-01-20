using EasyLoan.Api.Extensions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Business.Services;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.Loan;
using EasyLoan.Dtos.LoanPayment;
using EasyLoan.Models.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/loans")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly ILoanPaymentService _paymentService;

        public LoansController(ILoanService loanservice, ILoanPaymentService paymentservice)
        {
            _loanService = loanservice;
            _paymentService = paymentservice;
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<List<LoanSummaryResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<List<LoanSummaryResponseDto>>>> GetCustomerLoans(LoanStatus status)
        {
            var customerId = User.GetUserId();
            var loans = await _loanService.GetCustomerLoansAsync(customerId, status);
            return Ok(new ApiResponseDto<List<LoanSummaryResponseDto>> { Success = true, Data = loans }) ;
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("{loanNumber}")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<LoanDetailsResponseDto>>> GetDetails(string loanNumber)
        {
            var customerId = User.GetUserId();
            var loan = await _loanService.GetLoanDetailsAsync(customerId, loanNumber);
            return Ok(new ApiResponseDto<LoanDetailsResponseDto> { Success = true, Data = loan });
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("{loanNumber}/payments")]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ApiResponseDto<bool>>> MakePayment(string loanNumber, MakeLoanPaymentRequestDto request)
        {
            var customerId = User.GetUserId();
            await _paymentService.MakePaymentAsync(customerId, loanNumber, request);
            return Ok(new ApiResponseDto<bool> { Success = true, Data = true });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("emis")]
        [ProducesResponseType(typeof(ApiResponseDto<DueEmisResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ApiResponseDto<List<List<DueEmisResponseDto>>>>> GetAllDueEmisAsync(EmiDueStatus status)
        {
            var customerId = User.GetUserId();
            var payments = await _paymentService.GetAllDueEmisAsync(customerId, status);
            return Ok(new ApiResponseDto<List<List<DueEmisResponseDto>>> { Success = true, Data = payments });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("{loanNumber}/emis")]
        [ProducesResponseType(typeof(ApiResponseDto<DueEmisResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ApiResponseDto<List<DueEmisResponseDto>>>> GetDueEmisAsync(string loanNumber, EmiDueStatus status)
        {
            var customerId = User.GetUserId();
            var payments = await _paymentService.GetDueEmisAsync(customerId, loanNumber, status);
            return Ok(new ApiResponseDto<List<DueEmisResponseDto>> { Success = true, Data = payments });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("{loanNumber}/payments")]
        [ProducesResponseType(typeof(ApiResponseDto<List<LoanPaymentHistoryResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<List<LoanPaymentHistoryResponseDto>>>> GetPaymentsHistory(string loanNumber)
        {
            var customerId = User.GetUserId();
            var history = await _paymentService.GetPaymentsHistoryAsync(customerId, loanNumber);
            return Ok(new ApiResponseDto<List<LoanPaymentHistoryResponseDto>> { Success = true, Data = history });
        }
    }
}
