using System.Security.Cryptography;
using System.Text;

namespace EComAPI.Application.Common.Security
{
    public static class TokenHelper
    {
        /// <summary>
        /// Generate a cryptographically secure random token
        /// </summary>
        public static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Hash a token using SHA256
        /// </summary>
        public static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}