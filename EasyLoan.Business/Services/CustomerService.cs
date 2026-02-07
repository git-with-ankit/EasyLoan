using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.Dtos.Customer;

namespace EasyLoan.Business.Services
{

    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepo;

        public CustomerService(
            ICustomerRepository customerRepo)
        {
            _customerRepo = customerRepo;
        }

        public async Task<CustomerProfileResponseDto> GetProfileAsync(Guid customerId)
        {
            var customer = await _customerRepo.GetByIdAsync(customerId)
                ?? throw new NotFoundException(ErrorMessages.CustomerNotFound);

            return new CustomerProfileResponseDto
            {
                Name = customer.Name,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                DateOfBirth = customer.DateOfBirth,
                CreditScore = customer.CreditScore,
                AnnualSalary = customer.AnnualSalary,
                PanNumber = customer.PanNumber
            };
        }

        public async Task<CustomerProfileResponseDto> UpdateProfileAsync(Guid customerId, UpdateCustomerProfileRequestDto dto)
        {
            var customer = await _customerRepo.GetByIdAsync(customerId)
                ?? throw new NotFoundException(ErrorMessages.CustomerNotFound);

            customer.Name = dto.Name?.Trim() ?? customer.Name;
            customer.PhoneNumber = dto.PhoneNumber?.Trim() ?? customer.PhoneNumber;
            customer.AnnualSalary = dto.AnnualSalary ?? customer.AnnualSalary;

            //await _customerRepo.UpdateAsync(customer);
            await _customerRepo.SaveChangesAsync();

            return new CustomerProfileResponseDto()
            {
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                AnnualSalary = customer.AnnualSalary,
                PanNumber = customer.PanNumber,
                CreditScore = customer.CreditScore,
                DateOfBirth = customer.DateOfBirth,
                Name = customer.Name
            };
        }
    }
}
