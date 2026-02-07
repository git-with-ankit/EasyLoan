using EasyLoan.Api.Controllers;
using EasyLoan.Dtos.Common;
using EasyLoan.Dtos.Loan;
using EasyLoan.Dtos.LoanType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EasyLoan.UnitTest.Controllers
{
    [TestClass]
    public class LoanTypesControllerTests
    {
        private Mock<ILoanTypeService> _mockLoanTypeService = null!;
        private LoanTypesController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLoanTypeService = new Mock<ILoanTypeService>(MockBehavior.Strict);
            _controller = new LoanTypesController(_mockLoanTypeService.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [TestMethod]
        public async Task GetAllLoanTypes_ValidRequest_ReturnsOkWithLoanTypes()
        {
            // Arrange
            var loanTypes = new List<LoanTypeResponseDto>
            {
                new LoanTypeResponseDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Home Loan",
                    InterestRate = 7.5m,
                    MinAmount = 100000,
                    MaxTenureInMonths = 240
                },
                new LoanTypeResponseDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Car Loan",
                    InterestRate = 8.2m,
                    MinAmount = 50000,
                    MaxTenureInMonths = 84
                }
            };

            _mockLoanTypeService
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(loanTypes);

            // Act
            var result = await _controller.GetAllLoanTypes();

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var data = ok.Value as IEnumerable<LoanTypeResponseDto>;
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Count());

            _mockLoanTypeService.Verify(s => s.GetAllAsync(), Times.Once);
            _mockLoanTypeService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetAllLoanTypes_WhenEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            _mockLoanTypeService
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<LoanTypeResponseDto>());

            // Act
            var result = await _controller.GetAllLoanTypes();

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var data = ok.Value as IEnumerable<LoanTypeResponseDto>;
            Assert.IsNotNull(data);
            Assert.AreEqual(0, data.Count());

            _mockLoanTypeService.Verify(s => s.GetAllAsync(), Times.Once);
            _mockLoanTypeService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task GetLoanTypesById_ExistingId_ReturnsOkWithLoanType()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var loanType = new LoanTypeResponseDto
            {
                Id = loanTypeId,
                Name = "Home Loan",
                InterestRate = 7.5m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };

            _mockLoanTypeService
                .Setup(s => s.GetByIdAsync(loanTypeId))
                .ReturnsAsync(loanType);

            // Act
            var result = await _controller.GetLoanTypesById(loanTypeId);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var data = ok.Value as LoanTypeResponseDto;
            Assert.IsNotNull(data);

            Assert.AreEqual(loanTypeId, data.Id);
            Assert.AreEqual("Home Loan", data.Name);
            Assert.AreEqual(7.5m, data.InterestRate);
            Assert.AreEqual(100000, data.MinAmount);
            Assert.AreEqual(240, data.MaxTenureInMonths);

            _mockLoanTypeService.Verify(s => s.GetByIdAsync(loanTypeId), Times.Once);
            _mockLoanTypeService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task CreateLoanType_ValidRequest_ReturnsCreatedWithLoanType()
        {
            // Arrange
            var request = new LoanTypeRequestDto
            {
                Name = "Home Loan",
                InterestRate = 7.5m,
                MinAmount = 100000,
                MaxTenureInMonths = 240
            };

            var createdLoanType = new LoanTypeResponseDto
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                InterestRate = request.InterestRate,
                MinAmount = request.MinAmount,
                MaxTenureInMonths = request.MaxTenureInMonths
            };

            _mockLoanTypeService
                .Setup(s => s.CreateLoanTypeAsync(request))
                .ReturnsAsync(createdLoanType);

            // Act
            var result = await _controller.CreateLoanType(request);

            // Assert
            var created = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(created);
            Assert.AreEqual(StatusCodes.Status201Created, created.StatusCode);

            Assert.AreEqual(nameof(_controller.GetLoanTypesById), created.ActionName);

            Assert.IsNotNull(created.RouteValues);
            Assert.AreEqual(createdLoanType.Id, created.RouteValues["loanTypeId"]);

            var dto = created.Value as LoanTypeResponseDto;
            Assert.IsNotNull(dto);
            Assert.AreEqual(createdLoanType.Id, dto.Id);

            _mockLoanTypeService.Verify(s => s.CreateLoanTypeAsync(request), Times.Once);
            _mockLoanTypeService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task UpdateLoanType_ValidRequest_ReturnsOkWithUpdatedLoanType()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var request = new UpdateLoanTypeRequestDto
            {
                InterestRate = 6.9m,
                MinAmount = 150000,
                MaxTenureInMonths = 300
            };

            var updatedLoanType = new LoanTypeResponseDto
            {
                Id = loanTypeId,
                Name = "Updated Loan",
                InterestRate = request.InterestRate!.Value,
                MinAmount = request.MinAmount!.Value,
                MaxTenureInMonths = request.MaxTenureInMonths!.Value
            };

            _mockLoanTypeService
                .Setup(s => s.UpdateLoanTypeAsync(loanTypeId, request))
                .ReturnsAsync(updatedLoanType);

            // Act
            var result = await _controller.CreateLoanType(loanTypeId, request);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var dto = ok.Value as LoanTypeResponseDto;
            Assert.IsNotNull(dto);

            Assert.AreEqual(loanTypeId, dto.Id);
            Assert.AreEqual(6.9m, dto.InterestRate);
            Assert.AreEqual(150000, dto.MinAmount);
            Assert.AreEqual(300, dto.MaxTenureInMonths);

            _mockLoanTypeService.Verify(s => s.UpdateLoanTypeAsync(loanTypeId, request), Times.Once);
            _mockLoanTypeService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task PreviewEmiPlan_PageNumberLessThan1_ReturnsBadRequest()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var query = new PreviewEmiQueryDto
            {
                Amount = 500000m,
                TenureInMonths = 240
            };

            // Act
            var result = await _controller.PreviewEmiPlan(
                loanTypeId,
                query,
                pageNumber: 0,
                pageSize: 10);

            // Assert
            var bad = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            Assert.AreEqual(StatusCodes.Status400BadRequest, bad.StatusCode);
            Assert.AreEqual("Page number must be greater than 0", bad.Value);

            _mockLoanTypeService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task PreviewEmiPlan_PageSizeLessThan1_ReturnsBadRequest()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var query = new PreviewEmiQueryDto
            {
                Amount = 500000m,
                TenureInMonths = 240
            };

            // Act
            var result = await _controller.PreviewEmiPlan(
                loanTypeId,
                query,
                pageNumber: 1,
                pageSize: 0);

            // Assert
            var bad = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            Assert.AreEqual(StatusCodes.Status400BadRequest, bad.StatusCode);
            Assert.AreEqual("Page size must be between 1 and 100", bad.Value);

            _mockLoanTypeService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task PreviewEmiPlan_PageSizeGreaterThan100_ReturnsBadRequest()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var query = new PreviewEmiQueryDto
            {
                Amount = 500000m,
                TenureInMonths = 240
            };

            // Act
            var result = await _controller.PreviewEmiPlan(
                loanTypeId,
                query,
                pageNumber: 1,
                pageSize: 101);

            // Assert
            var bad = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            Assert.AreEqual(StatusCodes.Status400BadRequest, bad.StatusCode);
            Assert.AreEqual("Page size must be between 1 and 100", bad.Value);

            _mockLoanTypeService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task PreviewEmiPlan_ValidRequest_ReturnsOkWithPagedSchedule()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var query = new PreviewEmiQueryDto
            {
                Amount = 500000m,
                TenureInMonths = 240
            };

            var scheduleItems = new List<EmiScheduleItemResponseDto>
            {
                new EmiScheduleItemResponseDto
                {
                    EmiNumber = 1,
                    DueDate = DateTime.UtcNow.AddMonths(1),
                    PrincipalComponent = 1200,
                    InterestComponent = 3500,
                    TotalEmiAmount = 4700,
                    PrincipalRemainingAfterPayment = 498800
                },
                new EmiScheduleItemResponseDto
                {
                    EmiNumber = 2,
                    DueDate = DateTime.UtcNow.AddMonths(2),
                    PrincipalComponent = 1250,
                    InterestComponent = 3450,
                    TotalEmiAmount = 4700,
                    PrincipalRemainingAfterPayment = 497550
                }
            };

            var paged = new PagedResponseDto<EmiScheduleItemResponseDto>
            {
                Items = scheduleItems,
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 2,
                TotalPages = 1
            };

            _mockLoanTypeService
                .Setup(s => s.PreviewEmiAsync(
                    loanTypeId,
                    query.Amount,
                    query.TenureInMonths,
                    1,
                    10))
                .ReturnsAsync(paged);

            // Act
            var result = await _controller.PreviewEmiPlan(
                loanTypeId,
                query,
                pageNumber: 1,
                pageSize: 10);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

            var dto = ok.Value as PagedResponseDto<EmiScheduleItemResponseDto>;
            Assert.IsNotNull(dto);

            Assert.AreEqual(1, dto.PageNumber);
            Assert.AreEqual(10, dto.PageSize);
            Assert.AreEqual(2, dto.TotalCount);
            Assert.AreEqual(1, dto.TotalPages);

            Assert.IsNotNull(dto.Items);
            Assert.AreEqual(2, dto.Items.Count());
            Assert.AreEqual(1, dto.Items.First().EmiNumber);

            _mockLoanTypeService.Verify(
                s => s.PreviewEmiAsync(loanTypeId, query.Amount, query.TenureInMonths, 1, 10),
                Times.Once);

            _mockLoanTypeService.VerifyNoOtherCalls();
        }
    }
}
