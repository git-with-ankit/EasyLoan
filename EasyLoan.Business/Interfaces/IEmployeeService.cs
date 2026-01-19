using EasyLoan.Dtos.Customer;
using EasyLoan.Dtos.Employee;
using EasyLoan.Dtos.LoanApplication;
using EasyLoan.Dtos.LoanType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Interfaces
{
    public interface IEmployeeService
    {
        //Task<string> LoginAsync(EmployeeLoginRequestDto dto);
        //Task<Guid> CreateManagerAsync(CreateEmployeeRequestDto dto);
        Task<EmployeeProfileResponseDto> GetProfileAsync(Guid employeeId);
        Task UpdateProfileAsync(Guid employeeId, UpdateEmployeeProfileRequestDto updateProfile);
    }
}
