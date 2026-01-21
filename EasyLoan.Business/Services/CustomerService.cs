using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.Dtos.Customer;
using EasyLoan.Models.Common.Enums;

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

        //public async Task<Guid> RegisterAsync(RegisterCustomerRequestDto dto)
        //{
        //    if (await _customerRepo.ExistsByEmailAsync(dto.Email))
        //        throw new ValidationException(ErrorMessages.EmailAlreadyExists);

        //    if (await _customerRepo.ExistsByPanAsync(dto.PanNumber))
        //        throw new ValidationException(ErrorMessages.PanAlreadyExists);

        //    if (dto.DateOfBirth > DateTime.UtcNow.AddYears(-18))
        //        throw new BusinessRuleViolationException("Customer must be at least 18 years old.");

        //    var customer = new Customer
        //    {
        //        Id = Guid.NewGuid(),
        //        Name = dto.Name,
        //        Email = dto.Email,
        //        PhoneNumber = dto.PhoneNumber,
        //        DateOfBirth = dto.DateOfBirth,
        //        AnnualSalary = dto.AnnualSalary,
        //        PanNumber = dto.PanNumber,
        //        Password = dto.Password,//TODO:Hash Password
        //        CreditScore = 800,
        //        CreatedDate = DateTime.UtcNow
        //    };

        //    await _customerRepo.AddAsync(customer);
        //    await _customerRepo.SaveChangesAsync();

        //    return customer.Id;
        //}

        //public async Task<string> LoginAsync(CustomerLoginRequestDto dto)
        //{
        //    var customer = await _customerRepo.GetByEmailAsync(dto.Email)
        //        ?? throw new AuthenticationFailedException(ErrorMessages.InvalidCredentials);

        //    if (dto.Password != customer.Password) //TODO : Comapre using hash library
        //        throw new AuthenticationFailedException(ErrorMessages.InvalidCredentials);

        //    //return _tokenService.GenerateCustomerToken(customer);//Generate token
        //    return "Created";
        //}

        public async Task<CustomerProfileResponseDto> GetProfileAsync(Guid customerId)
        {
            var c = await _customerRepo.GetByIdWithDetailsAsync(customerId)
                ?? throw new NotFoundException(ErrorMessages.CustomerNotFound);

            return new CustomerProfileResponseDto
            {
                Name = c.Name,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                DateOfBirth = c.DateOfBirth,
                CreditScore = c.CreditScore,
                AnnualSalary = c.AnnualSalary,
                PanNumber = c.PanNumber
            };
        }

        public async Task<CustomerProfileResponseDto> UpdateProfileAsync(Guid customerId, UpdateCustomerProfileRequestDto dto)
        {
            var customer = await _customerRepo.GetByIdWithDetailsAsync(customerId)
                ?? throw new NotFoundException(ErrorMessages.CustomerNotFound);

            customer.Name = dto.Name?.Trim() ?? customer.Name;
            customer.PhoneNumber = dto.PhoneNumber?.Trim() ?? customer.PhoneNumber;
            customer.AnnualSalary = dto.AnnualSalary;

            await _customerRepo.UpdateAsync(customer);
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

        //public async Task<CustomerDashboardResponseDto> GeFinancialOverviewAsync(Guid customerId)
        //{
        //    var customer = await _customerRepo.GetByIdAsync(customerId)
        //        ?? throw new NotFoundException(ErrorMessages.CustomerNotFound);

        //    var today = DateTime.UtcNow.Date;

        //    var pendingPayments = customer.Loans
        //        .Where(l => l.Status == LoanStatus.Active)
        //        .SelectMany(l => l.LoanPayments)
        //        .Count(p =>
        //            p.PaymentDate == null &&
        //            p.DueDate.Date <= today);

        //    return new CustomerDashboardResponseDto
        //    {
        //        CreditScore = customer.CreditScore,
        //        TotalNumberOfActiveLoans = customer.Loans.Count(l => l.Status == LoanStatus.Active),
        //        TotalNumberOfClosedLoans = customer.Loans.Count(l => l.Status == LoanStatus.Closed),
        //        TotalNumberOfPendingApplications = customer.LoanApplications.Count(a => a.Status == LoanApplicationStatus.Pending),
        //        TotalNumberOfApprovedApplications = customer.LoanApplications.Count(a => a.Status == LoanApplicationStatus.Approved),
        //        TotalNumberOfRejectedApplications = customer.LoanApplications.Count(a => a.Status == LoanApplicationStatus.Rejected)
        //    };
        //}
    }
}
