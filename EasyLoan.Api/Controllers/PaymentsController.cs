using Azure.Core;
using EasyLoan.Api.Extensions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.LoanPayment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/loan-payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly ILoanPaymentService _service;

        public PaymentsController(ILoanPaymentService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ApiResponseDto<bool>>> MakePayment(MakeLoanPaymentRequestDto request)
        {
            var customerId = User.GetUserId();
            await _service.MakePaymentAsync(customerId, request);
            return Ok(new ApiResponseDto<bool> { Success = true, Data = true });
        }

        [Authorize(Roles ="Customer")]
        [HttpGet("{loanId}")]
        [ProducesResponseType(typeof(ApiResponseDto<NextEmiPaymentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ApiResponseDto<List<NextEmiPaymentResponseDto>>>> GetNextPaymentAsync(Guid loanId)
        {
            var customerId = User.GetUserId();
            var payment = await _service.GetNextPaymentAsync(customerId, loanId);
            return Ok(new ApiResponseDto<List<NextEmiPaymentResponseDto>> { Success = true, Data = payment });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("{loanId}/history")]
        [ProducesResponseType(typeof(ApiResponseDto<List<LoanPaymentHistoryResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<List<LoanPaymentHistoryResponseDto>>>> GetHistory(Guid loanId)
        {
            var customerId = User.GetUserId();
            var history = await _service.GetPaymentHistoryAsync(customerId, loanId);
            return Ok(new ApiResponseDto<List<LoanPaymentHistoryResponseDto>> { Success = true, Data = history });
        }
    }

}
