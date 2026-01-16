using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Interfaces
{
    public interface ICustomerService
    {
        Task<Guid> RegisterAsync(RegisterCustomerRequestDto dto);
        Task<string> LoginAsync(CustomerLoginRequestDto dto);
        Task<CustomerProfileResponseDto> GetProfileAsync(Guid customerId);
        Task UpdateProfileAsync(Guid customerId, UpdateCustomerProfileRequestDto dto);
        Task<CustomerDashboardResponseDto> GetDashboardAsync(Guid customerId);
    }
}
