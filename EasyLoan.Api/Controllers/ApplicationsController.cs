using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.LoanApplication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace EasyLoan.Api.Controllers
{
    [ApiController]
    [Route("api/loan-applications")]
    public class LoanApplicationsController : ControllerBase
    {
        private readonly ILoanApplicationService _service;

        public LoanApplicationsController(ILoanApplicationService service)
        {
            _service = service;
        }

        //[Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<string>>> Create(CreateLoanApplicationRequestDto request)
        {
            //var appId = await _service.CreateAsync(User.GetUserId(), request);
            var applicationId = await _service.CreateAsync(request);
            return Ok(new ApiResponseDto<string> { Success = true, Data = applicationId });
        }

        //[Authorize(Roles = "Customer")]
        [HttpGet("customers/{customerId}")]
        public async Task<ActionResult<ApiResponseDto<List<LoanApplicationListItemResponseDto>>>> GetCustomerApplications(Guid customerId)
        {
            //var apps = await _service.GetCustomerApplicationsAsync(User.GetUserId());
            var applications = await _service.GetCustomerApplicationsAsync(customerId);
            return Ok(new ApiResponseDto<List<LoanApplicationListItemResponseDto>> { Success = true, Data = applications });
        }

        //[Authorize(Roles ="Manager")]
        [HttpGet("manager/{managerId}")]
        public async Task<ActionResult<ApiResponseDto<List<LoanApplicationListItemResponseDto>>>> GetAssignedApplications(Guid managerId)
        {
            var assignedApplications = await _service.GetAssignedApplicationsAsync(managerId);
            return Ok(new ApiResponseDto<List<LoanApplicationListItemResponseDto>> { Success = true, Data = assignedApplications });
        }
        ////[Authorize(Roles ="Manager")]
        //[HttpGet("details/{applicationId}")]
        //public async Task<ApiResponseDto<LoanApplicationDetailsResponseDto>> GetApplicationDetails(Guid applicationId)
        //{
        //    var assignedApplications = await _service.GetByApplicationIdAsync(applicationId);
        //    return Ok(new ApiResponseDto<LoanApplicationDetailsResponseDto> { Success = true, Data = assignedApplications });
        //}  
        
        /////////////////////////////////////////IMPLEMENT???????????????????????////

        //[Authorize(Roles="Customer")]
        [HttpGet("{applicationId}")]
        public async Task<ActionResult<ApiResponseDto<LoanApplicationDetailsResponseDto>>> GetByApplicationId(string applicationId)
        {
            var details = await _service.GetByApplicationIdAsync(applicationId);
            return Ok(new ApiResponseDto<LoanApplicationDetailsResponseDto> { Success = true, Data = details });
        }

        //[Authorize(Roles = "Manager")]
        //[HttpGet("manager/{managerId}/pending")]
        //public async Task<ActionResult<ApiResponseDto<List<LoanApplicationListItemResponseDto>>>> GetPending()
        //{
        //    var apps = await _service.GetPendingAsync();
        //    return Ok(new ApiResponseDto<List<LoanApplicationListItemResponseDto>> { Success = true, Data = apps });
        //}

        //[Authorize(Roles = "Manager")]
        [HttpPost("{applicationId}/review")]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateReview(string applicationId,ReviewLoanApplicationRequestDto request )
        {
            await _service.UpdateReviewAsync(applicationId, request);
            return Ok(new ApiResponseDto<bool> { Success = true, Data = true });
        }
        //[Authorize(Roles = "Manager")]
        [HttpGet("{applicationId}/review")]
        public async Task<ActionResult<ApiResponseDto<LoanApplicationDetailsWithCustomerDataResponseDto>>> GetApplicationDetailsForReview(string applicationId,ReviewLoanApplicationRequestDto request )
        {
            var applicationDetails = await _service.GetApplicationDetailsForReview(applicationId);
            return Ok(new ApiResponseDto<LoanApplicationDetailsWithCustomerDataResponseDto> { Success = true, Data = applicationDetails });
        }
    }

}
