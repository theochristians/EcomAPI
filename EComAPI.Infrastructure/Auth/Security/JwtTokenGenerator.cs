using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Domain.Auth.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EComAPI.Infrastructure.Auth.Security
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleRepository _roleRepository;

        public JwtTokenGenerator(
            IOptions<JwtSettings> jwtOptions,
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository)
        {
            _jwtSettings = jwtOptions.Value;
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
        }

        public async Task<string> GenerateTokenAsync(User user)
        {
            // =========================
            // BASE CLAIMS
            // =========================
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // =========================
            // ROLE CLAIM
            // =========================
            var role = await _roleRepository.GetRoleByIdAsync(user.RoleId);

            if (role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            // =========================
            // PERMISSION CLAIMS
            // =========================
            var permissions = await _permissionRepository.GetPermissionByUserIdAsync(user.Id);

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission.Name));
            }

            // =========================
            // SIGNING CREDENTIALS
            // =========================
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            // =========================
            // CREATE TOKEN
            // =========================
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
