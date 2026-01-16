using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<Guid> RegisterAsync(RegisterCustomerRequestDto dto)
        {
            if (await _customerRepo.ExistsByEmailAsync(dto.Email))
                throw new InvalidOperationException("Email already exists");

            if (await _customerRepo.ExistsByPanAsync(dto.PanNumber))
                throw new InvalidOperationException("PAN already exists");

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                AnnualSalary = dto.AnnualSalary,
                PanNumber = dto.PanNumber,
                Password = dto.Password,//TODO:Hash Password
                CreditScore = 800,
                CreatedDate = DateTime.UtcNow
            };

            await _customerRepo.AddAsync(customer);
            await _customerRepo.SaveChangesAsync();

            return customer.Id;
        }

        public async Task<string> LoginAsync(CustomerLoginRequestDto dto)
        {
            var customer = await _customerRepo.GetByEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException("Invalid credentials");

            if (dto.Password != customer.Password) //TODO : Comapre using hash library
                throw new UnauthorizedAccessException("Invalid credentials");

            //return _tokenService.GenerateCustomerToken(customer);//Generate token
            return "Created";
        }

        public async Task<CustomerProfileResponseDto> GetProfileAsync(Guid customerId)
        {
            var c = await _customerRepo.GetByIdAsync(customerId)
                ?? throw new KeyNotFoundException();

            return new CustomerProfileResponseDto
            {
                CustomerId = c.Id,
                Name = c.Name,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                CreditScore = c.CreditScore,
                AnnualSalary = c.AnnualSalary,
                PanNumber = c.PanNumber
            };
        }

        public async Task UpdateProfileAsync(Guid customerId, UpdateCustomerProfileRequestDto dto)
        {
            var customer = await _customerRepo.GetByIdAsync(customerId)
                ?? throw new KeyNotFoundException();

            customer.Name = dto.Name;
            customer.PhoneNumber = dto.PhoneNumber;
            customer.AnnualSalary = dto.AnnualSalary;

            await _customerRepo.UpdateAsync(customer);
            await _customerRepo.SaveChangesAsync();
        }

        public async Task<CustomerDashboardResponseDto> GetDashboardAsync(Guid customerId)
        {
            var customer = await _customerRepo.GetByIdAsync(customerId)
                ?? throw new KeyNotFoundException();

            var today = DateTime.UtcNow.Date;

            var pendingPayments = customer.Loans
                .Where(l => l.Status == LoanStatus.Active)
                .SelectMany(l => l.LoanPayments)
                .Count(p =>
                    p.PaymentDate == null &&
                    p.DueDate.Date <= today);

            return new CustomerDashboardResponseDto
            {
                CreditScore = customer.CreditScore,
                TotalNumberOfActiveLoans = customer.Loans.Count(l => l.Status == LoanStatus.Active),
                TotalNumberOfClosedLoans = customer.Loans.Count(l => l.Status == LoanStatus.Closed),
                TotalNumberOfPendingApplications = customer.LoanApplications.Count(a => a.Status == LoanApplicationStatus.Pending),
                TotalNumberOfApprovedApplications = customer.LoanApplications.Count(a => a.Status == LoanApplicationStatus.Approved),
                TotalNumberOfRejectedApplications = customer.LoanApplications.Count(a => a.Status == LoanApplicationStatus.Rejected),
                NumberOfPendingPayments = pendingPayments
            };
        }
    }
}
