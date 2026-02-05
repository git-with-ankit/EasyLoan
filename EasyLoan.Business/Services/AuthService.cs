using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.Customer;
using EasyLoan.Dtos.Employee;
using EasyLoan.Models.Common.Enums;

namespace EasyLoan.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IJwtTokenGeneratorService _tokenGenerator;

        public AuthService(
            ICustomerRepository customerRepo, IEmployeeRepository employeeRepo, IJwtTokenGeneratorService tokenGenerator)
        {
            _customerRepo = customerRepo;
            _employeeRepo = employeeRepo;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<CustomerProfileResponseDto> RegisterCustomerAsync(RegisterCustomerRequestDto dto)
        {
            if (await _customerRepo.ExistsByEmailAsync(dto.Email.ToLower().Trim()))
                throw new BusinessRuleViolationException(ErrorMessages.EmailAlreadyExists);

            if (await _employeeRepo.ExistsByEmailAsync(dto.Email.ToLower().Trim()))
                throw new BusinessRuleViolationException(ErrorMessages.EmailAlreadyExists);

            if (await _customerRepo.ExistsByPanAsync(dto.PanNumber))
                throw new BusinessRuleViolationException(ErrorMessages.PanAlreadyExists);

            if (dto.DateOfBirth > DateTime.UtcNow.AddYears(-18))
                throw new BusinessRuleViolationException("Customer must be at least 18 years old.");

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Email = dto.Email.ToLower().Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                DateOfBirth = dto.DateOfBirth,
                AnnualSalary = dto.AnnualSalary,
                PanNumber = dto.PanNumber.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreditScore = 800,
                CreatedDate = DateTime.UtcNow
            };

            await _customerRepo.AddAsync(customer);
            await _customerRepo.SaveChangesAsync();

            return new CustomerProfileResponseDto()
            {
                Name = customer.Name,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                DateOfBirth = customer.DateOfBirth,
                AnnualSalary = customer.AnnualSalary,
                CreditScore = customer.CreditScore,
                PanNumber = customer.PanNumber
            };
        }

        public async Task<string> LoginCustomerAsync(CustomerLoginRequestDto dto)
        {
            var customer = await _customerRepo.GetByEmailAsync(dto.Email.ToLower().Trim())
                ?? throw new AuthenticationFailedException(ErrorMessages.InvalidCredentials);

            var passwordResult = BCrypt.Net.BCrypt.Verify(dto.Password, customer.Password);
            if (!passwordResult) 
                throw new AuthenticationFailedException(ErrorMessages.InvalidCredentials);

            return _tokenGenerator.GenerateToken(customer.Id, customer.Email, Role.Customer);//Generate token
        }

        public async Task<RegisterManagerResponseDto> RegisterManagerAsync(CreateManagerRequestDto dto)
        {
            if (await _employeeRepo.ExistsByEmailAsync(dto.Email.ToLower().Trim()))
                throw new BusinessRuleViolationException(ErrorMessages.EmailAlreadyExists);

            if(await _customerRepo.ExistsByEmailAsync(dto.Email.ToLower().Trim()))
                throw new BusinessRuleViolationException(ErrorMessages.EmailAlreadyExists);

            var emp = new Employee
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Email = dto.Email.ToLower().Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Password =  BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = EmployeeRole.Manager
            };

            await _employeeRepo.AddAsync(emp);
            await _employeeRepo.SaveChangesAsync();

            return new RegisterManagerResponseDto() 
            { 
                Name = emp.Name,
                Email = emp.Email,
                PhoneNumber = emp.PhoneNumber,
                Role = emp.Role
            };
        }
        public async Task<string> LoginEmployeeAsync(EmployeeLoginRequestDto dto)
        {
            var emp = await _employeeRepo.GetByEmailAsync(dto.Email.ToLower().Trim())
                ?? throw new AuthenticationFailedException(ErrorMessages.InvalidCredentials);

            var passwordResult = BCrypt.Net.BCrypt.Verify(dto.Password, emp.Password);

            if (!passwordResult)
                throw new AuthenticationFailedException(ErrorMessages.InvalidCredentials);

            if (emp.Role == EmployeeRole.Manager)
            {
                return _tokenGenerator.GenerateToken(emp.Id, emp.Email, Role.Manager);
            }
            else
            {
                return _tokenGenerator.GenerateToken(emp.Id, emp.Email, Role.Admin);
            }
        }

        public async Task<string> LoginAsync(LoginRequestDto dto)
        {
            var customer = await _customerRepo.GetByEmailAsync(dto.Email.ToLower().Trim());

            if (customer != null)
            {
                var passwordResult = BCrypt.Net.BCrypt.Verify(dto.Password, customer.Password);
                if (!passwordResult)
                    throw new AuthenticationFailedException(ErrorMessages.InvalidCredentials);

                return _tokenGenerator.GenerateToken(customer.Id, customer.Email, Role.Customer);//Generate token
            }
            else
            {

                var employee = await _employeeRepo.GetByEmailAsync(dto.Email.ToLower().Trim()) 
                    ?? throw new AuthenticationFailedException(ErrorMessages.InvalidCredentials);
                
                var passwordResult = BCrypt.Net.BCrypt.Verify(dto.Password, employee.Password);

                if (!passwordResult)
                    throw new AuthenticationFailedException(ErrorMessages.InvalidCredentials);

                var role = employee.Role == EmployeeRole.Manager ? Role.Manager : Role.Admin;

                var token = _tokenGenerator.GenerateToken(employee.Id, employee.Email, role);

                return token;
            }
        }

        public async Task ChangePasswordAsync(Guid userId, Role role, ChangePasswordRequestDto dto)
        {
            if (role == Role.Customer)
            {
                var customer = await _customerRepo.GetByIdAsync(userId);
                if (customer == null)
                    throw new NotFoundException(ErrorMessages.CustomerNotFound);

                if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, customer.Password)) // Changed PasswordHash to Password
                    throw new AuthenticationFailedException("Old password is incorrect.");

                customer.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword); // Changed PasswordHash to Password
                // await _customerRepo.UpdateAsync(customer);
                await _customerRepo.SaveChangesAsync(); // Added SaveChangesAsync
            }
            else // Manager or Admin
            {
                var employee = await _employeeRepo.GetByIdAsync(userId);
                if (employee == null)
                    throw new NotFoundException(ErrorMessages.EmployeeNotFound);

                if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, employee.Password)) // Changed PasswordHash to Password
                    throw new AuthenticationFailedException("Old password is incorrect.");

                employee.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword); // Changed PasswordHash to Password
                // await _employeeRepo.UpdateAsync(employee);
                await _employeeRepo.SaveChangesAsync(); // Added SaveChangesAsync
            }
        }
    }
}
