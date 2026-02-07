using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.LoanType;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    public class LoanTypeServiceTests
    {
        private Mock<ILoanTypeRepository> _mockRepo = null!;
        private Mock<IEmiCalculatorService> _emiServiceMock = null!;
        private LoanTypeService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<ILoanTypeRepository>(MockBehavior.Strict);
            _emiServiceMock = new Mock<IEmiCalculatorService>(MockBehavior.Strict);

            _service = new LoanTypeService(_mockRepo.Object, _emiServiceMock.Object);
        }

        // ============================================================
        // GetAllAsync
        // ============================================================

        [TestMethod]
        public async Task GetAllAsync_WhenMultipleLoanTypesExist_ReturnsMappedLoanTypes()
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
                },
                new LoanType
                {
                    Id = Guid.NewGuid(),
                    Name = "Car Loan",
                    InterestRate = 8.2m,
                    MinAmount = 50000,
                    MaxTenureInMonths = 84
                }
            };

            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(entities[0].Id, result[0].Id);
            Assert.AreEqual("Home Loan", result[0].Name);
            Assert.AreEqual(7.5m, result[0].InterestRate);

            Assert.AreEqual(entities[1].Id, result[1].Id);
            Assert.AreEqual("Car Loan", result[1].Name);
            Assert.AreEqual(8.2m, result[1].InterestRate);

            _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenNoLoanTypesExist_ReturnsEmptyCollection()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<LoanType>());

            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        // ============================================================
        // GetByIdAsync
        // ============================================================

        [TestMethod]
        public async Task GetByIdAsync_WhenLoanTypeExists_ReturnsMappedLoanType()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var entity = new LoanType
            {
                Id = loanTypeId,
                Name = "Home Loan",
                InterestRate = 7.25m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };

            _mockRepo.Setup(r => r.GetByIdAsync(loanTypeId)).ReturnsAsync(entity);

            // Act
            var result = await _service.GetByIdAsync(loanTypeId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(entity.Id, result.Id);
            Assert.AreEqual(entity.Name, result.Name);
            Assert.AreEqual(entity.InterestRate, result.InterestRate);
            Assert.AreEqual(entity.MinAmount, result.MinAmount);
            Assert.AreEqual(entity.MaxTenureInMonths, result.MaxTenureInMonths);

            _mockRepo.Verify(r => r.GetByIdAsync(loanTypeId), Times.Once);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenLoanTypeDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            _mockRepo.Setup(r => r.GetByIdAsync(loanTypeId)).ReturnsAsync((LoanType?)null);

            // Act
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
                () => _service.GetByIdAsync(loanTypeId));

            // Assert
            Assert.AreEqual(ErrorMessages.LoanTypeNotFound, ex.Message);

            _mockRepo.Verify(r => r.GetByIdAsync(loanTypeId), Times.Once);
        }

        // ============================================================
        // CreateLoanTypeAsync
        // ============================================================

        [TestMethod]
        public async Task CreateLoanTypeAsync_WhenMinAmountExceedsMax_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var request = new LoanTypeRequestDto
            {
                Name = "Test",
                InterestRate = 7,
                MinAmount = BusinessConstants.MaximumLoanAmount + 1,
                MaxTenureInMonths = 120
            };

            // Act
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(
                () => _service.CreateLoanTypeAsync(request));

            // Assert
            Assert.AreEqual(ErrorMessages.ExceededMaxAmount, ex.Message);

            _mockRepo.Verify(r => r.AddAsync(It.IsAny<LoanType>()), Times.Never);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task CreateLoanTypeAsync_WhenTenureExceedsMax_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var request = new LoanTypeRequestDto
            {
                Name = "Test",
                InterestRate = 7,
                MinAmount = 10000,
                MaxTenureInMonths = BusinessConstants.MaximumTenureInMonthsAllowed + 1
            };

            // Act
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(
                () => _service.CreateLoanTypeAsync(request));

            // Assert
            Assert.AreEqual(ErrorMessages.ExceededMaxTenure, ex.Message);

            _mockRepo.Verify(r => r.AddAsync(It.IsAny<LoanType>()), Times.Never);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task CreateLoanTypeAsync_ValidRequest_PersistsLoanTypeAndReturnsDto()
        {
            // Arrange
            var request = new LoanTypeRequestDto
            {
                Name = "  Home Loan  ",
                InterestRate = 7.5m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };

            LoanType? capturedEntity = null;

            _mockRepo
                .Setup(r => r.AddAsync(It.IsAny<LoanType>()))
                .Callback<LoanType>(lt => capturedEntity = lt)
                .Returns(Task.CompletedTask);

            _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateLoanTypeAsync(request);

            // Assert DTO
            Assert.IsNotNull(result);
            Assert.AreNotEqual(Guid.Empty, result.Id);
            Assert.AreEqual("Home Loan", result.Name); // trimmed
            Assert.AreEqual(request.InterestRate, result.InterestRate);
            Assert.AreEqual(request.MinAmount, result.MinAmount);
            Assert.AreEqual(request.MaxTenureInMonths, result.MaxTenureInMonths);

            // Assert Entity
            Assert.IsNotNull(capturedEntity);
            Assert.AreEqual(result.Id, capturedEntity!.Id);
            Assert.AreEqual("Home Loan", capturedEntity.Name);

            _mockRepo.Verify(r => r.AddAsync(It.IsAny<LoanType>()), Times.Once);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ============================================================
        // UpdateLoanTypeAsync
        // ============================================================

        [TestMethod]
        public async Task UpdateLoanTypeAsync_WhenLoanTypeDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            _mockRepo.Setup(r => r.GetByIdAsync(loanTypeId)).ReturnsAsync((LoanType?)null);

            var request = new UpdateLoanTypeRequestDto
            {
                InterestRate = 8.0m,
                MinAmount = 120000,
                MaxTenureInMonths = 200
            };

            // Act
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
                () => _service.UpdateLoanTypeAsync(loanTypeId, request));

            // Assert
            Assert.AreEqual(ErrorMessages.LoanTypeNotFound, ex.Message);

            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task UpdateLoanTypeAsync_WhenMinAmountExceedsMax_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var loanType = new LoanType
            {
                Id = loanTypeId,
                Name = "Home Loan",
                InterestRate = 7.5m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };

            _mockRepo.Setup(r => r.GetByIdAsync(loanTypeId)).ReturnsAsync(loanType);

            var request = new UpdateLoanTypeRequestDto
            {
                MinAmount = BusinessConstants.MaximumLoanAmount + 10
            };

            // Act
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(
                () => _service.UpdateLoanTypeAsync(loanTypeId, request));

            // Assert
            Assert.AreEqual(ErrorMessages.ExceededMaxAmount, ex.Message);

            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task UpdateLoanTypeAsync_WhenTenureExceedsMax_ThrowsBusinessRuleViolation()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var loanType = new LoanType
            {
                Id = loanTypeId,
                Name = "Home Loan",
                InterestRate = 7.5m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };

            _mockRepo.Setup(r => r.GetByIdAsync(loanTypeId)).ReturnsAsync(loanType);

            var request = new UpdateLoanTypeRequestDto
            {
                MaxTenureInMonths = BusinessConstants.MaximumTenureInMonthsAllowed + 1
            };

            // Act
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(
                () => _service.UpdateLoanTypeAsync(loanTypeId, request));

            // Assert
            Assert.AreEqual(ErrorMessages.ExceededMaxTenure, ex.Message);

            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task UpdateLoanTypeAsync_ValidRequest_UpdatesFieldsAndSaves()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var loanType = new LoanType
            {
                Id = loanTypeId,
                Name = "Home Loan",
                InterestRate = 7.5m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };

            _mockRepo.Setup(r => r.GetByIdAsync(loanTypeId)).ReturnsAsync(loanType);
            _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var request = new UpdateLoanTypeRequestDto
            {
                InterestRate = 10.5m,
                MinAmount = 150000,
                MaxTenureInMonths = 180
            };

            // Act
            var result = await _service.UpdateLoanTypeAsync(loanTypeId, request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(loanTypeId, result.Id);
            Assert.AreEqual("Home Loan", result.Name);

            Assert.AreEqual(10.5m, result.InterestRate);
            Assert.AreEqual(150000m, result.MinAmount);
            Assert.AreEqual(180, result.MaxTenureInMonths);

            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateLoanTypeAsync_WhenDtoFieldsAreNull_PreservesExistingValues()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var loanType = new LoanType
            {
                Id = loanTypeId,
                Name = "Home Loan",
                InterestRate = 7.5m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };

            _mockRepo.Setup(r => r.GetByIdAsync(loanTypeId)).ReturnsAsync(loanType);
            _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var request = new UpdateLoanTypeRequestDto
            {
                InterestRate = null,
                MinAmount = null,
                MaxTenureInMonths = null
            };

            // Act
            var result = await _service.UpdateLoanTypeAsync(loanTypeId, request);

            // Assert
            Assert.AreEqual(7.5m, result.InterestRate);
            Assert.AreEqual(100000m, result.MinAmount);
            Assert.AreEqual(240, result.MaxTenureInMonths);

            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ============================================================
        // GetLoanTypesAsync
        // ============================================================

        [TestMethod]
        public async Task GetLoanTypesAsync_WhenLoanTypesExist_ReturnsMappedLoanTypes()
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
                },
                new LoanType
                {
                    Id = Guid.NewGuid(),
                    Name = "Car Loan",
                    InterestRate = 8.2m,
                    MinAmount = 50000,
                    MaxTenureInMonths = 84
                }
            };

            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

            // Act
            var result = (await _service.GetLoanTypesAsync()).ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(entities[0].Id, result[0].Id);
            Assert.AreEqual(entities[1].Id, result[1].Id);

            _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetLoanTypesAsync_WhenNoLoanTypesExist_ReturnsEmptyCollection()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<LoanType>());

            // Act
            var result = (await _service.GetLoanTypesAsync()).ToList();

            // Assert
            Assert.AreEqual(0, result.Count);
            _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        // ============================================================
        // PreviewEmiAsync (PagedResponseDto)
        // ============================================================

        [TestMethod]
        public async Task PreviewEmiAsync_WhenLoanTypeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            _mockRepo.Setup(r => r.GetByIdAsync(loanTypeId)).ReturnsAsync((LoanType?)null);

            // Act
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
                () => _service.PreviewEmiAsync(
                    loanTypeId,
                    amount: 100000,
                    tenureInMonths: 24,
                    pageNumber: 1,
                    pageSize: 10));

            // Assert
            Assert.AreEqual(ErrorMessages.LoanTypeNotFound, ex.Message);
        }

        [TestMethod]
        public async Task PreviewEmiAsync_WhenTenureExceedsMax_ThrowsBusinessRuleViolationException()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var loanType = new LoanType
            {
                Id = loanTypeId,
                InterestRate = 8.0m,
                MinAmount = 50000,
                MaxTenureInMonths = 60
            };

            _mockRepo.Setup(r => r.GetByIdAsync(loanTypeId)).ReturnsAsync(loanType);

            // Act
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(
                () => _service.PreviewEmiAsync(
                    loanTypeId,
                    amount: 100000,
                    tenureInMonths: 120,
                    pageNumber: 1,
                    pageSize: 10));

            // Assert
            Assert.AreEqual(ErrorMessages.ExceededMaxTenure, ex.Message);
        }

        [TestMethod]
        public async Task PreviewEmiAsync_WhenAmountBelowMin_ThrowsBusinessRuleViolationException()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var loanType = new LoanType
            {
                Id = loanTypeId,
                InterestRate = 7.5m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };

            _mockRepo.Setup(r => r.GetByIdAsync(loanTypeId)).ReturnsAsync(loanType);

            // Act
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(
                () => _service.PreviewEmiAsync(
                    loanTypeId,
                    amount: 50000,
                    tenureInMonths: 60,
                    pageNumber: 1,
                    pageSize: 10));

            // Assert
            Assert.AreEqual(ErrorMessages.AmountLessThanMinAmount, ex.Message);
        }

        [TestMethod]
        public async Task PreviewEmiAsync_ValidInput_ReturnsPagedSchedule_AndMapsPagingCorrectly()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var loanType = new LoanType
            {
                Id = loanTypeId,
                MaxTenureInMonths = 24,
                MinAmount = 50000,
                InterestRate = 12
            };

            // Create a fake schedule of 12 EMIs
            var schedule = Enumerable.Range(1, 12)
                .Select(i => new EmiScheduleItemResponseDto
                {
                    EmiNumber = i,
                    DueDate = DateTime.UtcNow.AddMonths(i),
                    InterestComponent = 100,
                    PrincipalComponent = 900,
                    TotalEmiAmount = 1000,
                    PrincipalRemainingAfterPayment = 100000 - (i * 900)
                })
                .ToList();

            _mockRepo.Setup(r => r.GetByIdAsync(loanTypeId)).ReturnsAsync(loanType);

            _emiServiceMock
                .Setup(s => s.GenerateSchedule(
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<int>(),
                    It.IsAny<DateTime>()))
                .Returns(schedule);

            // Act
            var result = await _service.PreviewEmiAsync(
                loanTypeId,
                amount: 100000,
                tenureInMonths: 12,
                pageNumber: 2,
                pageSize: 5);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(2, result.PageNumber);
            Assert.AreEqual(5, result.PageSize);

            Assert.AreEqual(12, result.TotalCount);
            Assert.AreEqual(3, result.TotalPages); // 12 items, page size 5 => 3 pages

            Assert.AreEqual(5, result.Items.Count);
            Assert.AreEqual(6, result.Items.First().EmiNumber); // page 2 starts at 6
            Assert.AreEqual(10, result.Items.Last().EmiNumber); // ends at 10

            _mockRepo.Verify(r => r.GetByIdAsync(loanTypeId), Times.Once);

            _emiServiceMock.Verify(s => s.GenerateSchedule(
                100000,
                loanType.InterestRate,
                12,
                It.IsAny<DateTime>()), Times.Once);
        }
    }
}
