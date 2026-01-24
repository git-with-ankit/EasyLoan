using EasyLoan.Business.Enums;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Services;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
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
        public async Task GetProfileAsync_WhenEmployeeNotFound_ThrowsNotFoundException()
        {
            var employeeId = Guid.NewGuid();

            _mockEmployeeRepo
                .Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync((Employee?)null);

            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.GetProfileAsync(employeeId));
        }
        [TestMethod]
        public async Task GetProfileAsync_WhenEmployeeIdMismatch_ThrowsForbiddenException()
        {
            var employeeId = Guid.NewGuid();

            var employee = new Employee
            {
                Id = Guid.NewGuid(), // different ID
                Name = "John",
                Email = "john@test.com",
                PhoneNumber = "123",
                Role = EmployeeRole.Manager
            };

            _mockEmployeeRepo
                .Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);

            await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.GetProfileAsync(employeeId));
        }
        [TestMethod]
        public async Task GetProfileAsync_WhenAuthorized_ReturnsEmployeeProfile()
        {
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

            var result = await _service.GetProfileAsync(employeeId);

            Assert.IsNotNull(result);
            Assert.AreEqual(employee.Name, result.Name);
            Assert.AreEqual(employee.Email, result.Email);
            Assert.AreEqual(employee.PhoneNumber, result.PhoneNumber);
            Assert.AreEqual(employee.Role, result.Role);
        }
        [TestMethod]
        public async Task UpdateProfileAsync_WhenEmployeeNotFound_ThrowsNotFoundException()
        {
            var employeeId = Guid.NewGuid();

            _mockEmployeeRepo
                .Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync((Employee?)null);

            var dto = new UpdateEmployeeProfileRequestDto
            {
                Name = "New Name",
                PhoneNumber = "999"
            };

            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                _service.UpdateProfileAsync(employeeId, dto));
        }
        [TestMethod]
        public async Task UpdateProfileAsync_WhenEmployeeIdMismatch_ThrowsForbiddenException()
        {
            var employeeId = Guid.NewGuid();

            var employee = new Employee
            {
                Id = Guid.NewGuid(), // mismatch
                Name = "John"
            };

            _mockEmployeeRepo
                .Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);

            var dto = new UpdateEmployeeProfileRequestDto
            {
                Name = "New Name"
            };

            await Assert.ThrowsExceptionAsync<ForbiddenException>(() =>
                _service.UpdateProfileAsync(employeeId, dto));
        }
        [TestMethod]
        public async Task UpdateProfileAsync_WhenValid_UpdatesAndReturnsProfile()
        {
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

            //_mockEmployeeRepo
            //    .Setup(r => r.UpdateAsync(employee))
            //    .Returns(Task.CompletedTask);

            _mockEmployeeRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _service.UpdateProfileAsync(employeeId, dto);

            Assert.IsNotNull(result);
            Assert.AreEqual("Jane", result.Name);          // trimmed
            Assert.AreEqual("999", result.PhoneNumber);    // trimmed
            Assert.AreEqual(employee.Email, result.Email);
            Assert.AreEqual(employee.Role, result.Role);

            //_mockEmployeeRepo.Verify(r => r.UpdateAsync(employee), Times.Once);
            _mockEmployeeRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

    }
}
