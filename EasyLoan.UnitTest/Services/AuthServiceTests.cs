using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.Business.Services;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.Auth;
using EasyLoan.Dtos.Customer;
using EasyLoan.Dtos.Employee;
using EasyLoan.Models.Common.Enums;
using Moq;

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
                Name = " Test User ",
                Email = "TEST@EXAMPLE.COM ",
                PhoneNumber = "9876543210 ",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                AnnualSalary = 500000,
                PanNumber = " ABCDE1234F ",
                Password = "Strong@123"
            };
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

        private static ChangePasswordRequestDto CreateChangePasswordDto(string oldPass, string newPass)
        {
            return new ChangePasswordRequestDto
            {
                OldPassword = oldPass,
                NewPassword = newPass
            };
        }

        [TestMethod]
        public async Task RegisterCustomerAsync_WhenCustomerEmailAlreadyExists_ThrowsBusinessRuleViolationException()
        {
            // Arrange
            var dto = CreateValidRegisterDto();
            var normalizedEmail = dto.Email.ToLower().Trim();

            _customerRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.RegisterCustomerAsync(dto));

            Assert.AreEqual(ErrorMessages.EmailAlreadyExists, ex.Message);

            _employeeRepoMock.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>()), Times.Never);
            _customerRepoMock.Verify(r => r.ExistsByPanAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task RegisterCustomerAsync_WhenEmployeeEmailAlreadyExists_ThrowsBusinessRuleViolationException()
        {
            // Arrange
            var dto = CreateValidRegisterDto();
            var normalizedEmail = dto.Email.ToLower().Trim();

            _customerRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(false);

            _employeeRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.RegisterCustomerAsync(dto));

            Assert.AreEqual(ErrorMessages.EmailAlreadyExists, ex.Message);

            _customerRepoMock.Verify(r => r.ExistsByPanAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task RegisterCustomerAsync_WhenPanAlreadyExists_ThrowsBusinessRuleViolationException()
        {
            // Arrange
            var dto = CreateValidRegisterDto();
            var normalizedEmail = dto.Email.ToLower().Trim();

            _customerRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(false);

            _employeeRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(false);

            _customerRepoMock
                .Setup(r => r.ExistsByPanAsync(dto.PanNumber))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.RegisterCustomerAsync(dto));

            Assert.AreEqual(ErrorMessages.PanAlreadyExists, ex.Message);
        }

        [TestMethod]
        public async Task RegisterCustomerAsync_WhenCustomerUnder18_ThrowsBusinessRuleViolationException()
        {
            // Arrange
            var dto = CreateValidRegisterDto();
            dto.DateOfBirth = DateTime.UtcNow.AddYears(-17);

            var normalizedEmail = dto.Email.ToLower().Trim();

            _customerRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(false);

            _employeeRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(false);

            _customerRepoMock
                .Setup(r => r.ExistsByPanAsync(dto.PanNumber))
                .ReturnsAsync(false);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.RegisterCustomerAsync(dto));

            Assert.AreEqual("Customer must be at least 18 years old.", ex.Message);
        }

        [TestMethod]
        public async Task RegisterCustomerAsync_WhenValidRequest_CreatesCustomerAndReturnsProfile()
        {
            // Arrange
            var dto = CreateValidRegisterDto();
            var normalizedEmail = dto.Email.ToLower().Trim();

            Customer? savedCustomer = null;

            _customerRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(false);

            _employeeRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(false);

            _customerRepoMock
                .Setup(r => r.ExistsByPanAsync(dto.PanNumber))
                .ReturnsAsync(false);

            _customerRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Customer>()))
                .Callback<Customer>(c => savedCustomer = c)
                .Returns(Task.CompletedTask);

            _customerRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.RegisterCustomerAsync(dto);

            // Assert - repo calls
            _customerRepoMock.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            // Assert - saved entity correctness
            Assert.IsNotNull(savedCustomer);
            Assert.AreEqual(800, savedCustomer!.CreditScore);

            Assert.AreEqual(dto.Name.Trim(), savedCustomer.Name);
            Assert.AreEqual(normalizedEmail, savedCustomer.Email);
            Assert.AreEqual(dto.PhoneNumber.Trim(), savedCustomer.PhoneNumber);
            Assert.AreEqual(dto.PanNumber.Trim(), savedCustomer.PanNumber);

            Assert.IsTrue(BCrypt.Net.BCrypt.Verify(dto.Password, savedCustomer.Password));

            // Assert - returned DTO correctness
            Assert.AreEqual(dto.Name.Trim(), result.Name);
            Assert.AreEqual(normalizedEmail, result.Email);
            Assert.AreEqual(dto.PhoneNumber.Trim(), result.PhoneNumber);
            Assert.AreEqual(800, result.CreditScore);
            Assert.AreEqual(dto.PanNumber.Trim(), result.PanNumber);
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
                .Setup(t => t.GenerateToken(customerId, customer.Email, Role.Customer))
                .Returns("jwt-token");

            // Act
            var result = await _service.LoginCustomerAsync(dto);

            // Assert
            Assert.AreEqual("jwt-token", result);

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(customerId, customer.Email, Role.Customer),
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
            var ex = await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _service.LoginCustomerAsync(dto));

            Assert.AreEqual(ErrorMessages.InvalidCredentials, ex.Message);

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Role>()),
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
            var ex = await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _service.LoginCustomerAsync(dto));

            Assert.AreEqual(ErrorMessages.InvalidCredentials, ex.Message);

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Role>()),
                Times.Never);
        }

        [TestMethod]
        public async Task RegisterManagerAsync_WhenEmployeeEmailAlreadyExists_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var request = CreateValidManagerRequest();
            var normalizedEmail = request.Email.ToLower().Trim();

            _employeeRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.RegisterManagerAsync(request));

            Assert.AreEqual(ErrorMessages.EmailAlreadyExists, ex.Message);

            _employeeRepoMock.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Never);
            _employeeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task RegisterManagerAsync_WhenCustomerEmailAlreadyExists_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var request = CreateValidManagerRequest();
            var normalizedEmail = request.Email.ToLower().Trim();

            _employeeRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(false);

            _customerRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(() =>
                _service.RegisterManagerAsync(request));

            Assert.AreEqual(ErrorMessages.EmailAlreadyExists, ex.Message);

            _employeeRepoMock.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Never);
            _employeeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task RegisterManagerAsync_WhenEmailDoesNotExist_CreatesManager()
        {
            // Arrange
            var request = CreateValidManagerRequest();
            var normalizedEmail = request.Email.ToLower().Trim();

            _employeeRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
                .ReturnsAsync(false);

            _customerRepoMock
                .Setup(r => r.ExistsByEmailAsync(normalizedEmail))
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

            Assert.AreEqual(request.Name.Trim(), result.Name);
            Assert.AreEqual(normalizedEmail, result.Email);
            Assert.AreEqual(request.PhoneNumber.Trim(), result.PhoneNumber);
            Assert.AreEqual(EmployeeRole.Manager, result.Role);

            Assert.IsNotNull(savedEmployee);
            Assert.AreEqual(EmployeeRole.Manager, savedEmployee!.Role);
            Assert.AreEqual(request.Name.Trim(), savedEmployee.Name);
            Assert.AreEqual(normalizedEmail, savedEmployee.Email);
            Assert.AreEqual(request.PhoneNumber.Trim(), savedEmployee.PhoneNumber);

            Assert.IsTrue(
                BCrypt.Net.BCrypt.Verify(request.Password, savedEmployee.Password),
                "Password must be stored as BCrypt hash"
            );

            _employeeRepoMock.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
            _employeeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
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
                .Setup(t => t.GenerateToken(employeeId, employee.Email, Role.Manager))
                .Returns("manager-token");

            // Act
            var token = await _service.LoginEmployeeAsync(request);

            // Assert
            Assert.AreEqual("manager-token", token);

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(employeeId, employee.Email, Role.Manager),
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
                .Setup(t => t.GenerateToken(employeeId, employee.Email, Role.Admin))
                .Returns("admin-token");

            // Act
            var token = await _service.LoginEmployeeAsync(request);

            // Assert
            Assert.AreEqual("admin-token", token);

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(employeeId, employee.Email, Role.Admin),
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
            var ex = await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _service.LoginEmployeeAsync(request));

            Assert.AreEqual(ErrorMessages.InvalidCredentials, ex.Message);

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Role>()),
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
            var ex = await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _service.LoginEmployeeAsync(request));

            Assert.AreEqual(ErrorMessages.InvalidCredentials, ex.Message);

            _tokenGeneratorMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Role>()),
                Times.Never);
        }

        [TestMethod]
        public async Task ChangePasswordAsync_WhenRoleIsCustomer_AndCustomerNotFound_ThrowsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = CreateChangePasswordDto("Old@123", "New@123");

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((Customer?)null);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.ChangePasswordAsync(userId, Role.Customer, dto));

            Assert.AreEqual(ErrorMessages.CustomerNotFound, ex.Message);

            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task ChangePasswordAsync_WhenRoleIsCustomer_AndOldPasswordWrong_ThrowsAuthenticationFailed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = CreateChangePasswordDto("WrongOld@123", "New@123");

            var customer = new Customer
            {
                Id = userId,
                Email = "c@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword("CorrectOld@123")
            };

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(customer);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _service.ChangePasswordAsync(userId, Role.Customer, dto));

            Assert.AreEqual("Old password is incorrect.", ex.Message);

            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task ChangePasswordAsync_WhenRoleIsCustomer_AndOldPasswordCorrect_UpdatesPassword_AndSaves()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = CreateChangePasswordDto("CorrectOld@123", "New@123");

            var customer = new Customer
            {
                Id = userId,
                Email = "c@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword("CorrectOld@123")
            };

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(customer);

            _customerRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var oldHash = customer.Password;

            // Act
            await _service.ChangePasswordAsync(userId, Role.Customer, dto);

            // Assert
            Assert.AreNotEqual(oldHash, customer.Password);
            Assert.IsTrue(BCrypt.Net.BCrypt.Verify(dto.NewPassword, customer.Password));

            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _employeeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task ChangePasswordAsync_WhenRoleIsManagerOrAdmin_AndEmployeeNotFound_ThrowsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = CreateChangePasswordDto("Old@123", "New@123");

            _employeeRepoMock
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((Employee?)null);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.ChangePasswordAsync(userId, Role.Manager, dto));

            Assert.AreEqual(ErrorMessages.EmployeeNotFound, ex.Message);

            _employeeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task ChangePasswordAsync_WhenRoleIsManagerOrAdmin_AndOldPasswordWrong_ThrowsAuthenticationFailed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = CreateChangePasswordDto("WrongOld@123", "New@123");

            var employee = new Employee
            {
                Id = userId,
                Email = "e@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword("CorrectOld@123"),
                Role = EmployeeRole.Manager
            };

            _employeeRepoMock
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(employee);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<AuthenticationFailedException>(() =>
                _service.ChangePasswordAsync(userId, Role.Admin, dto));

            Assert.AreEqual("Old password is incorrect.", ex.Message);

            _employeeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task ChangePasswordAsync_WhenRoleIsManagerOrAdmin_AndOldPasswordCorrect_UpdatesPassword_AndSaves()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = CreateChangePasswordDto("CorrectOld@123", "New@123");

            var employee = new Employee
            {
                Id = userId,
                Email = "e@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword("CorrectOld@123"),
                Role = EmployeeRole.Admin
            };

            _employeeRepoMock
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(employee);

            _employeeRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var oldHash = employee.Password;

            // Act
            await _service.ChangePasswordAsync(userId, Role.Admin, dto);

            // Assert
            Assert.AreNotEqual(oldHash, employee.Password);
            Assert.IsTrue(BCrypt.Net.BCrypt.Verify(dto.NewPassword, employee.Password));

            _employeeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
