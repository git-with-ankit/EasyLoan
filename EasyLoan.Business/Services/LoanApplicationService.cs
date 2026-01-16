using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.LoanApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Services
{
    public class LoanApplicationService : ILoanApplicationService
    {
        private readonly ILoanApplicationRepository _loanApplicationrepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ILoanDetailsRepository _loanDetailsRepo;

        public LoanApplicationService(
            ILoanApplicationRepository repo,
            ICustomerRepository customerRepo,
            IEmployeeRepository employeeRepo,
            ILoanDetailsRepository loanDetailsRepo)
        {
            _loanApplicationrepo = repo;
            _customerRepo = customerRepo;
            _employeeRepo = employeeRepo;
            _loanDetailsRepo = loanDetailsRepo;
        }

        public async Task<string> CreateAsync(CreateLoanApplicationRequestDto dto)
        {
            var customer = await _customerRepo.GetByIdAsync(dto.CustomerId)
                ?? throw new KeyNotFoundException();

            if (customer.CreditScore <= 300)
                throw new InvalidOperationException("Credit score too low");

            var managers = await _employeeRepo.GetAllAsync();

            var assignedEmployeeId = managers
                    .OrderBy(m => m.AssignedLoanApplications.Count)
                    .Select(m => m.Id)
                    .FirstOrDefault();

            if (assignedEmployeeId == Guid.Empty)
                throw new InvalidOperationException("No managers available");

            var application = new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicationId = $"{dto.CustomerId}_{DateTime.UtcNow:yyyyMMddHHmmss}",
                CustomerId = dto.CustomerId,
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

            return application.ApplicationId;
        }

        public async Task UpdateReviewAsync(string applicationId, ReviewLoanApplicationRequestDto dto)
        {
            var application = await _loanApplicationrepo.GetByApplicationIdAsync(applicationId)
                ?? throw new KeyNotFoundException();

            if(dto.IsApproved)
            {
                
                application.Status = LoanApplicationStatus.Approved;
                application.ApprovedAmount = dto.ApprovedAmount;
                application.ManagerComments = dto.ManagerComments;

                var newLoan = new LoanDetails()
                {
                    Id = new Guid(),
                    Status = LoanStatus.Active,
                    ApprovedAmount = dto.ApprovedAmount,
                    ApprovedByEmployeeId = application.AssignedEmployeeId,
                    CustomerId = application.CustomerId,
                    InterestRate = application.InterestRate,
                    LoanTypeId = application.LoanTypeId,
                    PrincipalRemaining = application.ApprovedAmount,
                    TenureInMonths = application.RequestedTenureInMonths,
                    CreatedDate = DateTime.UtcNow
                };

                await _loanDetailsRepo.AddAsync(newLoan);
                await _loanDetailsRepo.SaveChangesAsync();
            }
            else
            {
                application.Status = LoanApplicationStatus.Rejected;
                application.ApprovedAmount = 0;
                application.ManagerComments = dto.ManagerComments;
            }


            await _loanApplicationrepo.UpdateAsync(application);
            await _loanApplicationrepo.SaveChangesAsync();
        }
        public async Task<LoanApplicationDetailsWithCustomerDataResponseDto> GetApplicationDetailsForReview(string applicationId)
        {
            var application = await _loanApplicationrepo.GetByApplicationIdAsync(applicationId)
                ?? throw new KeyNotFoundException("Loan application not found");

            var customer = application.Customer;
            var loanType = application.LoanType;

            return new LoanApplicationDetailsWithCustomerDataResponseDto
            {
                ApplicationId = application.ApplicationId,
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
                Status = application.Status.ToString(),
                ManagerComments = application.ManagerComments
            };
        }
        public async Task<List<LoanApplicationListItemResponseDto>> GetCustomerApplicationsAsync(Guid customerId)
        {
            var apps = await _loanApplicationrepo.GetByCustomerIdAsync(customerId);

            return apps.Select(a => new LoanApplicationListItemResponseDto
            {
                ApplicationId = a.ApplicationId,
                LoanTypeName = a.LoanType.Name,
                RequestedAmount = a.RequestedAmount,
                Status = a.Status.ToString(),
                CreatedDate = a.CreatedDate
            }).ToList();
        }

        public async Task<LoanApplicationDetailsResponseDto> GetByApplicationIdAsync(string applicationId)
        {
            var application = await _loanApplicationrepo.GetByApplicationIdAsync(applicationId)
                ?? throw new KeyNotFoundException("Loan application not found");

            return new LoanApplicationDetailsResponseDto
            {//TODO : EMI Plan
                ApplicationId = application.ApplicationId,
                CustomerName = application.Customer.Name,
                LoanType = application.LoanType.Name,
                RequestedAmount = application.RequestedAmount,
                InterestRate = application.InterestRate,
                AppprovedAmount = application.ApprovedAmount,
                RequestedTenureInMonths = application.RequestedTenureInMonths,
                Status = application.Status.ToString(),
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
        //            ApplicationId = a.ApplicationId,
        //            LoanTypeName = a.LoanType.Name,
        //            RequestedAmount = a.RequestedAmount,
        //            Status = a.Status.ToString(),
        //            CreatedDate = a.CreatedDate
        //        })
        //        .ToList();
        //}

        public async Task<List<LoanApplicationListItemResponseDto>> GetAllAsync()
        {
            var apps = await _loanApplicationrepo.GetAllAsync();

            return apps.Select(a => new LoanApplicationListItemResponseDto
            {
                ApplicationId = a.ApplicationId,
                AssignedEmployeeId = a.AssignedEmployeeId,
                TenureInMonths = a.RequestedTenureInMonths,
                LoanTypeName = a.LoanType.Name,
                RequestedAmount = a.RequestedAmount,
                Status = a.Status.ToString(),
                CreatedDate = a.CreatedDate
            }).ToList();
        }

        public async Task<List<LoanApplicationListItemResponseDto>> GetAssignedApplicationsAsync(Guid assignedManagerId){
            var applications = await _loanApplicationrepo.GetAllAsync();

            return applications.Where(a => a.AssignedEmployeeId == assignedManagerId).
                Select(a => new LoanApplicationListItemResponseDto
            {
                TenureInMonths = a.RequestedTenureInMonths,
                ApplicationId = a.ApplicationId,
                LoanTypeName = a.LoanType.Name,
                RequestedAmount = a.RequestedAmount,
                Status = a.Status.ToString(),
                CreatedDate = a.CreatedDate
            }).ToList();
        }
    }
}

