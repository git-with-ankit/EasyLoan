using EasyLoan.Api.Extensions;
using EasyLoan.Business.Interfaces;
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
        [ProducesResponseType(typeof(IEnumerable<LoanSummaryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<LoanSummaryResponseDto>>> GetCustomerLoans(LoanStatus status)
        {
            var customerId = User.GetUserId();
            var loans = await _loanService.GetCustomerLoansAsync(customerId, status);
            return Ok(loans) ;
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("{loanNumber}")]
        [ProducesResponseType(typeof(LoanDetailsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LoanDetailsResponseDto>> GetDetails(string loanNumber)
        {
            var customerId = User.GetUserId();
            var loan = await _loanService.GetLoanDetailsAsync(customerId, loanNumber);
            return Ok(loan);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("{loanNumber}/payments")]
        [ProducesResponseType(typeof(LoanPaymentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<LoanPaymentResponseDto>> MakePayment(string loanNumber, MakeLoanPaymentRequestDto request)
        {
            var customerId = User.GetUserId();
            var payment = await _paymentService.MakePaymentAsync(customerId, loanNumber, request);
            return Ok(payment);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("emis")]
        [ProducesResponseType(typeof(DueEmisResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<IEnumerable<IEnumerable<DueEmisResponseDto>>>> GetAllDueEmisAsync(EmiDueStatus status)
        {
            var customerId = User.GetUserId();
            var payments = await _paymentService.GetAllDueEmisAsync(customerId, status);
            return Ok(payments);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("{loanNumber}/emis")]
        [ProducesResponseType(typeof(DueEmisResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<IEnumerable<DueEmisResponseDto>>> GetDueEmisAsync(string loanNumber, EmiDueStatus status)
        {
            var customerId = User.GetUserId();
            var payments = await _paymentService.GetDueEmisAsync(customerId, loanNumber, status);
            return Ok(payments);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("{loanNumber}/payments")]
        [ProducesResponseType(typeof(IEnumerable<LoanPaymentHistoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<LoanPaymentHistoryResponseDto>>> GetPaymentsHistory(string loanNumber)
        {
            var customerId = User.GetUserId();
            var history = await _paymentService.GetPaymentsHistoryAsync(customerId, loanNumber);
            return Ok(history);
        }
    }
}
