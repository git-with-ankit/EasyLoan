using EasyLoan.Business.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    public class EmiCalculatorServiceTests
    {
        private EmiCalculatorService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _service = new EmiCalculatorService();
        }

        [TestMethod]
        public void GenerateSchedule_WhenValidInputs_ReturnsCorrectNumberOfEmis_AndCorrectEmiNumbers()
        {
            // Arrange
            var principal = 100000m;
            var interestRate = 12m; // 12% annual
            var tenure = 12;
            var startDate = new DateTime(2024, 1, 1);

            // Act
            var result = _service.GenerateSchedule(
                principal,
                interestRate,
                tenure,
                startDate).ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(12, result.Count);

            Assert.AreEqual(1, result.First().EmiNumber);
            Assert.AreEqual(12, result.Last().EmiNumber);

            for (int i = 0; i < tenure; i++)
            {
                Assert.AreEqual(i + 1, result[i].EmiNumber);
            }
        }

        [TestMethod]
        public void GenerateSchedule_WhenCalled_SetsCorrectDueDates()
        {

            // Arrange
            var startDate = new DateTime(2024, 1, 15);

            // Act
            var schedule = _service.GenerateSchedule(
                principal: 50000,
                annualInterestRate: 10,
                tenureInMonths: 3,
                startDate: startDate).ToList();

            // Assert
            Assert.AreEqual(startDate.AddMonths(1), schedule[0].DueDate);
            Assert.AreEqual(startDate.AddMonths(2), schedule[1].DueDate);
            Assert.AreEqual(startDate.AddMonths(3), schedule[2].DueDate);
        }

        [TestMethod]
        public void GenerateSchedule_WhenCalled_TotalEmiAmountIsConstantAcrossSchedule()
        {
            // Arrange
            var schedule = _service.GenerateSchedule(
                principal: 200000,
                annualInterestRate: 9,
                tenureInMonths: 24,
                startDate: DateTime.UtcNow).ToList();

            var firstEmiAmount = schedule.First().TotalEmiAmount;

            // Assert
            Assert.IsTrue(schedule.All(e => e.TotalEmiAmount == firstEmiAmount));
        }

        [TestMethod]
        public void GenerateSchedule_WhenCalled_PrincipalRemainingNeverBecomesNegative()
        {

            // Arrange
            var schedule = _service.GenerateSchedule(
                principal: 100000,
                annualInterestRate: 12,
                tenureInMonths: 12,
                startDate: DateTime.UtcNow).ToList();

            // Assert
            Assert.IsTrue(schedule.All(x => x.PrincipalRemainingAfterPayment >= 0m));
        }

        [TestMethod]
        public void GenerateSchedule_WhenCalled_PrincipalReducesToZeroAtEnd()
        {
            // Arrange
            var schedule = _service.GenerateSchedule(
                principal: 100000,
                annualInterestRate: 12,
                tenureInMonths: 12,
                startDate: DateTime.UtcNow).ToList();

            // Assert
            var lastEmi = schedule.Last();

            Assert.AreEqual(0m, lastEmi.PrincipalRemainingAfterPayment);
        }

        [TestMethod]
        public void GenerateSchedule_WhenCalled_InterestComponentDecreasesOverTime()
        {
            // Arrange
            var schedule = _service.GenerateSchedule(
                principal: 300000,
                annualInterestRate: 10,
                tenureInMonths: 12,
                startDate: DateTime.UtcNow).ToList();

            // Assert
            Assert.IsTrue(
                schedule.First().InterestComponent >
                schedule.Last().InterestComponent);
        }

        [TestMethod]
        public void GenerateSchedule_WhenTenureIsOne_ReturnsSingleEmi_AndPrincipalBecomesZero()
        {
            // Arrange
            var schedule = _service.GenerateSchedule(
                principal: 50000,
                annualInterestRate: 10,
                tenureInMonths: 1,
                startDate: DateTime.UtcNow).ToList();

            // Assert
            Assert.AreEqual(1, schedule.Count);

            Assert.AreEqual(1, schedule[0].EmiNumber);

            Assert.AreEqual(0m, schedule[0].PrincipalRemainingAfterPayment);
        }

        [TestMethod]
        public void GenerateSchedule_WhenCalled_EmiBreakdownIsValid_TotalEqualsPrincipalPlusInterest_WithTolerance()
        {
            var schedule = _service.GenerateSchedule(
                principal: 150000,
                annualInterestRate: 11,
                tenureInMonths: 6,
                startDate: DateTime.UtcNow).ToList();

            foreach (var item in schedule)
            {
                var sum = Math.Round(item.PrincipalComponent + item.InterestComponent, 2);
                var total = Math.Round(item.TotalEmiAmount, 2);

                Assert.IsTrue(
                    Math.Abs(sum - total) <= 0.01m,
                    $"EMI {item.EmiNumber} mismatch: Principal+Interest={sum}, Total={total}");
            }
        }


        [TestMethod]
        public void GenerateSchedule_WhenCalled_PrincipalComponentNeverExceedsTotalEmi()
        {
            // Arrange
            var schedule = _service.GenerateSchedule(
                principal: 100000,
                annualInterestRate: 15,
                tenureInMonths: 12,
                startDate: DateTime.UtcNow).ToList();

            // Assert
            Assert.IsTrue(schedule.All(x => x.PrincipalComponent <= x.TotalEmiAmount));
        }

        [TestMethod]
        public void GenerateSchedule_WhenCalled_LastEmi_PrincipalComponentEqualsRemainingPrincipal_BranchCoverage()
        {

            // Arrange
            var principal = 1000m;
            var interestRate = 1m;
            var tenure = 3;
            var startDate = new DateTime(2024, 1, 1);

            // Act
            var schedule = _service.GenerateSchedule(principal, interestRate, tenure, startDate).ToList();

            // Assert
            var last = schedule.Last();

            Assert.AreEqual(0m, last.PrincipalRemainingAfterPayment);
            Assert.IsTrue(last.PrincipalComponent > 0m);
        }
    }
}
