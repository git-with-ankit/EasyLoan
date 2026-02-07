using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Services;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.Customer;
using Moq;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    public class CustomerServiceTests
    {
        private Mock<ICustomerRepository> _customerRepoMock = null!;
        private CustomerService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _customerRepoMock = new Mock<ICustomerRepository>();
            _service = new CustomerService(_customerRepoMock.Object);
        }

        [TestMethod]
        public async Task GetProfileAsync_WhenCustomerExists_ReturnsProfile()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                Name = "Ankit",
                Email = "ankit@test.com",
                PhoneNumber = "9876543210",
                DateOfBirth = new DateTime(1998, 5, 10),
                CreditScore = 750,
                AnnualSalary = 1200000,
                PanNumber = "ABCDE1234F"
            };

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _service.GetProfileAsync(customerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(customer.Name, result.Name);
            Assert.AreEqual(customer.Email, result.Email);
            Assert.AreEqual(customer.PhoneNumber, result.PhoneNumber);
            Assert.AreEqual(customer.DateOfBirth, result.DateOfBirth);
            Assert.AreEqual(customer.CreditScore, result.CreditScore);
            Assert.AreEqual(customer.AnnualSalary, result.AnnualSalary);
            Assert.AreEqual(customer.PanNumber, result.PanNumber);

            _customerRepoMock.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        }

        [TestMethod]
        public async Task GetProfileAsync_WhenCustomerNotFound_ThrowsNotFoundException_WithCorrectMessage()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetProfileAsync(customerId));

            Assert.AreEqual(ErrorMessages.CustomerNotFound, ex.Message);

            _customerRepoMock.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_WhenCustomerNotFound_ThrowsNotFoundException_AndDoesNotSave()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var dto = new UpdateCustomerProfileRequestDto
            {
                Name = "New Name",
                PhoneNumber = "9999999999",
                AnnualSalary = 1000000
            };

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.UpdateProfileAsync(customerId, dto));

            Assert.AreEqual(ErrorMessages.CustomerNotFound, ex.Message);

            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_WhenAllFieldsProvided_UpdatesAllFields_AndTrimsStrings()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                Name = "Old Name",
                PhoneNumber = "9876543210",
                AnnualSalary = 500000,
                Email = "test@test.com",
                PanNumber = "ABCDE1234F",
                CreditScore = 700,
                DateOfBirth = new DateTime(1995, 1, 1)
            };

            var dto = new UpdateCustomerProfileRequestDto
            {
                Name = "  New Name  ",
                PhoneNumber = "  9123456789  ",
                AnnualSalary = 1000000
            };

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _customerRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateProfileAsync(customerId, dto);

            // Assert - returned DTO
            Assert.AreEqual("New Name", result.Name);
            Assert.AreEqual("9123456789", result.PhoneNumber);
            Assert.AreEqual(1000000, result.AnnualSalary);

            // Assert - customer entity updated too
            Assert.AreEqual("New Name", customer.Name);
            Assert.AreEqual("9123456789", customer.PhoneNumber);
            Assert.AreEqual(1000000, customer.AnnualSalary);

            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_WhenFieldsAreNull_PreservesExistingValues()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                Name = "Existing Name",
                PhoneNumber = "9876543210",
                AnnualSalary = 750000,
                Email = "test@test.com",
                PanNumber = "ABCDE1234F",
                CreditScore = 720,
                DateOfBirth = new DateTime(1994, 6, 15)
            };

            var dto = new UpdateCustomerProfileRequestDto
            {
                Name = null,
                PhoneNumber = null,
                AnnualSalary = null
            };

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _customerRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateProfileAsync(customerId, dto);

            // Assert
            Assert.AreEqual("Existing Name", result.Name);
            Assert.AreEqual("9876543210", result.PhoneNumber);
            Assert.AreEqual(750000, result.AnnualSalary);

            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_WhenOnlyNameProvided_UpdatesOnlyName()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                Name = "Old",
                PhoneNumber = "1111111111",
                AnnualSalary = 200000,
                Email = "a@test.com",
                PanNumber = "ABCDE1234F",
                CreditScore = 700,
                DateOfBirth = new DateTime(1995, 1, 1)
            };

            var dto = new UpdateCustomerProfileRequestDto
            {
                Name = "  Updated  ",
                PhoneNumber = null,
                AnnualSalary = null
            };

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _customerRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateProfileAsync(customerId, dto);

            // Assert
            Assert.AreEqual("Updated", result.Name);
            Assert.AreEqual("1111111111", result.PhoneNumber);
            Assert.AreEqual(200000, result.AnnualSalary);

            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_WhenOnlyPhoneProvided_UpdatesOnlyPhone()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                Name = "Old",
                PhoneNumber = "1111111111",
                AnnualSalary = 200000,
                Email = "a@test.com",
                PanNumber = "ABCDE1234F",
                CreditScore = 700,
                DateOfBirth = new DateTime(1995, 1, 1)
            };

            var dto = new UpdateCustomerProfileRequestDto
            {
                Name = null,
                PhoneNumber = "  9999999999  ",
                AnnualSalary = null
            };

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _customerRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateProfileAsync(customerId, dto);

            // Assert
            Assert.AreEqual("Old", result.Name);
            Assert.AreEqual("9999999999", result.PhoneNumber);
            Assert.AreEqual(200000, result.AnnualSalary);

            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_WhenOnlyAnnualSalaryProvided_UpdatesOnlySalary()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                Name = "Old",
                PhoneNumber = "1111111111",
                AnnualSalary = 200000,
                Email = "a@test.com",
                PanNumber = "ABCDE1234F",
                CreditScore = 700,
                DateOfBirth = new DateTime(1995, 1, 1)
            };

            var dto = new UpdateCustomerProfileRequestDto
            {
                Name = null,
                PhoneNumber = null,
                AnnualSalary = 999999
            };

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _customerRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateProfileAsync(customerId, dto);

            // Assert
            Assert.AreEqual("Old", result.Name);
            Assert.AreEqual("1111111111", result.PhoneNumber);
            Assert.AreEqual(999999, result.AnnualSalary);

            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_WhenNameIsWhitespace_UpdatesNameToEmptyString_BranchCoverage()
        {

            // Arrange
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                Name = "Existing Name",
                PhoneNumber = "1111111111",
                AnnualSalary = 200000,
                Email = "a@test.com",
                PanNumber = "ABCDE1234F",
                CreditScore = 700,
                DateOfBirth = new DateTime(1995, 1, 1)
            };

            var dto = new UpdateCustomerProfileRequestDto
            {
                Name = "   ",
                PhoneNumber = null,
                AnnualSalary = null
            };

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _customerRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateProfileAsync(customerId, dto);

            // Assert
            Assert.AreEqual("", result.Name);
            Assert.AreEqual("", customer.Name);

            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
