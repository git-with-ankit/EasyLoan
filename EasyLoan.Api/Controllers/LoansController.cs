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
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<LoanSummaryResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<LoanSummaryResponseDto>>>> GetCustomerLoans(LoanStatus status)
        {
            var customerId = User.GetUserId();
            var loans = await _loanService.GetCustomerLoansAsync(customerId, status);
            return Ok(new ApiResponseDto<IEnumerable<LoanSummaryResponseDto>> { Success = true, Data = loans }) ;
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("{loanNumber}")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponseDto<LoanDetailsResponseDto>>> GetDetails(string loanNumber)
        {
            var customerId = User.GetUserId();
            var loan = await _loanService.GetLoanDetailsAsync(customerId, loanNumber);
            return Ok(new ApiResponseDto<LoanDetailsResponseDto> { Success = true, Data = loan });
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("{loanNumber}/payments")]
        [ProducesResponseType(typeof(ApiResponseDto<LoanPaymentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ApiResponseDto<LoanPaymentResponseDto>>> MakePayment(string loanNumber, MakeLoanPaymentRequestDto request)
        {
            var customerId = User.GetUserId();
            var payment = await _paymentService.MakePaymentAsync(customerId, loanNumber, request);
            return Ok(new ApiResponseDto<LoanPaymentResponseDto> { Success = true, Data = payment });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("emis")]
        [ProducesResponseType(typeof(ApiResponseDto<DueEmisResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<IEnumerable<DueEmisResponseDto>>>>> GetAllDueEmisAsync(EmiDueStatus status)
        {
            var customerId = User.GetUserId();
            var payments = await _paymentService.GetAllDueEmisAsync(customerId, status);
            return Ok(new ApiResponseDto<IEnumerable<IEnumerable<DueEmisResponseDto>>> { Success = true, Data = payments });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("{loanNumber}/emis")]
        [ProducesResponseType(typeof(ApiResponseDto<DueEmisResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<DueEmisResponseDto>>>> GetDueEmisAsync(string loanNumber, EmiDueStatus status)
        {
            var customerId = User.GetUserId();
            var payments = await _paymentService.GetDueEmisAsync(customerId, loanNumber, status);
            return Ok(new ApiResponseDto<IEnumerable<DueEmisResponseDto>> { Success = true, Data = payments });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("{loanNumber}/payments")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<LoanPaymentHistoryResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<ProblemDetails>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<LoanPaymentHistoryResponseDto>>>> GetPaymentsHistory(string loanNumber)
        {
            var customerId = User.GetUserId();
            var history = await _paymentService.GetPaymentsHistoryAsync(customerId, loanNumber);
            return Ok(new ApiResponseDto<IEnumerable<LoanPaymentHistoryResponseDto>> { Success = true, Data = history });
        }
    }
}
