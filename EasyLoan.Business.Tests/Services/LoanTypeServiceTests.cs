using EasyLoan.Business.Exceptions;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.LoanType;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Tests.Services
{
    [TestClass]
    public class LoanTypeServiceTests
    {
        private Mock<ILoanTypeRepository> _repoMock;
        private LoanTypeService _service;

        [TestInitialize]
        public void Setup() 
        {
            _repoMock = new Mock<ILoanTypeRepository>(MockBehavior.Strict);
            _service = new LoanTypeService(_repoMock.Object);
        }
        [TestMethod]
        public async Task GetAllAsync_WhenLoanTypesExist_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<LoanType>
            {
                new LoanType
                {
                    Id = Guid.NewGuid(),
                    Name = "Home Loan",
                    InterestRate = 7.5m,
                    MinAmount = 100000,
                    MaxTenureInMonths = 240
                }
            };

            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Home Loan", result.First().Name);
        }
        [TestMethod]
        public async Task GetByIdAsync_ValidId_ReturnsLoanType()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(new LoanType
                {
                    Id = id,
                    Name = "Home Loan",
                    InterestRate = 7.5m,
                    MinAmount = 100000,
                    MaxTenureInMonths = 240
                });

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.AreEqual(id, result.Id);
        }
        [TestMethod]
        public async Task GetByIdAsync_InvalidId_ThrowsNotFoundException()
        {
            _repoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((LoanType?)null);

            await Assert.ThrowsExceptionAsync<NotFoundException>(
                () => _service.GetByIdAsync(Guid.NewGuid())
            );
        }
        [TestMethod]
        public async Task CreateLoanTypeAsync_ValidRequest_CallsAddAndSave()
        {
            // Arrange
            LoanType? capturedEntity = null;

            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<LoanType>()))
                .Callback<LoanType>(e => capturedEntity = e)
                .Returns(Task.CompletedTask);

            _repoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var request = new LoanTypeRequestDto
            {
                Name = " Home Loan ",
                InterestRate = 7.5m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };

            // Act
            var result = await _service.CreateLoanTypeAsync(request);

            // Assert
            Assert.IsNotNull(capturedEntity);
            Assert.AreEqual("Home Loan", capturedEntity!.Name); // trimmed
            Assert.AreEqual(result.Id, capturedEntity.Id);

            _repoMock.Verify(r => r.AddAsync(It.IsAny<LoanType>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        [TestMethod]
        public async Task UpdateLoanTypeAsync_ValidId_UpdatesLoanType()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new LoanType
            {
                Id = id,
                Name = "Home Loan",
                InterestRate = 7.5m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _repoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var update = new LoanTypeRequestDto
            {
                Name = "Home Loan",
                InterestRate = 6.9m,
                MinAmount = 150000,
                MaxTenureInMonths = 300
            };

            // Act
            var result = await _service.UpdateLoanTypeAsync(id, update);

            // Assert
            Assert.AreEqual(6.9m, result.InterestRate);
        }
        [TestMethod]
        public async Task PreviewEmiAsync_AmountBelowMin_ThrowsBusinessRuleViolation()
        {
            _repoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new LoanType
                {
                    Id = Guid.NewGuid(),
                    Name = "Home Loan",
                    InterestRate = 7.5m,
                    MinAmount = 100000,
                    MaxTenureInMonths = 240
                });

            await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(
                () => _service.PreviewEmiAsync(Guid.NewGuid(), 50000, 120)
            );
        }


    }

}
