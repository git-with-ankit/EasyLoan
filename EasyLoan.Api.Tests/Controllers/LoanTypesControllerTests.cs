using EasyLoan.Api.Controllers;
using EasyLoan.Business.Interfaces;
using EasyLoan.Dtos.Loan;
using EasyLoan.Dtos.LoanApplication;
using EasyLoan.Dtos.LoanType;
using EasyLoan.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace EasyLoan.UnitTest.Controllers
{
    [TestClass]
    public class LoanTypesControllerTests
    {
        private readonly Mock<ILoanTypeService> _mockLoanTypeService;
        private readonly LoanTypesController _controller;

        public LoanTypesControllerTests()
        {
            _mockLoanTypeService = new Mock<ILoanTypeService>();
            _controller = new LoanTypesController(_mockLoanTypeService.Object);
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
            Assert.IsNotNull(result);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var data = okResult.Value as IEnumerable<LoanTypeResponseDto>;
            Assert.IsNotNull(data);

            Assert.AreEqual(2, data.Count());
            Assert.AreEqual("Home Loan", data.First().Name);
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
            Assert.IsNotNull(result);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var data = okResult.Value as LoanTypeResponseDto;
            Assert.IsNotNull(data);

            Assert.AreEqual(loanTypeId, data.Id);
            Assert.AreEqual("Home Loan", data.Name);
            Assert.AreEqual(7.5m, data.InterestRate);
            Assert.AreEqual(100000, data.MinAmount);
            Assert.AreEqual(240, data.MaxTenureInMonths);
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
            Assert.IsNotNull(result);

            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);

            Assert.AreEqual(nameof(_controller.GetLoanTypesById), createdResult.ActionName);
            Assert.IsNotNull(createdResult.RouteValues);
            Assert.AreEqual(createdLoanType.Id, createdResult.RouteValues["loanTypeId"]);

            var data = createdResult.Value as LoanTypeResponseDto;
            Assert.IsNotNull(data);

            Assert.AreEqual(createdLoanType.Id, data.Id);
            Assert.AreEqual("Home Loan", data.Name);
            Assert.AreEqual(7.5m, data.InterestRate);
            Assert.AreEqual(100000, data.MinAmount);
            Assert.AreEqual(240, data.MaxTenureInMonths);
        }

        [TestMethod]
        public async Task UpdateLoanType_ValidRequest_ReturnsOkWithUpdatedLoanType()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var request = new LoanTypeRequestDto
            {
                Name = "Updated Home Loan",
                InterestRate = 6.9m,
                MinAmount = 150000,
                MaxTenureInMonths = 300
            };

            var updatedLoanType = new LoanTypeResponseDto
            {
                Id = loanTypeId,
                Name = request.Name,
                InterestRate = request.InterestRate,
                MinAmount = request.MinAmount,
                MaxTenureInMonths = request.MaxTenureInMonths
            };

            _mockLoanTypeService
                .Setup(s => s.UpdateLoanTypeAsync(loanTypeId, request))
                .ReturnsAsync(updatedLoanType);

            // Act
            var result = await _controller.CreateLoanType(loanTypeId, request);

            // Assert
            Assert.IsNotNull(result);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var data = okResult.Value as LoanTypeResponseDto;
            Assert.IsNotNull(data);

            Assert.AreEqual(loanTypeId, data.Id);
            Assert.AreEqual("Updated Home Loan", data.Name);
            Assert.AreEqual(6.9m, data.InterestRate);
            Assert.AreEqual(150000, data.MinAmount);
            Assert.AreEqual(300, data.MaxTenureInMonths);
        }

        [TestMethod]
        public async Task PreviewEmiPlan_ValidRequest_ReturnsOkWithEmiSchedule()
        {
            // Arrange
            var loanTypeId = Guid.NewGuid();

            var query = new PreviewEmiQueryDto
            {
                Amount = 500000m,
                TenureInMonths = 240
            };

             var emiSchedule = new List<EmiScheduleItemResponseDto>
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

            _mockLoanTypeService
                .Setup(s => s.PreviewEmiAsync(
                    loanTypeId,
                    query.Amount,
                    query.TenureInMonths))
                .ReturnsAsync(emiSchedule);

            // Act
            var result = await _controller.PreviewEmiPlan(loanTypeId, query);

            // Assert
            Assert.IsNotNull(result);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var data = okResult.Value as IEnumerable<EmiScheduleItemResponseDto>;
            Assert.IsNotNull(data);

            Assert.AreEqual(2, data.Count());
            Assert.AreEqual(1, data.First().EmiNumber);
            Assert.AreEqual(4700, data.First().TotalEmiAmount);
        }



    }
}
