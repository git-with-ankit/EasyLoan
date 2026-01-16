using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.LoanPayment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentsController : ControllerBase
    {
        private readonly ILoanPaymentService _service;

        public PaymentsController(ILoanPaymentService service)
        {
            _service = service;
        }

        //[Authorize(Roles = "Customer")]
        [HttpPost("{customerId}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MakePayment(Guid customerId,MakeLoanPaymentRequestDto request)
        {
            //await _service.MakePaymentAsync(User.GetUserId(), request);
            await _service.MakePaymentAsync(customerId, request);
            return Ok(new ApiResponseDto<bool> { Success = true, Data = true });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("history/{customerId}")]
        public async Task<ActionResult<ApiResponseDto<List<LoanPaymentHistoryResponseDto>>>> GetHistory(Guid customerId, LoanPaymentHistoryRequestDto dto)
        {
            var history = await _service.GetPaymentHistoryAsync(customerId, dto);
            return Ok(new ApiResponseDto<List<LoanPaymentHistoryResponseDto>> { Success = true, Data = history });
        }
    }

}
