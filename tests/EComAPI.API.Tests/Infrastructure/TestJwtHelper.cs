using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EComAPI.API.Tests.Infrastructure
{
    /// <summary>
    /// Helper untuk membuat JWT token valid dalam integration tests.
    /// Menggunakan secret yang sama seperti di appsettings.json.
    /// </summary>
    public static class TestJwtHelper
    {
        // Secret yang sama dengan appsettings.json
        private const string Secret = "uSdNsfTAcgVEbQ49q2jyJO6hnXI8ZCkFPvURpDimHr5GBK7Le31ta0zoWYMwlx";
        private const string Issuer = "EComAPI";
        private const string Audience = "EComAPIClients";

        /// <summary>
        /// Buat JWT token dengan claims tertentu untuk user role tertentu.
        /// </summary>
        public static string GenerateToken(
            Guid userId,
            string email,
            string role,
            IEnumerable<string>? permissions = null,
            int expiryMinutes = 60)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("jti", Guid.NewGuid().ToString()),
            };

            // Tambah permission claims
            if (permissions != null)
            {
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("permission", permission));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Buat token Admin dengan semua permissions.
        /// </summary>
        public static string GenerateAdminToken(Guid? userId = null)
        {
            return GenerateToken(
                userId: userId ?? Guid.NewGuid(),
                email: "admin@test.com",
                role: "Admin",
                permissions: new[]
                {
                    "categories.read", "categories.create", "categories.update", "categories.delete", "categories.restore",
                    "products.read", "products.create", "products.update", "products.delete", "products.restore",
                    "users.read.all", "users.create", "users.update.any", "users.delete.any", "users.restore.any"
                }
            );
        }

        /// <summary>
        /// Buat token Customer dengan permissions terbatas.
        /// </summary>
        public static string GenerateCustomerToken(Guid? userId = null)
        {
            return GenerateToken(
                userId: userId ?? Guid.NewGuid(),
                email: "customer@test.com",
                role: "Customer",
                permissions: new[]
                {
                    "users.read.own", "users.update.own", "users.delete.own"
                }
            );
        }
    }
}
