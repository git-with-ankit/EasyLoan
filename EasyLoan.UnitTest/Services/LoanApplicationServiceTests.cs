using EasyLoan.Business.Constants;
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
                RequestedTenureInMonths = 120
            };

            _customerRepo
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.CreateAsync(customerId, dto));

            Assert.AreEqual(ErrorMessages.CustomerNotFound, ex.Message);
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
                RequestedTenureInMonths = 120
            };

            _customerRepo
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.CreateAsync(customerId, dto));

            Assert.AreEqual(ErrorMessages.CreditScoreTooLow, ex.Message);
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
                RequestedTenureInMonths = 120
            };

            _customerRepo
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _loanTypeRepo
                .Setup(r => r.GetByIdAsync(dto.LoanTypeId))
                .ReturnsAsync((LoanType?)null);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.CreateAsync(customerId, dto));

            Assert.AreEqual(ErrorMessages.LoanTypeNotFound, ex.Message);
        }

        [TestMethod]
        public async Task CreateAsync_RequestedAmountExceedsMaximum_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var loanTypeId = Guid.NewGuid();

            _customerRepo.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(new Customer { Id = customerId, CreditScore = 700 });

            _loanTypeRepo.Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync(new LoanType { Id = loanTypeId, MaxTenureInMonths = 240 });

            var dto = new CreateLoanApplicationRequestDto
            {
                LoanTypeId = loanTypeId,
                RequestedAmount = BusinessConstants.MaximumLoanAmount + 1,
                RequestedTenureInMonths = 120
            };

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.CreateAsync(customerId, dto));

            Assert.AreEqual(ErrorMessages.ExceededMaxAmount, ex.Message);
        }

        [TestMethod]
        public async Task CreateAsync_TenureExceedsMax_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var loanTypeId = Guid.NewGuid();

            _customerRepo.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(new Customer { Id = customerId, CreditScore = 700 });

            var loanType = new LoanType
            {
                Id = loanTypeId,
                MaxTenureInMonths = 120
            };

            _loanTypeRepo.Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync(loanType);

            var dto = new CreateLoanApplicationRequestDto
            {
                LoanTypeId = loanTypeId,
                RequestedAmount = 500000,
                RequestedTenureInMonths = 240
            };

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.CreateAsync(customerId, dto));

            Assert.AreEqual(ErrorMessages.ExceededMaxTenure, ex.Message);
        }

        [TestMethod]
        public async Task CreateAsync_WhenRateLimitingTriggered_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var loanTypeId = Guid.NewGuid();

            _customerRepo.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(new Customer { Id = customerId, CreditScore = 700 });

            _loanTypeRepo.Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync(new LoanType { Id = loanTypeId, MaxTenureInMonths = 240 });

            // Customer has a recent application
            _loanAppRepo.Setup(r => r.GetByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(new List<LoanApplication>
                {
                    new LoanApplication
                    {
                        CustomerId = customerId,
                        CreatedDate = DateTime.UtcNow.AddDays(-(BusinessConstants.MinimumDaysRequiredForAnotherLoan - 1))
                    }
                });

            var dto = new CreateLoanApplicationRequestDto
            {
                LoanTypeId = loanTypeId,
                RequestedAmount = 500000,
                RequestedTenureInMonths = 120
            };

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.CreateAsync(customerId, dto));

            Assert.IsTrue(ex.Message.Contains(BusinessConstants.MinimumDaysRequiredForAnotherLoan.ToString()));
        }

        [TestMethod]
        public async Task CreateAsync_NoManagersAvailable_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var loanTypeId = Guid.NewGuid();

            _customerRepo.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(new Customer { Id = customerId, CreditScore = 700 });

            _loanTypeRepo.Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync(new LoanType { Id = loanTypeId, MaxTenureInMonths = 240 });

            // Rate limiting passes
            _loanAppRepo.Setup(r => r.GetByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(new List<LoanApplication>());

            // No managers returned
            _employeeRepo.Setup(r => r.GetAllWithDetailsAsync())
                .ReturnsAsync(new List<Employee>());

            var dto = new CreateLoanApplicationRequestDto
            {
                LoanTypeId = loanTypeId,
                RequestedAmount = 500000,
                RequestedTenureInMonths = 120
            };

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.CreateAsync(customerId, dto));

            Assert.AreEqual(ErrorMessages.ManagersNotAvailable, ex.Message);
        }

        [TestMethod]
        public async Task CreateAsync_ValidRequest_AssignsLeastLoadedManager_AndCreatesApplication()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var loanTypeId = Guid.NewGuid();

            _customerRepo.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(new Customer { Id = customerId, CreditScore = 750 });

            _loanTypeRepo.Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync(new LoanType { Id = loanTypeId, MaxTenureInMonths = 240 });

            // Rate limiting passes
            _loanAppRepo.Setup(r => r.GetByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(new List<LoanApplication>());

            var manager1 = new Employee
            {
                Id = Guid.NewGuid(),
                Role = EmployeeRole.Manager,
                AssignedLoanApplications = new List<LoanApplication>
                {
                    new LoanApplication(),
                    new LoanApplication()
                }
            };

            var manager2 = new Employee
            {
                Id = Guid.NewGuid(),
                Role = EmployeeRole.Manager,
                AssignedLoanApplications = new List<LoanApplication>
                {
                    new LoanApplication()
                }
            };

            _employeeRepo.Setup(r => r.GetAllWithDetailsAsync())
                .ReturnsAsync(new List<Employee> { manager1, manager2 });

            _publicIdService.Setup(s => s.GenerateApplicationNumber())
                .Returns("LA-ABCDEFGH");

            LoanApplication? savedApp = null;

            _loanAppRepo.Setup(r => r.AddAsync(It.IsAny<LoanApplication>()))
                .Callback<LoanApplication>(a => savedApp = a)
                .Returns(Task.CompletedTask);

            _loanAppRepo.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

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
            Assert.AreEqual(LoanApplicationStatus.Pending, result.Status);

            Assert.IsNotNull(savedApp);
            Assert.AreEqual(manager2.Id, savedApp!.AssignedEmployeeId); // least loaded

            _loanAppRepo.Verify(r => r.AddAsync(It.IsAny<LoanApplication>()), Times.Once);
            _loanAppRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateReviewAsync_InvalidApplicationNumber_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var dto = new ReviewLoanApplicationRequestDto
            {
                IsApproved = true,
                ApprovedAmount = 10000
            };

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.UpdateReviewAsync("INVALID", Guid.NewGuid(), dto));

            Assert.AreEqual(ErrorMessages.WrongFormatForLoanApplication, ex.Message);
        }

        [TestMethod]
        public async Task UpdateReviewAsync_ApplicationNotFound_ThrowsKeyNotFound()
        {
            // Arrange
            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((LoanApplication?)null);

            var dto = new ReviewLoanApplicationRequestDto
            {
                IsApproved = true,
                ApprovedAmount = 10000
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() =>
                _service.UpdateReviewAsync("LA-ABCDEFGH", Guid.NewGuid(), dto));
        }

        [TestMethod]
        public async Task UpdateReviewAsync_ManagerNotAssigned_ThrowsForbidden()
        {
            // Arrange
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

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.UpdateReviewAsync(application.ApplicationNumber, Guid.NewGuid(), dto));

            Assert.AreEqual(ErrorMessages.AccessDenied, ex.Message);
        }

        [TestMethod]
        public async Task UpdateReviewAsync_AlreadyReviewed_ThrowsBusinessRuleViolation()
        {
            // Arrange
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

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.UpdateReviewAsync(application.ApplicationNumber, managerId, dto));

            Assert.AreEqual(ErrorMessages.LoanApplicationAlreadyReviewed, ex.Message);
        }

        [TestMethod]
        public async Task UpdateReviewAsync_ApprovedAmountExceedsRequested_ThrowsBusinessRuleViolation()
        {
            // Arrange
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

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.UpdateReviewAsync(application.ApplicationNumber, managerId, dto));

            Assert.AreEqual(ErrorMessages.ApprovedAmountCannotExceedRequestedAmount, ex.Message);
        }

        [TestMethod]
        public async Task UpdateReviewAsync_ApprovedAmountBelowMin_ThrowsBusinessRuleViolation()
        {
            // Arrange
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

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.UpdateReviewAsync(application.ApplicationNumber, managerId, dto));

            Assert.AreEqual(ErrorMessages.BelowMinimumLoanAmount, ex.Message);
        }

        [TestMethod]
        public async Task UpdateReviewAsync_Approved_CreatesLoanWithEmis_AndReturnsApprovedResponse()
        {
            // Arrange
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
                    new() { EmiNumber = 1, DueDate = DateTime.UtcNow.AddMonths(1), TotalEmiAmount = 5000, InterestComponent = 100, PrincipalComponent = 4900, PrincipalRemainingAfterPayment = 25100 },
                    new() { EmiNumber = 2, DueDate = DateTime.UtcNow.AddMonths(2), TotalEmiAmount = 5000, InterestComponent = 90, PrincipalComponent = 4910, PrincipalRemainingAfterPayment = 20190 }
                });

            LoanDetails? createdLoan = null;

            _loanDetailsRepo.Setup(r => r.AddAsync(It.IsAny<LoanDetails>()))
                .Callback<LoanDetails>(l => createdLoan = l)
                .Returns(Task.CompletedTask);

            _loanDetailsRepo.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _loanAppRepo.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var dto = new ReviewLoanApplicationRequestDto
            {
                IsApproved = true,
                ApprovedAmount = 30000,
                ManagerComments = "  Approved  "
            };

            // Act
            var result = await _service.UpdateReviewAsync(application.ApplicationNumber, managerId, dto);

            // Assert
            Assert.AreEqual(LoanApplicationStatus.Approved, result.Status);
            Assert.AreEqual(30000, result.ApprovedAmount);
            Assert.AreEqual("Approved", result.ManagerComments);

            Assert.IsNotNull(createdLoan);
            Assert.AreEqual(LoanStatus.Active, createdLoan!.Status);
            Assert.AreEqual(30000, createdLoan.ApprovedAmount);
            Assert.AreEqual(application.CustomerId, createdLoan.CustomerId);
            Assert.AreEqual(application.Id, createdLoan.LoanApplicationId);
            Assert.AreEqual(application.LoanTypeId, createdLoan.LoanTypeId);
            Assert.AreEqual(application.RequestedTenureInMonths, createdLoan.TenureInMonths);
            Assert.AreEqual(application.LoanType.InterestRate, createdLoan.InterestRate);
            Assert.AreEqual(2, createdLoan.Emis.Count);

            _loanDetailsRepo.Verify(r => r.AddAsync(It.IsAny<LoanDetails>()), Times.Once);
            _loanDetailsRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _loanAppRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateReviewAsync_Rejected_DoesNotCreateLoan_AndSaves()
        {
            // Arrange
            var managerId = Guid.NewGuid();

            var application = new LoanApplication
            {
                ApplicationNumber = "LA-ABCDEFGH",
                AssignedEmployeeId = managerId,
                Status = LoanApplicationStatus.Pending,
                RequestedAmount = 50000,
                LoanType = new LoanType { MinAmount = 5000 }
            };

            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(application.ApplicationNumber))
                .ReturnsAsync(application);

            _loanDetailsRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _loanAppRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new ReviewLoanApplicationRequestDto
            {
                IsApproved = false,
                ManagerComments = "Rejected"
            };

            // Act
            var result = await _service.UpdateReviewAsync(application.ApplicationNumber, managerId, dto);

            // Assert
            Assert.AreEqual(LoanApplicationStatus.Rejected, result.Status);
            Assert.AreEqual(0, result.ApprovedAmount);

            _loanDetailsRepo.Verify(r => r.AddAsync(It.IsAny<LoanDetails>()), Times.Never);
            _loanDetailsRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _loanAppRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetApplicationDetailsForReview_InvalidApplicationNumber_ThrowsBusinessRuleViolation()
        {
            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.GetApplicationDetailsForReview("INVALID", Guid.NewGuid(), Role.Manager));

            Assert.AreEqual(ErrorMessages.WrongFormatForLoanApplication, ex.Message);
        }

        [TestMethod]
        public async Task GetApplicationDetailsForReview_ApplicationNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((LoanApplication?)null);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetApplicationDetailsForReview("LA-ABCDEFGH", Guid.NewGuid(), Role.Manager));

            Assert.AreEqual(ErrorMessages.LoanApplicationNotFound, ex.Message);
        }

        [TestMethod]
        public async Task GetApplicationDetailsForReview_WhenManagerNotAssigned_ThrowsForbidden()
        {
            // Arrange
            var application = new LoanApplication
            {
                ApplicationNumber = "LA-ABCDEFGH",
                AssignedEmployeeId = Guid.NewGuid(), // different manager
                Customer = new Customer { Name = "X", Loans = new List<LoanDetails>() },
                LoanType = new LoanType { Name = "Home", InterestRate = 8 }
            };

            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(application.ApplicationNumber))
                .ReturnsAsync(application);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.GetApplicationDetailsForReview(application.ApplicationNumber, Guid.NewGuid(), Role.Manager));

            Assert.AreEqual(ErrorMessages.AccessDenied, ex.Message);
        }

        [TestMethod]
        public async Task GetApplicationDetailsForReview_WhenRoleIsAdmin_IgnoresAssignment_AndReturnsDetails()
        {
            // Arrange
            var adminId = Guid.NewGuid();
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
                RequestedTenureInMonths = 120,
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
            var result = await _service.GetApplicationDetailsForReview(application.ApplicationNumber, adminId, Role.Admin);

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
            Assert.AreEqual(application.RequestedTenureInMonths, result.RequestedTenureInMonths);
            Assert.AreEqual(application.ApprovedAmount, result.ApprovedAmount);
            Assert.AreEqual(loanType.InterestRate, result.InterestRate);

            Assert.AreEqual(application.Status, result.Status);
            Assert.AreEqual(application.ManagerComments, result.ManagerComments);

            Assert.AreEqual(1, result.TotalOngoingLoans);
        }

        [TestMethod]
        public async Task GetByApplicationNumberAsync_InvalidFormat_ThrowsBusinessRuleViolation()
        {
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.GetByApplicationNumberAsync("INVALID"));

            Assert.AreEqual(ErrorMessages.WrongFormatForLoanApplication, ex.Message);
        }

        [TestMethod]
        public async Task GetByApplicationNumberAsync_NotFound_ThrowsNotFoundException()
        {
            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((LoanApplication?)null);

            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetByApplicationNumberAsync("LA-ABCDEFGH"));

            Assert.AreEqual(ErrorMessages.LoanApplicationNotFound, ex.Message);
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
                Customer = new Customer { Name = "Ankit Sharma" },
                LoanType = new LoanType { Name = "Home Loan", InterestRate = 8.75m }
            };

            _loanAppRepo
                .Setup(r => r.GetByApplicationNumberWithDetailsAsync(application.ApplicationNumber))
                .ReturnsAsync(application);

            // Act
            var result = await _service.GetByApplicationNumberAsync(application.ApplicationNumber);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(application.ApplicationNumber, result.ApplicationNumber);
            Assert.AreEqual(application.Customer.Name, result.CustomerName);
            Assert.AreEqual(application.LoanType.Name, result.LoanType);
            Assert.AreEqual(application.RequestedAmount, result.RequestedAmount);
            Assert.AreEqual(application.LoanType.InterestRate, result.InterestRate);
            Assert.AreEqual(application.ApprovedAmount, result.ApprovedAmount);
            Assert.AreEqual(application.AssignedEmployeeId, result.AssignedEmployeeId);
            Assert.AreEqual(application.RequestedTenureInMonths, result.RequestedTenureInMonths);
            Assert.AreEqual(application.Status, result.Status);
            Assert.AreEqual(application.ManagerComments, result.ManagerComments);
        }

        [TestMethod]
        public async Task GetApplicationsAsync_Admin_ReturnsPagedFilteredApplications()
        {
            // Arrange
            var apps = new List<LoanApplication>
            {
                new()
                {
                    ApplicationNumber = "LA-AAAA1111",
                    Status = LoanApplicationStatus.Pending,
                    RequestedAmount = 100000,
                    RequestedTenureInMonths = 120,
                    CreatedDate = DateTime.UtcNow,
                    LoanType = new LoanType { Name = "Home Loan" }
                },
                new()
                {
                    ApplicationNumber = "LA-BBBB2222",
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

            // Act
            var result = await _service.GetApplicationsAsync(
                userId: Guid.NewGuid(),
                userRole: Role.Admin,
                status: LoanApplicationStatus.Pending,
                pageNumber: 1,
                pageSize: 10);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual("LA-AAAA1111", result.Items.First().ApplicationNumber);
            Assert.AreEqual(1, result.TotalCount);
            Assert.AreEqual(1, result.TotalPages);
        }

        [TestMethod]
        public async Task GetApplicationsAsync_Manager_ReturnsOnlyAssignedApplications()
        {
            // Arrange
            var managerId = Guid.NewGuid();

            var apps = new List<LoanApplication>
            {
                new()
                {
                    ApplicationNumber = "LA-AAAA1111",
                    AssignedEmployeeId = managerId,
                    Status = LoanApplicationStatus.Pending,
                    RequestedTenureInMonths = 120,
                    CreatedDate = DateTime.UtcNow,
                    LoanType = new LoanType { Name = "Home Loan" }
                },
                new()
                {
                    ApplicationNumber = "LA-BBBB2222",
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

            // Act
            var result = await _service.GetApplicationsAsync(
                userId: managerId,
                userRole: Role.Manager,
                status: LoanApplicationStatus.Pending,
                pageNumber: 1,
                pageSize: 10);

            // Assert
            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual("LA-AAAA1111", result.Items.First().ApplicationNumber);
            Assert.AreEqual(1, result.TotalCount);
        }

        [TestMethod]
        public async Task GetApplicationsAsync_Customer_ReturnsOwnApplications()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var apps = new List<LoanApplication>
            {
                new()
                {
                    CustomerId = customerId,
                    ApplicationNumber = "LA-AAAA1111",
                    Status = LoanApplicationStatus.Pending,
                    RequestedTenureInMonths = 120,
                    CreatedDate = DateTime.UtcNow,
                    LoanType = new LoanType { Name = "Home Loan" }
                }
            };

            _loanAppRepo
                .Setup(r => r.GetByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(apps);

            // Act
            var result = await _service.GetApplicationsAsync(
                userId: customerId,
                userRole: Role.Customer,
                status: LoanApplicationStatus.Pending,
                pageNumber: 1,
                pageSize: 10);

            // Assert
            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual("LA-AAAA1111", result.Items.First().ApplicationNumber);
        }

        [TestMethod]
        public async Task GetApplicationsAsync_Customer_OtherCustomerApplication_ThrowsForbidden()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var apps = new List<LoanApplication>
            {
                new()
                {
                    CustomerId = Guid.NewGuid(), // not same customer
                    ApplicationNumber = "LA-AAAA1111",
                    Status = LoanApplicationStatus.Pending,
                    LoanType = new LoanType { Name = "Home Loan" }
                }
            };

            _loanAppRepo
                .Setup(r => r.GetByCustomerIdWithDetailsAsync(customerId))
                .ReturnsAsync(apps);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.GetApplicationsAsync(
                    userId: customerId,
                    userRole: Role.Customer,
                    status: LoanApplicationStatus.Pending,
                    pageNumber: 1,
                    pageSize: 10));

            Assert.AreEqual(ErrorMessages.AccessDenied, ex.Message);
        }

        [TestMethod]
        public async Task GetApplicationsAsync_InvalidRole_ThrowsForbidden()
        {
            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.GetApplicationsAsync(
                    userId: Guid.NewGuid(),
                    userRole: (Role)999,
                    status: LoanApplicationStatus.Pending,
                    pageNumber: 1,
                    pageSize: 10));

            Assert.AreEqual(ErrorMessages.AccessDenied, ex.Message);
        }

        [TestMethod]
        public async Task GetApplicationsAsync_Admin_Pagination_WorksCorrectly()
        {
            // Arrange
            var apps = new List<LoanApplication>
            {
                new()
                {
                    ApplicationNumber = "LA-AAAA1111",
                    Status = LoanApplicationStatus.Pending,
                    RequestedTenureInMonths = 120,
                    CreatedDate = DateTime.UtcNow,
                    LoanType = new LoanType { Name = "Home Loan" }
                },
                new()
                {
                    ApplicationNumber = "LA-BBBB2222",
                    Status = LoanApplicationStatus.Pending,
                    RequestedTenureInMonths = 240,
                    CreatedDate = DateTime.UtcNow,
                    LoanType = new LoanType { Name = "Car Loan" }
                },
                new()
                {
                    ApplicationNumber = "LA-CCCC3333",
                    Status = LoanApplicationStatus.Pending,
                    RequestedTenureInMonths = 60,
                    CreatedDate = DateTime.UtcNow,
                    LoanType = new LoanType { Name = "Personal Loan" }
                }
            };

            _loanAppRepo
                .Setup(r => r.GetAllWithDetailsAsync())
                .ReturnsAsync(apps);

            // Act
            var page1 = await _service.GetApplicationsAsync(
                userId: Guid.NewGuid(),
                userRole: Role.Admin,
                status: LoanApplicationStatus.Pending,
                pageNumber: 1,
                pageSize: 2);

            var page2 = await _service.GetApplicationsAsync(
                userId: Guid.NewGuid(),
                userRole: Role.Admin,
                status: LoanApplicationStatus.Pending,
                pageNumber: 2,
                pageSize: 2);

            // Assert
            Assert.AreEqual(2, page1.Items.Count);
            Assert.AreEqual(1, page2.Items.Count);

            Assert.AreEqual(3, page1.TotalCount);
            Assert.AreEqual(2, page1.TotalPages);
        }
    }
}
