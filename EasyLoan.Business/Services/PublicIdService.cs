using EasyLoan.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Business.Services
{
    public class PublicIdService : IPublicIdService
    {
        private static readonly char[] Chars =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();

        public string GenerateLoanNumber()
        {
            return $"LN-{GuidToBase62(Guid.NewGuid())}";  // LN-A7K9M2P4 (11 chars)
        }

        public string GenerateApplicationNumber()
        {
            return $"LA-{GuidToBase62(Guid.NewGuid())}";  // LA-X4N7Q2R8 (11 chars)
        }

        private static string GuidToBase62(Guid guid)
        {
            byte[] bytes = guid.ToByteArray();
            long num = BitConverter.ToInt64(bytes, 0);  // First 64 bits (unique)

            var result = new char[8];
            for (int i = 7; i >= 0; i--)
            {
                result[i] = Chars[num % 62];
                num /= 62;
            }
            return new string(result);
        }
    }
}
