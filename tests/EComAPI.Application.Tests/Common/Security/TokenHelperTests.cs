using EComAPI.Application.Common.Security;
using FluentAssertions;
using Xunit;

namespace EComAPI.Application.Tests.Common.Security
{
    public class TokenHelperTests
    {
        [Fact]
        public void ResolveAccessTokenExpiry_ExpTerlaluJauh_ShouldClampMaksimal24Jam()
        {
            var now = DateTime.UtcNow;
            var farFutureExp = DateTimeOffset.UtcNow.AddDays(365).ToUnixTimeSeconds();
            var token = BuildUnsignedJwt(farFutureExp);

            var expiresAt = TokenHelper.ResolveAccessTokenExpiry(token);

            expiresAt.Should().BeAfter(now.AddHours(23));
            expiresAt.Should().BeOnOrBefore(now.AddHours(24).AddSeconds(1));
        }

        [Fact]
        public void ResolveAccessTokenExpiry_ExpValid_ShouldReturnExpiryAsli()
        {
            var expected = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
            var token = BuildUnsignedJwt(expected);

            var expiresAt = TokenHelper.ResolveAccessTokenExpiry(token);

            expiresAt.Should().BeCloseTo(DateTimeOffset.FromUnixTimeSeconds(expected).UtcDateTime, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void ResolveAccessTokenExpiry_TokenInvalid_ShouldFallback15Menit()
        {
            var now = DateTime.UtcNow;

            var expiresAt = TokenHelper.ResolveAccessTokenExpiry("invalid-token");

            expiresAt.Should().BeAfter(now.AddMinutes(14));
            expiresAt.Should().BeOnOrBefore(now.AddMinutes(15).AddSeconds(1));
        }

        private static string BuildUnsignedJwt(long expUnixSeconds)
        {
            var headerJson = "{\"alg\":\"none\",\"typ\":\"JWT\"}";
            var payloadJson = $"{{\"exp\":{expUnixSeconds}}}";

            return $"{Base64UrlEncode(headerJson)}.{Base64UrlEncode(payloadJson)}.";
        }

        private static string Base64UrlEncode(string value)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
