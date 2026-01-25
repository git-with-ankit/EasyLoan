using EasyLoan.Business.Services;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.Customer;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    public class CustomerService_GetProfileTests
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
        }
        [TestMethod]
        public async Task GetProfileAsync_WhenCustomerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act
            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetProfileAsync(customerId));
        }
        [TestMethod]
        public async Task UpdateProfileAsync_WhenAllFieldsProvided_UpdatesAllFields()
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
                PhoneNumber = "9123456789",
                AnnualSalary = 1000000
            };

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _service.UpdateProfileAsync(customerId, dto);

            // Assert
            Assert.AreEqual("New Name", result.Name);
            Assert.AreEqual("9123456789", result.PhoneNumber);
            Assert.AreEqual(1000000, result.AnnualSalary);

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

            // Act
            var result = await _service.UpdateProfileAsync(customerId, dto);

            // Assert
            Assert.AreEqual("Existing Name", result.Name);
            Assert.AreEqual("9876543210", result.PhoneNumber);
            Assert.AreEqual(750000, result.AnnualSalary);

            _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        [TestMethod]
        public async Task UpdateProfileAsync_WhenCustomerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var dto = new UpdateCustomerProfileRequestDto();

            _customerRepoMock
                .Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.UpdateProfileAsync(customerId, dto));
        }

    }
}
