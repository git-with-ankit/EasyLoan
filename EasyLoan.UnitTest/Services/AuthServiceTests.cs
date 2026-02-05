;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Business.Services;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.Customer;
using EasyLoan.Dtos.Employee;
using EasyLoan.Models.Common.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<ICustomerRepository> _customerRepoMock = null!;
        private Mock<IEmployeeRepository> _employeeRepoMock = null!;
        private Mock<IJwtTokenGeneratorService> _tokenGeneratorMock = null!;
        private AuthService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _customerRepoMock = new Mock<ICustomerRepository>();
            _employeeRepoMock = new Mock<IEmployeeRepository>();
            _tokenGeneratorMock = new Mock<IJwtTokenGeneratorService>();

            _service = new AuthService(
                _customerRepoMock.Object,
                _employeeRepoMock.Object,
                _tokenGeneratorMock.Object
            );
        }
        private static RegisterCustomerRequestDto CreateValidRegisterDto()
        {
            return new RegisterCustomerRequestDto
            {
                Name = "Test User",
                Email = "test@example.com",
                PhoneNumber = "9876543210",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                AnnualSalary = 500000,
                PanNumber = "ABCDE1234F",
                Password = "Strong@123"
            };
        }

        [TestMethod]
        public async Task RegisterCustomerAsync_EmailAlreadyExists_ThrowsBusinessRuleViolationException()
        {
            // Arrange
            var dto = CreateValidRegisterDto();

            _customerRepoMock
                .Setup(r => r.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.RegisterCustomerAsync(dto)
            );
        }
        [TestMethod]
        public async Task RegisterCustomerAsync_PanAlreadyExists_ThrowsBusinessRuleViolationException()
        {
            // Arrange
            var dto = CreateValidRegisterDto();

            _customerRepoMock
                .Setup(r => r.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(false);

            _customerRepoMock
                .Setup(r => r.ExistsByPanAsync(dto.PanNumber))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.RegisterCustomerAsync(dto)
            );
        }

        [TestMethod]
        public async Task RegisterCustomerAsync_CustomerUnder18_ThrowsBusinessRuleViolationException()
        {
            // Arrange
            var dto = CreateValidRegisterDto();
            dto.DateOfBirth = DateTime.UtcNow.AddYears(-17);

            _customerRepoMock
                .Setup(r => r.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(false);

            _customerRepoMock
                .Setup(r => r.ExistsByPanAsync(dto.PanNumber))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.RegisterCustomerAsync(dto)
            );
        }
        [TestMethod]
        public async Task RegisterCustomerAsync_ValidRequest_CreatesCustomerAndReturnsProfile()
        {
            // Arrange
            var dto = CreateValidRegisterDto();

            Customer? savedCustomer = null;

            _customerRepoMock
                .Setup(r => r.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(false);

            _customerRepoMock
                .Setup(r => r.ExistsByPanAsync(dto.PanNumber))
                .ReturnsAsync(false);

            _customerRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Customer>()))
                .Callback<Customer>(c => savedCustomer = c)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.RegisterCustomerAsync(dto);

            // Assert – repository interactions
            _customerRepoMock.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            // Assert – entity state
            Assert.IsNotNull(savedCustomer);
            Assert.AreEqual(800, savedCustomer!.CreditScore);
            Assert.IsTrue(BCrypt.Net.BCrypt.Verify(dto.Password, savedCustomer.Password));

            // Assert – returned DTO
            Assert.AreEqual(dto.Name, result.Name);
            Assert.AreEqual(dto.Email.ToLower(), result.Email);
            Assert.AreEqual(dto.PhoneNumber, result.PhoneNumber);
            Assert.AreEqual(800, result.CreditScore);
        }
        [TestMethod]
        public async Task LoginCustomerAsync_WhenCredentialsValid_ReturnsToken()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var plainPassword = "Strong@123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            var dto = new CustomerLoginRequestDto
            {
                Email = "TEST@EMAIL.COM ",
                Password = plainPassword
            };

            var customer = new Customer
            {
                Id = customerId,
                Email = "test@email.com",
                Password = hashedPassword
            };

            _customerRepoMock
                .Setup(r => r.GetByEmailAsync("test@email.com"))
                .ReturnsAsync(customer);

            _tokenGeneratorMock
                .Setup(t => t.GenerateToken(customerId, Role.Customer))
                .Returns("jwt-token");

            // Act
            var result = await _service.LoginCustomerAsync(dto);

            // Assert
            Assert.AreEqual("jwt-token", result);

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(customerId, Role.Customer),
                Times.Once);
        }
        [TestMethod]
        public async Task LoginCustomerAsync_WhenCustomerNotFound_ThrowsAuthenticationFailed()
        {
            // Arrange
            var dto = new CustomerLoginRequestDto
            {
                Email = "missing@email.com",
                Password = "Any@123"
            };

            _customerRepoMock
                .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _service.LoginCustomerAsync(dto));

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<Role>()),
                Times.Never);
        }
        [TestMethod]
        public async Task LoginCustomerAsync_WhenPasswordIncorrect_ThrowsAuthenticationFailed()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                Email = "user@email.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Correct@123")
            };

            var dto = new CustomerLoginRequestDto
            {
                Email = "user@email.com",
                Password = "Wrong@123"
            };

            _customerRepoMock
                .Setup(r => r.GetByEmailAsync("user@email.com"))
                .ReturnsAsync(customer);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _service.LoginCustomerAsync(dto));

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<Role>()),
                Times.Never);
        }
        [TestMethod]
        public async Task RegisterManagerAsync_WhenEmailDoesNotExist_CreatesManager()
        {
            // Arrange
            var request = CreateValidManagerRequest();

            _employeeRepoMock
                .Setup(r => r.ExistsByEmailAsync("manager@test.com"))
                .ReturnsAsync(false);

            Employee? savedEmployee = null;

            _employeeRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Employee>()))
                .Callback<Employee>(e => savedEmployee = e)
                .Returns(Task.CompletedTask);

            _employeeRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.RegisterManagerAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Manager Name", result.Name);
            Assert.AreEqual("manager@test.com", result.Email);
            Assert.AreEqual("9876543210", result.PhoneNumber);
            Assert.AreEqual(EmployeeRole.Manager, result.Role);

            Assert.IsNotNull(savedEmployee);
            Assert.AreEqual(EmployeeRole.Manager, savedEmployee.Role);
            Assert.IsTrue(
                BCrypt.Net.BCrypt.Verify(request.Password, savedEmployee.Password),
                "Password must be stored as BCrypt hash"
            );

            _employeeRepoMock.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
            _employeeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        [TestMethod]
        public async Task RegisterManagerAsync_WhenEmailAlreadyExists_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var request = CreateValidManagerRequest();

            _employeeRepoMock
                .Setup(r => r.ExistsByEmailAsync("manager@test.com"))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.RegisterManagerAsync(request));

            _employeeRepoMock.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Never);
            _employeeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
        private static CreateManagerRequestDto CreateValidManagerRequest()
        {
            return new CreateManagerRequestDto
            {
                Name = " Manager Name ",
                Email = "MANAGER@TEST.COM ",
                PhoneNumber = "9876543210 ",
                Password = "Strong@123"
            };
        }
        [TestMethod]
        public async Task LoginEmployeeAsync_WhenManagerCredentialsAreValid_ReturnsManagerToken()
        {
            // Arrange
            var password = "Strong@123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var employeeId = Guid.NewGuid();

            var employee = new Employee
            {
                Id = employeeId,
                Email = "manager@test.com",
                Password = hashedPassword,
                Role = EmployeeRole.Manager
            };

            var request = new EmployeeLoginRequestDto
            {
                Email = "MANAGER@TEST.COM ",
                Password = password
            };

            _employeeRepoMock
                .Setup(r => r.GetByEmailAsync("manager@test.com"))
                .ReturnsAsync(employee);

            _tokenGeneratorMock
                .Setup(t => t.GenerateToken(employeeId, Role.Manager))
                .Returns("manager-token");

            // Act
            var token = await _service.LoginEmployeeAsync(request);

            // Assert
            Assert.AreEqual("manager-token", token);

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(employeeId, Role.Manager),
                Times.Once);
        }
        [TestMethod]
        public async Task LoginEmployeeAsync_WhenAdminCredentialsAreValid_ReturnsAdminToken()
        {
            // Arrange
            var password = "Strong@123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var employeeId = Guid.NewGuid();

            var employee = new Employee
            {
                Id = employeeId,
                Email = "admin@test.com",
                Password = hashedPassword,
                Role = EmployeeRole.Admin
            };

            var request = new EmployeeLoginRequestDto
            {
                Email = "ADMIN@TEST.COM",
                Password = password
            };

            _employeeRepoMock
                .Setup(r => r.GetByEmailAsync("admin@test.com"))
                .ReturnsAsync(employee);

            _tokenGeneratorMock
                .Setup(t => t.GenerateToken(employeeId, Role.Admin))
                .Returns("admin-token");

            // Act
            var token = await _service.LoginEmployeeAsync(request);

            // Assert
            Assert.AreEqual("admin-token", token);

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(employeeId, Role.Admin),
                Times.Once);
        }
        [TestMethod]
        public async Task LoginEmployeeAsync_WhenEmailNotFound_ThrowsAuthenticationFailed()
        {
            // Arrange
            var request = new EmployeeLoginRequestDto
            {
                Email = "missing@test.com",
                Password = "Strong@123"
            };

            _employeeRepoMock
                .Setup(r => r.GetByEmailAsync("missing@test.com"))
                .ReturnsAsync((Employee?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _service.LoginEmployeeAsync(request));

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<Role>()),
                Times.Never);
        }
        [TestMethod]
        public async Task LoginEmployeeAsync_WhenPasswordIsInvalid_ThrowsAuthenticationFailed()
        {
            // Arrange
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                Email = "user@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Correct@123"),
                Role = EmployeeRole.Manager
            };

            var request = new EmployeeLoginRequestDto
            {
                Email = "user@test.com",
                Password = "Wrong@123"
            };

            _employeeRepoMock
                .Setup(r => r.GetByEmailAsync("user@test.com"))
                .ReturnsAsync(employee);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _service.LoginEmployeeAsync(request));

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<Role>()),
                Times.Never);
        }
    }
}
