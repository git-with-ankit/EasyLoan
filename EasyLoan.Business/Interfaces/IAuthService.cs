using EasyLoan.Dtos.Customer;
using EasyLoan.Dtos.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Interfaces
{
    public interface IAuthService
    {
        Task<CustomerProfileResponseDto> RegisterCustomerAsync(RegisterCustomerRequestDto dto);
        Task<string> LoginCustomerAsync(CustomerLoginRequestDto dto);
        Task<RegisterManagerResponseDto> RegisterManagerAsync(CreateManagerRequestDto dto);
        Task<string> LoginEmployeeAsync(EmployeeLoginRequestDto dto);
    }
}
