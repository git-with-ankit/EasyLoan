using EasyLoan.Business.Constants;
using EasyLoan.Business.Enums;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
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

        public async Task<RegisterCustomerResponseDto> RegisterCustomerAsync(RegisterCustomerRequestDto dto)
        {
            if (await _customerRepo.ExistsByEmailAsync(dto.Email))
                throw new ValidationException(ErrorMessages.EmailAlreadyExists);

            if (await _customerRepo.ExistsByPanAsync(dto.PanNumber))
                throw new ValidationException(ErrorMessages.PanAlreadyExists);

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

            return new RegisterCustomerResponseDto()
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

            return _tokenGenerator.GenerateToken(customer.Id , Role.Customer);//Generate token
        }

        public async Task<RegisterManagerResponseDto> RegisterManagerAsync(CreateManagerRequestDto dto)
        {
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
                return _tokenGenerator.GenerateToken(emp.Id, Role.Manager);
            }
            else
            {
                return _tokenGenerator.GenerateToken(emp.Id, Role.Admin);
            }
        }
    }
}
