using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Helper;
using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.LoanApplication;
using EasyLoan.Models.Common.Enums;

namespace EasyLoan.Business.Services
{
    public class LoanApplicationService : ILoanApplicationService
    {
        private readonly ILoanApplicationRepository _loanApplicationrepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ILoanDetailsRepository _loanDetailsRepo;
        private readonly ILoanTypeRepository _loanTypeRepo;
        private readonly IPublicIdService _publicIdService;

        public LoanApplicationService(
            ILoanApplicationRepository repo,
            ICustomerRepository customerRepo,
            IEmployeeRepository employeeRepo,
            ILoanDetailsRepository loanDetailsRepo,
            ILoanTypeRepository loanTypeRepo,
            IPublicIdService publicIdService)
        {
            _loanApplicationrepo = repo;
            _customerRepo = customerRepo;
            _employeeRepo = employeeRepo;
            _loanDetailsRepo = loanDetailsRepo;
            _loanTypeRepo = loanTypeRepo;
            _publicIdService = publicIdService;
        }

        public async Task<string> CreateAsync(Guid customerId, CreateLoanApplicationRequestDto dto)
        {
            var customer = await _customerRepo.GetByIdAsync(customerId)
                ?? throw new NotFoundException(ErrorMessages.CustomerNotFound);

            if (customer.CreditScore <= 300)
                throw new BusinessRuleViolationException(ErrorMessages.CreditScoreTooLow);

            var loanType = await _loanTypeRepo.GetByIdAsync(dto.LoanTypeId) ?? throw new NotFoundException(ErrorMessages.LoanTypeNotFound);



            if (dto.RequestedTenureInMonths > loanType.MaxTenureInMonths)
                throw new BusinessRuleViolationException(ErrorMessages.ExceededMaxTenure);

            var managers = await _employeeRepo.GetAllAsync();

            var assignedEmployeeId = managers
                    .OrderBy(m => m.AssignedLoanApplications.Count)
                    .Select(m => m.Id)
                    .FirstOrDefault();

            if (assignedEmployeeId == Guid.Empty)
                throw new BusinessRuleViolationException(ErrorMessages.ManagersNotAvailable);

            var application = new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = _publicIdService.GenerateApplicationNumber(),
                CustomerId = customerId,
                LoanTypeId = dto.LoanTypeId,
                ApprovedAmount = 0,
                AssignedEmployeeId = assignedEmployeeId,
                RequestedAmount = dto.RequestedAmount,
                RequestedTenureInMonths = dto.RequestedTenureInMonths,
                Status = LoanApplicationStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };

            await _loanApplicationrepo.AddAsync(application);
            await _loanApplicationrepo.SaveChangesAsync();

            return application.ApplicationNumber;
        }

        public async Task UpdateReviewAsync(string applicationNumber,Guid managerId, ReviewLoanApplicationRequestDto dto)
        {
            var application = await _loanApplicationrepo.GetByApplicationNumberAsync(applicationNumber)
                ?? throw new KeyNotFoundException();
            if (application.AssignedEmployeeId != managerId)
                throw new ForbiddenException(ErrorMessages.AccessDenied);
            if (application.Status != LoanApplicationStatus.Pending)
                throw new BusinessRuleViolationException(ErrorMessages.LoanApplicationAlreadyReviewed);

            if (dto.IsApproved)
            {
                if (dto.ApprovedAmount > application.RequestedAmount)
                    throw new BusinessRuleViolationException(ErrorMessages.ApprovedAmountCannotExceedRequestedAmount);

                if(dto.ApprovedAmount < application.LoanType.MinAmount)
                    throw new BusinessRuleViolationException(ErrorMessages.BelowMinimumLoanAmount);

                application.Status = LoanApplicationStatus.Approved;
                application.ApprovedAmount = dto.ApprovedAmount;
                application.ManagerComments = dto.ManagerComments?.Trim();

                var newLoan = new LoanDetails()
                {
                    Id = Guid.NewGuid(),
                    Status = LoanStatus.Active,
                    ApprovedAmount = dto.ApprovedAmount,
                    LoanApplicationNumber = application.Id,
                    ApprovedByEmployeeId = application.AssignedEmployeeId,
                    CustomerId = application.CustomerId,
                    LoanNumber = _publicIdService.GenerateLoanNumber(),
                    InterestRate = application.LoanType.InterestRate,
                    LoanTypeId = application.LoanTypeId,
                    PrincipalRemaining = application.ApprovedAmount,
                    TenureInMonths = application.RequestedTenureInMonths,
                    CreatedDate = DateTime.UtcNow
                };

                var emiSchedule = EmiCalculator.GenerateSchedule(
                    principal: dto.ApprovedAmount,
                    annualInterestRate: application.LoanType.InterestRate,
                    tenureInMonths: application.RequestedTenureInMonths,
                    startDate: DateTime.UtcNow
                );

                foreach (var item in emiSchedule)
                {
                    newLoan.Emis.Add(new LoanEmi
                    {
                        Id = Guid.NewGuid(),
                        EmiNumber = item.EmiNumber,
                        DueDate = item.DueDate,
                        TotalAmount = item.TotalEmiAmount,
                        RemainingAmount = item.TotalEmiAmount
                    });
                }


                await _loanDetailsRepo.AddAsync(newLoan);
                
            }
            else
            {
                application.Status = LoanApplicationStatus.Rejected;
                application.ApprovedAmount = 0;
                application.ManagerComments = dto.ManagerComments;
            }


            await _loanApplicationrepo.UpdateAsync(application);
            await _loanDetailsRepo.SaveChangesAsync();
            await _loanApplicationrepo.SaveChangesAsync();
        }
        public async Task<LoanApplicationDetailsWithCustomerDataResponseDto> GetApplicationDetailsForReview(string applicationNumber, Guid managerId)
        {
            var application = await _loanApplicationrepo.GetByApplicationNumberAsync(applicationNumber)
                ?? throw new NotFoundException(ErrorMessages.LoanApplicationNotFound);

            if (application.AssignedEmployeeId != managerId)
                throw new ForbiddenException(ErrorMessages.AccessDenied);

            var customer = application.Customer;
            var loanType = application.LoanType;

            return new LoanApplicationDetailsWithCustomerDataResponseDto
            {
                ApplicationNumber = application.ApplicationNumber,
                CustomerName = customer.Name,
                AnnualSalaryOfCustomer = customer.AnnualSalary,
                PhoneNumber = customer.PhoneNumber,
                CreditScore = customer.CreditScore,
                DateOfBirth = customer.DateOfBirth,
                PanNumber = customer.PanNumber,
                LoanType = loanType.Name,
                RequestedAmount = application.RequestedAmount,
                AppprovedAmount = application.ApprovedAmount,
                InterestRate = loanType.InterestRate,
                Status = application.Status,
                ManagerComments = application.ManagerComments
            };
        }
        public async Task<List<LoanApplicationListItemResponseDto>> GetCustomerApplicationsAsync(Guid customerId)
        {
            var apps = await _loanApplicationrepo.GetByCustomerIdAsync(customerId) ?? throw new NotFoundException(ErrorMessages.LoanApplicationNotFound);

            if (apps.Any(a => a.CustomerId != customerId))
                throw new ForbiddenException(ErrorMessages.AccessDenied);

            return apps.Select(a => new LoanApplicationListItemResponseDto
            {
                ApplicationNumber = a.ApplicationNumber,
                LoanTypeName = a.LoanType.Name,
                RequestedAmount = a.RequestedAmount,
                TenureInMonths = a.RequestedTenureInMonths,
                Status = a.Status,
                CreatedDate = a.CreatedDate
            }).ToList();
        }

