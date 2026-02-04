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

        //public async Task<string> LoginAsync(EmployeeLoginRequestDto dto)
        //{
        //    var emp = await _employeeRepo.GetByEmailAsync(dto.Email)
        //        ?? throw new UnauthorizedAccessException();

        //    if (dto.Password == emp.Password) //TODO : Check hashed password
        //        throw new UnauthorizedAccessException();

        //    //return _tokenService.GenerateEmployeeToken(emp);  TODO : Generate token
        //    return "Created";
        //}

        ////public async Task<List<AssignedLoanApplicationsResponseDto>> GetAssignedApplicationsAsync(Guid employeeId)
        ////{
        ////    var manager = await _employeeRepo.GetByIdAsync(employeeId)
        ////        ?? throw new KeyNotFoundException();

        ////    return manager.AssignedLoanApplications.Select(a => new AssignedLoanApplicationsResponseDto
        ////    {
        ////        ApplicationNumber = a.ApplicationNumber,
        ////        Status = a.Status.ToString(),
        ////        LoanTypeName = a.LoanType.Name,
        ////        CreatedDate = a.CreatedDate,
        ////        CustomerName = a.Customer.Name,
        ////        RequestedTenureInMonths = a.RequestedTenureInMonths,
        ////        RequestedAmount = a.RequestedAmount
        ////    }).ToList();
        ////} TODO : Remove
        //public async Task<Guid> CreateManagerAsync(CreateEmployeeRequestDto dto)
        //{
        //    var emp = new Employee
        //    {
        //        Id = Guid.NewGuid(),
        //        Name = dto.Name,
        //        Email = dto.Email,
        //        PhoneNumber = dto.PhoneNumber,
        //        Password = dto.Password,//TODO : Hash Password
        //        Role = EmployeeRole.Manager
        //    };

        //    await _employeeRepo.AddAsync(emp);
        //    await _employeeRepo.SaveChangesAsync();

        //    return emp.Id;
        //}

        //public async Task<Guid> CreateLoanTypeAsync(CreateLoanTypeRequestDto dto)
        //{
        //    var type = new LoanType
        //    {
        //        Id = Guid.NewGuid(),
        //        Name = dto.Name,
        //        InterestRate = dto.InterestRate,
        //        MinAmount = dto.MinAmount,
        //        MaxTenureInMonths = dto.MaxTenureInMonths
        //    };

        //    await _loanTypeRepo.AddAsync(type);
        //    await _loanTypeRepo.SaveChangesAsync();

        //    return type.Id;
        //}

        //public async Task UpdateLoanTypeAsync(Guid loanTypeId, UpdateLoanTypeRequestDto dto)
        //{
        //    var type = await _loanTypeRepo.GetByIdAsync(loanTypeId)
        //        ?? throw new KeyNotFoundException();

        //    type.InterestRate = dto.InterestRate;
        //    type.MinAmount = dto.MinAmount;
        //    type.MaxTenureInMonths = dto.MaxTenureInMonths;

        //    await _loanTypeRepo.UpdateAsync(type);
        //    await _loanTypeRepo.SaveChangesAsync();
        //}

        //public async Task<List<LoanTypeResponseDto>> GetLoanTypesAsync()
        //{
        //    var types = await _loanTypeRepo.GetAllAsync();

        //    return types.Select(t => new LoanTypeResponseDto
        //    {
        //        Id = t.Id,
        //        Name = t.Name,
        //        InterestRate = t.InterestRate,
        //        MinAmount = t.MinAmount,
        //        MaxTenureInMonths = t.MaxTenureInMonths
        //    }).ToList();
        //}  TODO : Remove

        //public async Task<List<LoanApplicationListItemResponseDto>> GetAllApplications()
        //{
        //    var applications = await _loanApplicationRepo.GetAllAsync();

        //    return applications
        //        .Select(a => new LoanApplicationListItemResponseDto
        //        {
        //            ApplicationNumber = a.ApplicationNumber,
        //            Status = a.Status.ToString(),
        //            LoanTypeName = a.LoanType.Name,
        //            CreatedDate = a.CreatedDate,
        //            AssignedEmployeeId = a.AssignedEmployeeId,
        //            RequestedAmount = a.RequestedAmount
        //        }).ToList();
        //} TODO : Remove
    }
}
