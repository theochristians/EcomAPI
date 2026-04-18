using System.Security.Cryptography;

namespace EComAPI.Application.Auth.Common
{
    internal static class OtpCodeGenerator
    {
        public static string GenerateSixDigits()
        {
            Span<byte> randomBytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(randomBytes);

            var randomValue = BitConverter.ToUInt32(randomBytes) % 1_000_000;
            return randomValue.ToString("D6");
        }
    }
}
