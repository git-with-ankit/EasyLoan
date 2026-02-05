//Business rules , Exceptions , Decision logic , Correct repository interactions

;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Business.Services;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.LoanApplication;
using EasyLoan.Dtos.LoanType;
using EasyLoan.Models.Common.Enums;
using Moq;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    public class LoanApplicationServiceTests
    {
        private Mock<ILoanApplicationRepository> _loanAppRepo = null!;
        private Mock<ICustomerRepository> _customerRepo = null!;
        private Mock<IEmployeeRepository> _employeeRepo = null!;
        private Mock<ILoanDetailsRepository> _loanDetailsRepo = null!;
        private Mock<ILoanTypeRepository> _loanTypeRepo = null!;
        private Mock<IPublicIdService> _publicIdService = null!;
        private Mock<IEmiCalculatorService> _emiCalculator = null!;

        private LoanApplicationService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _loanAppRepo = new Mock<ILoanApplicationRepository>();
            _customerRepo = new Mock<ICustomerRepository>();
            _employeeRepo = new Mock<IEmployeeRepository>();
            _loanDetailsRepo = new Mock<ILoanDetailsRepository>();
            _loanTypeRepo = new Mock<ILoanTypeRepository>();
            _publicIdService = new Mock<IPublicIdService>();
            _emiCalculator = new Mock<IEmiCalculatorService>();

            _service = new LoanApplicationService(
                _loanAppRepo.Object,
                _customerRepo.Object,
                _employeeRepo.Object,
                _loanDetailsRepo.Object,
                _loanTypeRepo.Object,
                _publicIdService.Object,
                _emiCalculator.Object
                );
        }

        [TestMethod]
        public async Task CreateAsync_CustomerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var dto = new CreateLoanApplicationRequestDto
            {
                LoanTypeId = Guid.NewGuid(),
                RequestedAmount = 500000,
                RequestedTenureInMonths = 240
            };

            _customerRepo
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.CreateAsync(customerId, dto));
        }
        [TestMethod]
        public async Task CreateAsync_LowCreditScore_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                CreditScore = 300
            };

            var dto = new CreateLoanApplicationRequestDto
            {
                LoanTypeId = Guid.NewGuid(),
                RequestedAmount = 500000,
                RequestedTenureInMonths = 240
            };

            _customerRepo
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.CreateAsync(customerId, dto));
        }
        [TestMethod]
        public async Task CreateAsync_LoanTypeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                CreditScore = 700
            };

            var dto = new CreateLoanApplicationRequestDto
            {
                LoanTypeId = Guid.NewGuid(),
                RequestedAmount = 500000,
                RequestedTenureInMonths = 240
            };

            _customerRepo.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);
            _loanTypeRepo.Setup(r => r.GetByIdAsync(dto.LoanTypeId)).ReturnsAsync((LoanType?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.CreateAsync(customerId, dto));
        }
        [TestMethod]
        public async Task CreateAsync_TenureExceedsMax_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _customerRepo.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(new Customer { Id = customerId, CreditScore = 700 });

            var loanType = new LoanType { Id = Guid.NewGuid(), MaxTenureInMonths = 120 };

            var dto = new CreateLoanApplicationRequestDto
            {
                LoanTypeId = loanType.Id,
                RequestedAmount = 500000,
                RequestedTenureInMonths = 240
            };

            _loanTypeRepo.Setup(r => r.GetByIdAsync(loanType.Id)).ReturnsAsync(loanType);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.CreateAsync(customerId, dto));
        }
        [TestMethod]
        public async Task CreateAsync_NoManagersAvailable_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _customerRepo.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(new Customer { Id = customerId, CreditScore = 700 });

            _loanTypeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new LoanType { MaxTenureInMonths = 300 });

            _employeeRepo.Setup(r => r.GetAllWithDetailsAsync())
                .ReturnsAsync(new List<Employee>());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.CreateAsync(customerId, new CreateLoanApplicationRequestDto
                {
                    LoanTypeId = Guid.NewGuid(),
                    RequestedAmount = 500000,
                    RequestedTenureInMonths = 120
                }));
        }
        [TestMethod]
        public async Task CreateAsync_ValidRequest_CreatesApplicationSuccessfully()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var loanTypeId = Guid.NewGuid();
            var managerId = Guid.NewGuid();

            _customerRepo.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(new Customer { Id = customerId, CreditScore = 750 });

            _loanTypeRepo.Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync(new LoanType { Id = loanTypeId, MaxTenureInMonths = 240 });

            _employeeRepo.Setup(r => r.GetAllWithDetailsAsync())
                .ReturnsAsync(new List<Employee>
                {
            new Employee
            {
                Id = managerId,
                AssignedLoanApplications = new List<LoanApplication>()
            }
                });

            _publicIdService.Setup(s => s.GenerateApplicationNumber())
                .Returns("LA-ABCDEFGH");

            var dto = new CreateLoanApplicationRequestDto
            {
                LoanTypeId = loanTypeId,
                RequestedAmount = 500000,
                RequestedTenureInMonths = 120
            };

            // Act
            var result = await _service.CreateAsync(customerId, dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("LA-ABCDEFGH", result.ApplicationNumber);

            _loanAppRepo.Verify(r => r.AddAsync(It.IsAny<LoanApplication>()), Times.Once);
            _loanAppRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        [TestMethod]
        public async Task UpdateReviewAsync_InvalidApplicationNumber_ThrowsBusinessRuleViolation()
        {
            var dto = new ReviewLoanApplicationRequestDto
            {
                IsApproved = true,
                ApprovedAmount = 10000
            };

            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.UpdateReviewAsync("INVALID", Guid.NewGuid(), dto));
        }
        [TestMethod]
        public async Task UpdateReviewAsync_ApplicationNotFound_ThrowsKeyNotFound()
        {
            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((LoanApplication?)null);

            var dto = new ReviewLoanApplicationRequestDto
            {
                IsApproved = true,
                ApprovedAmount = 10000
            };

            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() =>
                _service.UpdateReviewAsync("LA-ABCDEFGH", Guid.NewGuid(), dto));
        }

        [TestMethod]
        public async Task UpdateReviewAsync_ManagerNotAssigned_ThrowsForbidden()
        {
            var application = new LoanApplication
            {
                ApplicationNumber = "LA-ABCDEFGH",
                AssignedEmployeeId = Guid.NewGuid(),
                Status = LoanApplicationStatus.Pending,
                LoanType = new LoanType { MinAmount = 5000 }
            };

            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(application.ApplicationNumber))
                .ReturnsAsync(application);

            var dto = new ReviewLoanApplicationRequestDto
            {
                IsApproved = true,
                ApprovedAmount = 10000
            };

            await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.UpdateReviewAsync(application.ApplicationNumber, Guid.NewGuid(), dto));
        }
        [TestMethod]
        public async Task UpdateReviewAsync_AlreadyReviewed_ThrowsBusinessRuleViolation()
        {
            var managerId = Guid.NewGuid();

            var application = new LoanApplication
            {
                ApplicationNumber = "LA-ABCDEFGH",
                AssignedEmployeeId = managerId,
                Status = LoanApplicationStatus.Approved,
                LoanType = new LoanType { MinAmount = 5000 }
            };

            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(application.ApplicationNumber))
                .ReturnsAsync(application);

            var dto = new ReviewLoanApplicationRequestDto
            {
                IsApproved = true,
                ApprovedAmount = 10000
            };

            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.UpdateReviewAsync(application.ApplicationNumber, managerId, dto));
        }
        [TestMethod]
        public async Task UpdateReviewAsync_ApprovedAmountExceedsRequested_ThrowsBusinessRuleViolation()
        {
            var managerId = Guid.NewGuid();

            var application = new LoanApplication
            {
                ApplicationNumber = "LA-ABCDEFGH",
                AssignedEmployeeId = managerId,
                Status = LoanApplicationStatus.Pending,
                RequestedAmount = 50000,
                LoanType = new LoanType { MinAmount = 10000 }
            };

            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(application.ApplicationNumber))
                .ReturnsAsync(application);

            var dto = new ReviewLoanApplicationRequestDto
            {
                IsApproved = true,
                ApprovedAmount = 60000
            };

            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.UpdateReviewAsync(application.ApplicationNumber, managerId, dto));
        }
        [TestMethod]
        public async Task UpdateReviewAsync_ApprovedAmountBelowMin_ThrowsBusinessRuleViolation()
        {
            var managerId = Guid.NewGuid();

            var application = new LoanApplication
            {
                ApplicationNumber = "LA-ABCDEFGH",
                AssignedEmployeeId = managerId,
                Status = LoanApplicationStatus.Pending,
                RequestedAmount = 50000,
                LoanType = new LoanType { MinAmount = 20000 }
            };

            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(application.ApplicationNumber))
                .ReturnsAsync(application);

            var dto = new ReviewLoanApplicationRequestDto
            {
                IsApproved = true,
                ApprovedAmount = 10000
            };

            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.UpdateReviewAsync(application.ApplicationNumber, managerId, dto));
        }
        [TestMethod]
        public async Task UpdateReviewAsync_Approved_CreatesLoanAndReturnsApprovedResponse()
        {
            var managerId = Guid.NewGuid();

            var application = new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = "LA-ABCDEFGH",
                AssignedEmployeeId = managerId,
                CustomerId = Guid.NewGuid(),
                Status = LoanApplicationStatus.Pending,
                RequestedAmount = 50000,
                RequestedTenureInMonths = 12,
                LoanTypeId = Guid.NewGuid(),
                LoanType = new LoanType
                {
                    MinAmount = 10000,
                    InterestRate = 12
                }
            };

            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(application.ApplicationNumber))
                .ReturnsAsync(application);

            _publicIdService
                .Setup(s => s.GenerateLoanNumber())
                .Returns("LN-12345678");

            _emiCalculator
                .Setup(e => e.GenerateSchedule(
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<int>(),
                    It.IsAny<DateTime>()))
                .Returns(new List<EmiScheduleItemResponseDto>
                {
            new() { EmiNumber = 1, TotalEmiAmount = 5000 },
            new() { EmiNumber = 2, TotalEmiAmount = 5000 }
                });

            var dto = new ReviewLoanApplicationRequestDto
            {
                IsApproved = true,
                ApprovedAmount = 30000,
                ManagerComments = "Approved"
            };

            var result =
                await _service.UpdateReviewAsync(application.ApplicationNumber, managerId, dto);

            Assert.AreEqual(LoanApplicationStatus.Approved, result.Status);
            Assert.AreEqual(30000, result.ApprovedAmount);

            _loanDetailsRepo.Verify(r => r.AddAsync(It.IsAny<LoanDetails>()), Times.Once);
            _loanDetailsRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _loanAppRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        [TestMethod]
        public async Task UpdateReviewAsync_Rejected_DoesNotCreateLoan()
        {
            var managerId = Guid.NewGuid();

            var application = new LoanApplication
            {
                ApplicationNumber = "LA-ABCDEFGH",
                AssignedEmployeeId = managerId,
                Status = LoanApplicationStatus.Pending,
                LoanType = new LoanType { MinAmount = 5000 }
            };

            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(application.ApplicationNumber))
                .ReturnsAsync(application);

            var dto = new ReviewLoanApplicationRequestDto
            {
                IsApproved = false,
                ManagerComments = "Rejected"
            };

            var result =
                await _service.UpdateReviewAsync(application.ApplicationNumber, managerId, dto);

            Assert.AreEqual(LoanApplicationStatus.Rejected, result.Status);

            _loanDetailsRepo.Verify(r => r.AddAsync(It.IsAny<LoanDetails>()), Times.Never);
        }
        [TestMethod]
        public async Task GetApplicationDetailsForReview_InvalidApplicationNumber_ThrowsBusinessRuleViolation()
        {
            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.GetApplicationDetailsForReview("INVALID", Guid.NewGuid()));
        }
        [TestMethod]
        public async Task GetApplicationDetailsForReview_ApplicationNotFound_ThrowsNotFoundException()
        {
            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((LoanApplication?)null);

            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetApplicationDetailsForReview("LA-ABCDEFGH", Guid.NewGuid()));
        }
        //[TestMethod]
        //public async Task GetApplicationDetailsForReview_ManagerNotAssigned_ThrowsForbidden()
        //{
        //    var application = new LoanApplication
        //    {
        //        ApplicationNumber = "LA-ABCDEFGH",
        //        AssignedEmployeeId = Guid.NewGuid()
        //    };

        //    _loanAppRepo
        //        .Setup(r => r.GetByApplicationNumberWithDetailsAsync(application.ApplicationNumber))
        //        .ReturnsAsync(application);

        //    await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
        //        _service.GetApplicationDetailsForReview(
        //            application.ApplicationNumber,
        //            Guid.NewGuid()));
        //}
        [TestMethod]
        public async Task GetApplicationDetailsForReview_ValidRequest_ReturnsCorrectDetails()
        {
            // Arrange
            var managerId = Guid.NewGuid();

            var customer = new Customer
            {
                Name = "Ravi Kumar",
                AnnualSalary = 1200000,
                PhoneNumber = "9876543210",
                CreditScore = 780,
                DateOfBirth = new DateTime(1990, 5, 10),
                PanNumber = "ABCDE1234F",
                Loans = new List<LoanDetails>
        {
            new() { Status = LoanStatus.Active },
            new() { Status = LoanStatus.Closed }
        }
            };

            var loanType = new LoanType
            {
                Name = "Home Loan",
                InterestRate = 8.5m
            };

            var application = new LoanApplication
            {
                ApplicationNumber = "LA-ABCDEFGH",
                AssignedEmployeeId = managerId,
                RequestedAmount = 500000,
                ApprovedAmount = 450000,
                Status = LoanApplicationStatus.Pending,
                ManagerComments = "Under review",
                Customer = customer,
                LoanType = loanType
            };

            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(application.ApplicationNumber))
                .ReturnsAsync(application);

            // Act
            var result =
                await _service.GetApplicationDetailsForReview(application.ApplicationNumber, managerId);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(application.ApplicationNumber, result.ApplicationNumber);
            Assert.AreEqual(customer.Name, result.CustomerName);
            Assert.AreEqual(customer.AnnualSalary, result.AnnualSalaryOfCustomer);
            Assert.AreEqual(customer.PhoneNumber, result.PhoneNumber);
            Assert.AreEqual(customer.CreditScore, result.CreditScore);
            Assert.AreEqual(customer.DateOfBirth, result.DateOfBirth);
            Assert.AreEqual(customer.PanNumber, result.PanNumber);
            Assert.AreEqual(loanType.Name, result.LoanType);
            Assert.AreEqual(application.RequestedAmount, result.RequestedAmount);
            //Assert.AreEqual(application.ApprovedAmount, result.AppprovedAmount);
            Assert.AreEqual(loanType.InterestRate, result.InterestRate);
            Assert.AreEqual(application.Status, result.Status);
            Assert.AreEqual(application.ManagerComments, result.ManagerComments);
            Assert.AreEqual(1, result.TotalOngoingLoans);
        }
        [TestMethod]
        public async Task GetByApplicationNumberAsync_InvalidFormat_ThrowsBusinessRuleViolation()
        {
            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.GetByApplicationNumberAsync("INVALID"));
        }
        [TestMethod]
        public async Task GetByApplicationNumberAsync_NotFound_ThrowsNotFoundException()
        {
            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((LoanApplication?)null);

            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetByApplicationNumberAsync("LA-ABCDEFGH"));
        }
        [TestMethod]
        public async Task GetByApplicationNumberAsync_ValidApplication_ReturnsMappedDetails()
        {
            // Arrange
            var application = new LoanApplication
            {
                ApplicationNumber = "LA-ABCDEFGH",
                RequestedAmount = 500000,
                ApprovedAmount = 450000,
                RequestedTenureInMonths = 240,
                AssignedEmployeeId = Guid.NewGuid(),
                Status = LoanApplicationStatus.Approved,
                ManagerComments = "Approved",

                Customer = new Customer
                {
                    Name = "Ankit Sharma"
                },

                LoanType = new LoanType
                {
                    Name = "Home Loan",
                    InterestRate = 8.75m
                }
            };

            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(application.ApplicationNumber))
                .ReturnsAsync(application);

            // Act
            var result =
                await _service.GetByApplicationNumberAsync(application.ApplicationNumber);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(application.ApplicationNumber, result.ApplicationNumber);
            Assert.AreEqual(application.Customer.Name, result.CustomerName);
            Assert.AreEqual(application.LoanType.Name, result.LoanType);
            Assert.AreEqual(application.RequestedAmount, result.RequestedAmount);
            Assert.AreEqual(application.LoanType.InterestRate, result.InterestRate);
            //Assert.AreEqual(application.ApprovedAmount, result.AppprovedAmount);
            Assert.AreEqual(application.AssignedEmployeeId, result.AssignedEmployeeId);
            Assert.AreEqual(application.RequestedTenureInMonths, result.RequestedTenureInMonths);
            Assert.AreEqual(application.Status, result.Status);
            Assert.AreEqual(application.ManagerComments, result.ManagerComments);
        }
        [TestMethod]
        public async Task GetApplicationsAsync_Admin_ReturnsFilteredApplications()
        {
            var apps = new List<LoanApplication>
    {
        new()
        {
            ApplicationNumber = "LA-1",
            Status = LoanApplicationStatus.Pending,
            RequestedAmount = 100000,
            RequestedTenureInMonths = 120,
            CreatedDate = DateTime.UtcNow,
            LoanType = new LoanType { Name = "Home Loan" }
        },
        new()
        {
            ApplicationNumber = "LA-2",
            Status = LoanApplicationStatus.Approved,
            RequestedAmount = 200000,
            RequestedTenureInMonths = 240,
            CreatedDate = DateTime.UtcNow,
            LoanType = new LoanType { Name = "Car Loan" }
        }
    };

            _loanAppRepo
                .Setup(r => r.GetAllWithDetailsAsync())
                .ReturnsAsync(apps);

            var result = await _service.GetApplicationsAsync(
                Guid.NewGuid(),
                Role.Admin,
                LoanApplicationStatus.Pending);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("LA-1", result.First().ApplicationNumber);
        }
        [TestMethod]
        public async Task GetApplicationsAsync_Admin_NoApplications_ThrowsNotFound()
        {
            _loanAppRepo
                .Setup(r => r.GetAllWithDetailsAsync())
                .ReturnsAsync(new List<LoanApplication>());

            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetApplicationsAsync(
                    Guid.NewGuid(),
                    Role.Admin,
                    LoanApplicationStatus.Pending));
        }
        [TestMethod]
        public async Task GetApplicationsAsync_Manager_ReturnsOnlyAssignedApplications()
        {
            var managerId = Guid.NewGuid();

            var apps = new List<LoanApplication>
    {
        new()
        {
            ApplicationNumber = "LA-1",
            AssignedEmployeeId = managerId,
            Status = LoanApplicationStatus.Pending,
            RequestedTenureInMonths = 120,
            CreatedDate = DateTime.UtcNow,
            LoanType = new LoanType { Name = "Home Loan" }
        },
        new()
        {
            ApplicationNumber = "LA-2",
            AssignedEmployeeId = Guid.NewGuid(),
            Status = LoanApplicationStatus.Pending,
            RequestedTenureInMonths = 240,
            CreatedDate = DateTime.UtcNow,
            LoanType = new LoanType { Name = "Car Loan" }
        }
    };

            _loanAppRepo
                .Setup(r => r.GetAllWithDetailsAsync())
                .ReturnsAsync(apps);

            var result = await _service.GetApplicationsAsync(
                managerId,
                Role.Manager,
                LoanApplicationStatus.Pending);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("LA-1", result.First().ApplicationNumber);
        }
        [TestMethod]
        public async Task GetApplicationsAsync_Customer_ReturnsOwnApplications()
        {
            var customerId = Guid.NewGuid();

            var apps = new List<LoanApplication>
    {
        new()
        {
            CustomerId = customerId,
            ApplicationNumber = "LA-1",
            Status = LoanApplicationStatus.Pending,
            RequestedTenureInMonths = 120,
            CreatedDate = DateTime.UtcNow,
            LoanType = new LoanType { Name = "Home Loan" }
        }
    };

            _loanAppRepo
                .Setup(r => r.GetByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(apps);

            var result = await _service.GetApplicationsAsync(
                customerId,
                Role.Customer,
                LoanApplicationStatus.Pending);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("LA-1", result.First().ApplicationNumber);
        }

        [TestMethod]
        public async Task GetApplicationsForCustomerAsync_WhenNoApplications_ThrowsNotFoundException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var status = LoanApplicationStatus.Pending;

            _loanAppRepo
                .Setup(r => r.GetByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(Enumerable.Empty<LoanApplication>());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetApplicationsAsync(
                    customerId,
                    Role.Customer,
                    status));
        }
        [TestMethod]
        public async Task GetApplicationsAsync_Customer_OtherCustomerApplication_ThrowsForbidden()
        {
            var customerId = Guid.NewGuid();

            var apps = new List<LoanApplication>
    {
        new()
        {
            CustomerId = Guid.NewGuid(), // NOT the same
            ApplicationNumber = "LA-1",
            Status = LoanApplicationStatus.Pending,
            LoanType = new LoanType { Name = "Home Loan" }
        }
    };

            _loanAppRepo
                .Setup(r => r.GetByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(apps);

            await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.GetApplicationsAsync(
                    customerId,
                    Role.Customer,
                    LoanApplicationStatus.Pending));
        }
        [TestMethod]
        public async Task GetApplicationsAsync_InvalidRole_ThrowsForbidden()
        {
            await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.GetApplicationsAsync(
                    Guid.NewGuid(),
                    (Role)999,
                    LoanApplicationStatus.Pending));
        }


    }

}
