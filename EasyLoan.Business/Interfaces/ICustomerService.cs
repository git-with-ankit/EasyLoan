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
        Task<CustomerProfileResponseDto> GetProfileAsync(Guid customerId);
        Task<CustomerProfileResponseDto> UpdateProfileAsync(Guid customerId, UpdateCustomerProfileRequestDto dto);
    }
}
