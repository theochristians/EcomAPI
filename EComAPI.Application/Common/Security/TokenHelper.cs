using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EComAPI.Application.Common.Security
{
    public static class TokenHelper
    {
        private static readonly TimeSpan DefaultFallbackLifetime = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan DefaultMaxBlacklistLifetime = TimeSpan.FromHours(24);

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

        /// <summary>
        /// Resolve access token expiration with a safe upper bound.
        /// This protects blacklist storage from untrusted token payloads with extreme exp values.
        /// </summary>
        public static DateTime ResolveAccessTokenExpiry(
            string accessToken,
            TimeSpan? fallbackLifetime = null,
            TimeSpan? maxBlacklistLifetime = null)
        {
            var fallback = fallbackLifetime ?? DefaultFallbackLifetime;
            var maxLifetime = maxBlacklistLifetime ?? DefaultMaxBlacklistLifetime;
            var now = SecurityTime.UtcNow;
            var maxAllowedExpiresAt = now.Add(maxLifetime);

            try
            {
                var tokenParts = accessToken.Split('.');
                if (tokenParts.Length < 2)
                    return now.Add(fallback);

                var payload = tokenParts[1]
                    .Replace('-', '+')
                    .Replace('_', '/');

                switch (payload.Length % 4)
                {
                    case 2:
                        payload += "==";
                        break;
                    case 3:
                        payload += "=";
                        break;
                }

                var payloadBytes = Convert.FromBase64String(payload);
                using var document = JsonDocument.Parse(payloadBytes);

                if (document.RootElement.TryGetProperty("exp", out var expClaim)
                    && expClaim.TryGetInt64(out var expUnixSeconds))
                {
                    var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnixSeconds).UtcDateTime;

                    if (expiresAt <= now)
                        return now.Add(fallback);

                    if (expiresAt > maxAllowedExpiresAt)
                        return maxAllowedExpiresAt;

                    return expiresAt;
                }
            }
            catch
            {
                // Fall back to minimum blacklist window when token parsing fails.
            }

            return now.Add(fallback);
        }
    }
}
