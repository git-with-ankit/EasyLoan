using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.Dtos.Customer;
using EasyLoan.Dtos.Employee;
using EasyLoan.Models.Common.Enums;

namespace EasyLoan.Business.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ILoanTypeRepository _loanTypeRepo;
        private readonly ILoanApplicationRepository _loanApplicationRepo;

        public EmployeeService(
            IEmployeeRepository employeeRepo,
            ILoanTypeRepository loanTypeRepo,
            ILoanApplicationRepository loanApplicationRepo)
        {
            _employeeRepo = employeeRepo;
            _loanTypeRepo = loanTypeRepo;
            _loanApplicationRepo = loanApplicationRepo;
        }

        public async Task<EmployeeProfileResponseDto> GetProfileAsync(Guid employeeId)
        {
            var employee = await _employeeRepo.GetByIdAsync(employeeId)
                ?? throw new NotFoundException(ErrorMessages.CustomerNotFound);

            if(employee.Id != employeeId) 
                throw new ForbiddenException(ErrorMessages.AccessDenied);

            return new EmployeeProfileResponseDto
            {
                Name = employee.Name,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                Role = employee.Role,
            };
        }

        public async Task<EmployeeProfileResponseDto> UpdateProfileAsync(Guid employeeId, UpdateEmployeeProfileRequestDto dto)
        {
            var employee = await _employeeRepo.GetByIdAsync(employeeId)
                ?? throw new NotFoundException(ErrorMessages.CustomerNotFound);

            if(employee.Id != employeeId)
                throw new ForbiddenException(ErrorMessages.AccessDenied);

            employee.Name = dto.Name?.Trim() ?? employee.Name;
            employee.PhoneNumber = dto.PhoneNumber?.Trim() ?? employee.PhoneNumber;

            //await _employeeRepo.UpdateAsync(employee);
            await _employeeRepo.SaveChangesAsync();

            return new EmployeeProfileResponseDto()
            {
                Name = employee.Name,
                PhoneNumber = employee.PhoneNumber,
                Email = employee.Email,
                Role = employee.Role,
            };
        }

        public async Task<AdminDashboardResponseDto> GetAdminDashboardAsync()
        {
            var loanTypes = await _loanTypeRepo.GetAllAsync();

            var applications = await _loanApplicationRepo.GetAllAsync();

            var managers = await _employeeRepo.GetManagersAsync();

            return new AdminDashboardResponseDto
            {
                NumberOfLoanTypes = loanTypes.Count(),
                NumberOfPendingApplications =
                    applications.Count(a => a.Status == LoanApplicationStatus.Pending),

                NumberOfApprovedApplications =
                    applications.Count(a => a.Status == LoanApplicationStatus.Approved),

                NumberOfRejectedApplications =
                    applications.Count(a => a.Status == LoanApplicationStatus.Rejected),

                NumberOfManagers = managers.Count()
            };
        }
    }
}
