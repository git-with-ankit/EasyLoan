using EasyLoan.Business.Interfaces;
using EasyLoan.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.UnitTest.Services
{
    [TestClass]
    public class PublicIdServiceTests
    {
        private IPublicIdService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _service = new PublicIdService();
        }
        [TestMethod]
        public void GenerateLoanNumber_WhenCalled_ReturnsValidLoanNumber()
        {
            // Act
            var result = _service.GenerateLoanNumber();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StartsWith("LN-"));
            Assert.AreEqual(11, result.Length);
        }
        [TestMethod]
        public void GenerateApplicationNumber_WhenCalled_ReturnsValidApplicationNumber()
        {
            // Act
            var result = _service.GenerateApplicationNumber();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StartsWith("LA-"));
            Assert.AreEqual(11, result.Length);
        }
        [TestMethod]
        public void GenerateLoanNumber_WhenCalled_ContainsOnlyBase62Characters()
        {
            // Act
            var loanNumber = _service.GenerateLoanNumber();
            var base62Part = loanNumber.Substring(3);

            const string base62Chars =
                "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            // Assert
            Assert.IsTrue(base62Part.All(c => base62Chars.Contains(c)));
        }
        [TestMethod]
        public void GenerateApplicationNumber_WhenCalled_ContainsOnlyBase62Characters()
        {
            // Act
            var applicationNumber = _service.GenerateApplicationNumber();
            var base62Part = applicationNumber.Substring(3);

            const string base62Chars =
                "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            // Assert
            Assert.IsTrue(base62Part.All(c => base62Chars.Contains(c)));
        }
        [TestMethod]
        public void GenerateLoanNumber_MultipleCalls_ReturnUniqueValues()
        {
            var set = new HashSet<string>();

            for (int i = 0; i < 1000; i++)
            {
                var value = _service.GenerateLoanNumber();
                Assert.IsTrue(set.Add(value));
            }
        }
        [TestMethod]
        public void GenerateApplicationNumber_MultipleCalls_ReturnUniqueValues()
        {
            var set = new HashSet<string>();

            for (int i = 0; i < 1000; i++)
            {
                var value = _service.GenerateApplicationNumber();
                Assert.IsTrue(set.Add(value));
            }
        }


    }

}
