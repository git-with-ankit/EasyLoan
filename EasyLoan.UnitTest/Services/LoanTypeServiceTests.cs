using EasyLoan.Business.Constants;
using EasyLoan.Business.Exceptions;
using EasyLoan.Business.Interfaces;
using EasyLoan.DataAccess.Interfaces;
using EasyLoan.DataAccess.Models;
using EasyLoan.Dtos.LoanType;
using Moq;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    public class LoanTypeServiceTests
    {
        private Mock<ILoanTypeRepository> _mockRepo = null!;
        private Mock<IEmiCalculatorService> _emiServiceMock;
        private LoanTypeService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<ILoanTypeRepository>(MockBehavior.Strict);
            _emiServiceMock = new Mock<IEmiCalculatorService>(MockBehavior.Strict);
            _service = new LoanTypeService(_mockRepo.Object, _emiServiceMock.Object);
        }

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

            _mockRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());

            var first = result.First();
            Assert.AreEqual("Home Loan", first.Name);
            Assert.AreEqual(7.5m, first.InterestRate);
        }
        [TestMethod]
        public async Task GetAllAsync_WhenSingleLoanTypeExists_ReturnsSingleMappedLoanType()
        {
            // Arrange
            var entity = new LoanType
            {
                Id = Guid.NewGuid(),
                Name = "Education Loan",
                InterestRate = 6.5m,
                MinAmount = 20000,
                MaxTenureInMonths = 120
            };

            _mockRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<LoanType> { entity });

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.AreEqual(1, result.Count());

            var dto = result.First();
            Assert.AreEqual(entity.Id, dto.Id);
            Assert.AreEqual(entity.Name, dto.Name);
        }
        [TestMethod]
        public async Task GetAllAsync_WhenNoLoanTypesExist_ReturnsEmptyCollection()
        {
            // Arrange
            _mockRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<LoanType>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }
        //[TestMethod]
        //public async Task GetAllAsync_WhenNoLoanTypes_ThrowsException()
        //{
        //    // Arrange
        //    _mockRepo
        //        .Setup(r => r.GetAllAsync())
        //        .ReturnsAsync((IEnumerable<LoanType>)null!);

        //    // Act & Assert
        //    await Assert.ThrowsExceptionAsync<NullReferenceException>(
        //        () => _service.GetAllAsync()
        //    );
        //} //repo never returns null

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

            _mockRepo
                .Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync(entity);

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

            _mockRepo
                .Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync((LoanType?)null);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
                () => _service.GetByIdAsync(loanTypeId));

            Assert.AreEqual(ErrorMessages.LoanTypeNotFound, ex.Message);

            _mockRepo.Verify(r => r.GetByIdAsync(loanTypeId), Times.Once);
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

            _mockRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateLoanTypeAsync(request);

            // Assert – returned DTO
            Assert.IsNotNull(result);
            Assert.AreEqual(request.InterestRate, result.InterestRate);
            Assert.AreEqual(request.MinAmount, result.MinAmount);
            Assert.AreEqual(request.MaxTenureInMonths, result.MaxTenureInMonths);
            Assert.AreNotEqual(Guid.Empty, result.Id);

            // Assert – persisted entity
            Assert.IsNotNull(capturedEntity);
            Assert.AreEqual(result.Id, capturedEntity!.Id);

            // Assert – interactions
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<LoanType>()), Times.Once);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        [TestMethod]
        public async Task UpdateLoanTypeAsync_WhenLoanTypeExists_UpdatesSuccessfully()
        {
            // Arrange
            var loanType = new LoanType
            {
                Id = Guid.NewGuid(),
                Name = "Home Loan"
            };

            _mockRepo
                .Setup(r => r.GetByIdAsync(loanType.Id))
                .ReturnsAsync(loanType);

            _mockRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var dto = new UpdateLoanTypeRequestDto
            {
                InterestRate = 12
            };

            // Act
            var result = await _service.UpdateLoanTypeAsync(loanType.Id, dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Home Loan", loanType.Name);
        }

        [TestMethod]
        public async Task UpdateLoanTypeAsync_WhenLoanTypeDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var request = new UpdateLoanTypeRequestDto
            {
                InterestRate = 8.0m,
                MinAmount = 120000,
                MaxTenureInMonths = 200
            };

            _mockRepo
                .Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync((LoanType?)null);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
                () => _service.UpdateLoanTypeAsync(loanTypeId, request));

            Assert.AreEqual(ErrorMessages.LoanTypeNotFound, ex.Message);

            // Ensure no update/save happens
            //_mockRepo.Verify(r => r.UpdateAsync(It.IsAny<LoanType>()), Times.Never);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
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

            _mockRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            // Act
            var result = await _service.GetLoanTypesAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());

            var first = result.First();
            Assert.AreEqual(entities[0].Id, first.Id);
            Assert.AreEqual(entities[0].Name, first.Name);
            Assert.AreEqual(entities[0].InterestRate, first.InterestRate);
        }
        [TestMethod]
        public async Task GetLoanTypesAsync_WhenNoLoanTypesExist_ReturnsEmptyCollection()
        {
            // Arrange
            _mockRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<LoanType>());

            // Act
            var result = await _service.GetLoanTypesAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }
        [TestMethod]
        public async Task PreviewEmiAsync_WhenLoanTypeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            _mockRepo
                .Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync((LoanType?)null);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
                () => _service.PreviewEmiAsync(loanTypeId, 100000, 60));

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

            _mockRepo
                .Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync(loanType);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(
                () => _service.PreviewEmiAsync(loanTypeId, 100000, 120));

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

            _mockRepo
                .Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync(loanType);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleViolationException>(
                () => _service.PreviewEmiAsync(loanTypeId, 50000, 60));

            Assert.AreEqual(ErrorMessages.AmountLessThanMinAmount, ex.Message);
        }

        [TestMethod]
        public async Task PreviewEmiAsync_ValidInput_ReturnsSchedule()
        {
            var loanTypeId = Guid.NewGuid();

            var loanType = new LoanType
            {
                Id = loanTypeId,
                MaxTenureInMonths = 24,
                MinAmount = 50000,
                InterestRate = 12
            };

            var schedule = new List<EmiScheduleItemResponseDto>
            {
                new EmiScheduleItemResponseDto
                {
                    EmiNumber = 1,
                    TotalEmiAmount = 5000,
                    InterestComponent = 1000,
                    PrincipalComponent = 4000,
                    PrincipalRemainingAfterPayment = 96000
                }
            };

            _mockRepo
                .Setup(r => r.GetByIdAsync(loanTypeId))
                .ReturnsAsync(loanType);

            _emiServiceMock
                .Setup(s => s.GenerateSchedule(
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<int>(),
                    It.IsAny<DateTime>()))
                .Returns(schedule);

            var result = await _service.PreviewEmiAsync(loanTypeId, 100000, 24);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
        }

    }
}