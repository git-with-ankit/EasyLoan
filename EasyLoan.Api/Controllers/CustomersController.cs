using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerservice;

        public CustomersController(ICustomerService service)
        {
            _customerservice = service;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponseDto<Guid>>> Register(
            RegisterCustomerRequestDto request)
        {
            var id = await _customerservice.RegisterAsync(request);
            return Ok(new ApiResponseDto<Guid> { Success = true, Data = id });
        }

        //[HttpPost("login")]
        //public async Task<ActionResult<ApiResponseDto<string>>> Login(
        //    CustomerLoginRequestDto request)
        //{
        //    var token = await _customerservice.LoginAsync(request);
        //    return Ok(new ApiResponseDto<string> { Success = true, Data = token });//TODO : Return status code 201
        //}

        //[Authorize(Roles = "Customer")]
        [HttpGet("profile/{customerId}")]
        public async Task<ActionResult<ApiResponseDto<CustomerProfileResponseDto>>> GetProfile(Guid customerId)
        {
            //var customerId = User.GetUserId();
            var profile = await _customerservice.GetProfileAsync(customerId);
            return Ok(new ApiResponseDto<CustomerProfileResponseDto> { Success = true, Data = profile });
        }

        //[Authorize(Roles = "Customer")]
        [HttpPut("profile/{customerId}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateProfile(Guid customerId,
            UpdateCustomerProfileRequestDto updateProfile)
        {
            //await _service.UpdateProfileAsync(User.GetUserId(), request);
            await _customerservice.UpdateProfileAsync(customerId, updateProfile);
            return Ok(new ApiResponseDto<bool> { Success = true, Data = true });
        }

        //[Authorize(Roles = "Customer")]
        [HttpGet("dashboard/{customerId}")]
        public async Task<ActionResult<ApiResponseDto<CustomerDashboardResponseDto>>> GetDashboard(Guid customerId)
        {
            //var dashboard = await _service.GetDashboardAsync(User.GetUserId());
            var dashboard = await _customerservice.GetDashboardAsync(customerId);
            return Ok(new ApiResponseDto<CustomerDashboardResponseDto> { Success = true, Data = dashboard });
        }
    }
}
