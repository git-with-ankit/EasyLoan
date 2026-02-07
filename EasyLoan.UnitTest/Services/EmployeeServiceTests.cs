using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Services;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.Employee;
using EasyLoan.Models.Common.Enums;
using Moq;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    public class EmployeeServiceTests
    {
        private Mock<IEmployeeRepository> _mockEmployeeRepo = null!;
        private Mock<ILoanTypeRepository> _mockLoanTypeRepo = null!;
        private Mock<ILoanApplicationRepository> _mockLoanApplicationRepo = null!;
        private EmployeeService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockEmployeeRepo = new Mock<IEmployeeRepository>();
            _mockLoanTypeRepo = new Mock<ILoanTypeRepository>();
            _mockLoanApplicationRepo = new Mock<ILoanApplicationRepository>();

            _service = new EmployeeService(
                _mockEmployeeRepo.Object,
                _mockLoanTypeRepo.Object,
                _mockLoanApplicationRepo.Object);
        }

        [TestMethod]
        public async Task GetProfileAsync_WhenEmployeeNotFound_ThrowsNotFoundException_WithCorrectMessage()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            _mockEmployeeRepo
                .Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync((Employee?)null);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetProfileAsync(employeeId));

            Assert.AreEqual(ErrorMessages.CustomerNotFound, ex.Message);

            _mockEmployeeRepo.Verify(r => r.GetByIdAsync(employeeId), Times.Once);
        }

        [TestMethod]
        public async Task GetProfileAsync_WhenEmployeeIdMismatch_ThrowsForbiddenException_WithCorrectMessage()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            var employee = new Employee
            {
                Id = Guid.NewGuid(), // mismatch
                Name = "John",
                Email = "john@test.com",
                PhoneNumber = "123",
                Role = EmployeeRole.Manager
            };

            _mockEmployeeRepo
                .Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.GetProfileAsync(employeeId));

            Assert.AreEqual(ErrorMessages.AccessDenied, ex.Message);
        }

        [TestMethod]
        public async Task GetProfileAsync_WhenAuthorized_ReturnsEmployeeProfile()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            var employee = new Employee
            {
                Id = employeeId,
                Name = "John",
                Email = "john@test.com",
                PhoneNumber = "123",
                Role = EmployeeRole.Manager
            };

            _mockEmployeeRepo
                .Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);

            // Act
            var result = await _service.GetProfileAsync(employeeId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(employee.Name, result.Name);
            Assert.AreEqual(employee.Email, result.Email);
            Assert.AreEqual(employee.PhoneNumber, result.PhoneNumber);
            Assert.AreEqual(employee.Role, result.Role);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_WhenEmployeeNotFound_ThrowsNotFoundException_AndDoesNotSave()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            _mockEmployeeRepo
                .Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync((Employee?)null);

            var dto = new UpdateEmployeeProfileRequestDto
            {
                Name = "New Name",
                PhoneNumber = "999"
            };

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.UpdateProfileAsync(employeeId, dto));

            Assert.AreEqual(ErrorMessages.CustomerNotFound, ex.Message);

            _mockEmployeeRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_WhenEmployeeIdMismatch_ThrowsForbiddenException_AndDoesNotSave()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                Name = "John",
                Email = "john@test.com",
                PhoneNumber = "123",
                Role = EmployeeRole.Manager
            };

            _mockEmployeeRepo
                .Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);

            var dto = new UpdateEmployeeProfileRequestDto
            {
                Name = "New Name",
                PhoneNumber = "999"
            };

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.UpdateProfileAsync(employeeId, dto));

            Assert.AreEqual(ErrorMessages.AccessDenied, ex.Message);

            _mockEmployeeRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_WhenValid_UpdatesAndReturnsProfile_TrimsStrings()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            var employee = new Employee
            {
                Id = employeeId,
                Name = "John",
                Email = "john@test.com",
                PhoneNumber = "123",
                Role = EmployeeRole.Manager
            };

            var dto = new UpdateEmployeeProfileRequestDto
            {
                Name = "  Jane  ",
                PhoneNumber = "  999  "
            };

            _mockEmployeeRepo
                .Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);

            _mockEmployeeRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateProfileAsync(employeeId, dto);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual("Jane", result.Name);
            Assert.AreEqual("999", result.PhoneNumber);

            Assert.AreEqual(employee.Email, result.Email);
            Assert.AreEqual(employee.Role, result.Role);

            _mockEmployeeRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_WhenFieldsAreNull_PreservesExistingValues()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            var employee = new Employee
            {
                Id = employeeId,
                Name = "Existing Name",
                Email = "existing@test.com",
                PhoneNumber = "111",
                Role = EmployeeRole.Admin
            };

            var dto = new UpdateEmployeeProfileRequestDto
            {
                Name = null,
                PhoneNumber = null
            };

            _mockEmployeeRepo
                .Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);

            _mockEmployeeRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateProfileAsync(employeeId, dto);

            // Assert
            Assert.AreEqual("Existing Name", result.Name);
            Assert.AreEqual("111", result.PhoneNumber);

            _mockEmployeeRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_WhenNameIsWhitespace_UpdatesNameToEmptyString_BranchCoverage()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            var employee = new Employee
            {
                Id = employeeId,
                Name = "Existing Name",
                Email = "existing@test.com",
                PhoneNumber = "111",
                Role = EmployeeRole.Admin
            };

            var dto = new UpdateEmployeeProfileRequestDto
            {
                Name = "   ",
                PhoneNumber = null
            };

            _mockEmployeeRepo
                .Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);

            _mockEmployeeRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateProfileAsync(employeeId, dto);

            // Assert
            Assert.AreEqual("", result.Name);
            Assert.AreEqual("", employee.Name);

            _mockEmployeeRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAdminDashboardAsync_WhenCalled_ReturnsCorrectCounts()
        {
            // Arrange
            var loanTypes = new List<LoanType>
            {
                new LoanType { Id = Guid.NewGuid(), Name = "Home Loan" },
                new LoanType { Id = Guid.NewGuid(), Name = "Car Loan" }
            };

            var applications = new List<LoanApplication>
            {
                new LoanApplication { Id = Guid.NewGuid(), Status = LoanApplicationStatus.Pending },
                new LoanApplication { Id = Guid.NewGuid(), Status = LoanApplicationStatus.Pending },
                new LoanApplication { Id = Guid.NewGuid(), Status = LoanApplicationStatus.Approved },
                new LoanApplication { Id = Guid.NewGuid(), Status = LoanApplicationStatus.Rejected },
                new LoanApplication { Id = Guid.NewGuid(), Status = LoanApplicationStatus.Rejected },
                new LoanApplication { Id = Guid.NewGuid(), Status = LoanApplicationStatus.Rejected }
            };

            var managers = new List<Employee>
            {
                new Employee { Id = Guid.NewGuid(), Role = EmployeeRole.Manager },
                new Employee { Id = Guid.NewGuid(), Role = EmployeeRole.Manager }
            };

            _mockLoanTypeRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(loanTypes);

            _mockLoanApplicationRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(applications);

            _mockEmployeeRepo
                .Setup(r => r.GetManagersAsync())
                .ReturnsAsync(managers);

            // Act
            var result = await _service.GetAdminDashboardAsync();

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(2, result.NumberOfLoanTypes);
            Assert.AreEqual(2, result.NumberOfPendingApplications);
            Assert.AreEqual(1, result.NumberOfApprovedApplications);
            Assert.AreEqual(3, result.NumberOfRejectedApplications);
            Assert.AreEqual(2, result.NumberOfManagers);
        }

        [TestMethod]
        public async Task GetAdminDashboardAsync_WhenNoData_ReturnsZeros()
        {
            // Arrange
            _mockLoanTypeRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<LoanType>());

            _mockLoanApplicationRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<LoanApplication>());

            _mockEmployeeRepo
                .Setup(r => r.GetManagersAsync())
                .ReturnsAsync(new List<Employee>());

            // Act
            var result = await _service.GetAdminDashboardAsync();

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(0, result.NumberOfLoanTypes);
            Assert.AreEqual(0, result.NumberOfPendingApplications);
            Assert.AreEqual(0, result.NumberOfApprovedApplications);
            Assert.AreEqual(0, result.NumberOfRejectedApplications);
            Assert.AreEqual(0, result.NumberOfManagers);
        }
    }
}