        public async Task<LoanApplicationDetailsResponseDto> GetByApplicationNumberAsync(string applicationNumber)
        {
            var application = await _loanApplicationrepo.GetByApplicationNumberAsync(applicationNumber)
                ?? throw new NotFoundException(ErrorMessages.LoanApplicationNotFound);

            return new LoanApplicationDetailsResponseDto
            {
                ApplicationNumber = application.ApplicationNumber,
                CustomerName = application.Customer.Name,
                LoanType = application.LoanType.Name,
                RequestedAmount = application.RequestedAmount,
                InterestRate = application.LoanType.InterestRate,
                AppprovedAmount = application.ApprovedAmount,
                AssignedEmployeeId = application.AssignedEmployeeId,
                RequestedTenureInMonths = application.RequestedTenureInMonths,
                Status = application.Status,
                ManagerComments = application.ManagerComments
            };
        }

        //public async Task<List<LoanApplicationListItemResponseDto>> GetPendingAsync()
        //{
        //    var applications = await _loanApplicationrepo.GetAllAsync();

        //    return applications
        //        .Where(a => a.Status == LoanApplicationStatus.Pending)
        //        .Select(a => new LoanApplicationListItemResponseDto
        //        {
        //            ApplicationNumber = a.ApplicationNumber,
        //            LoanTypeName = a.LoanType.Name,
        //            RequestedAmount = a.RequestedAmount,
        //            Status = a.Status.ToString(),
        //            CreatedDate = a.CreatedDate
        //        })
        //        .ToList();
        //}

        public async Task<List<LoanApplicationListItemResponseForAdminDto>> GetAllPendingApplicationsAsync()
        {
            var apps = await _loanApplicationrepo.GetAllAsync();

            return apps.Where(a => a.Status == LoanApplicationStatus.Pending).Select(a => new LoanApplicationListItemResponseForAdminDto
            {
                ApplicationNumber = a.ApplicationNumber,
                AssignedEmployeeId = a.AssignedEmployeeId,
                TenureInMonths = a.RequestedTenureInMonths,
                LoanTypeName = a.LoanType.Name,
                RequestedAmount = a.RequestedAmount,
                Status = a.Status,
                CreatedDate = a.CreatedDate
            }).ToList();
        }

        public async Task<List<LoanApplicationListItemResponseDto>> GetAssignedApplicationsAsync(Guid assignedManagerId){
            var applications = await _loanApplicationrepo.GetAllAsync();

            if (!applications.Any())
                throw new NotFoundException(ErrorMessages.LoanApplicationNotFound);
            return applications.Where(a => a.AssignedEmployeeId == assignedManagerId).
                Select(a => new LoanApplicationListItemResponseDto
            {
                TenureInMonths = a.RequestedTenureInMonths,
                ApplicationNumber = a.ApplicationNumber,
                LoanTypeName = a.LoanType.Name,
                RequestedAmount = a.RequestedAmount,
                Status = a.Status,
                CreatedDate = a.CreatedDate
            }).ToList();
        }
    }
}

