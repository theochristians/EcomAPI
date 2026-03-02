using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using EComAPI.API.Auth.Dtos.Request;
using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Tests.Infrastructure;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.API.Tests.Auth
{
    public class UsersControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public UsersControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        // ─────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────

        private async Task EnsureCustomerRoleExists()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!db.Roles.Any(r => r.Name == "Customer"))
            {
                db.Roles.Add(new Role("Customer", Guid.NewGuid()));
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Register + login a fresh user, return their access token.
        /// </summary>
        private async Task<string> RegisterAndLoginAsync(string? email = null, string password = "Password123!")
        {
            await EnsureCustomerRoleExists();

            email ??= $"usr_{Guid.NewGuid()}@test.com";
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Test User", email, password));

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
                new LoginRequest(email, password, null));

            var loginBody = await loginResponse.Content.ReadFromJsonAsync<TestApiResponse<LoginResponse>>(_json);
            return loginBody!.Data!.AccessToken;
        }

        private void SetBearerToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private void ClearAuth()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/users/me  (HasPermission users.read.own)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetOwnProfile_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.GetAsync("/api/users/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetOwnProfile_WithCustomerToken_Returns200()
        {
            var token = await RegisterAndLoginAsync();
            SetBearerToken(token);

            var response = await _client.GetAsync("/api/users/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<UserProfileResponse>>(_json);
            body!.Success.Should().BeTrue();
            body.Data!.Email.Should().NotBeNullOrWhiteSpace();
        }

        // ─────────────────────────────────────────────────────────
        // PATCH /api/users/me  (HasPermission users.update.own)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateOwnProfile_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new UpdateOwnProfileRequest("New Name", null, null, null, null, null);

            var response = await _client.PatchAsJsonAsync("/api/users/me", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateOwnProfile_WithCustomerToken_Returns200()
        {
            var token = await RegisterAndLoginAsync();
            SetBearerToken(token);

            var request = new UpdateOwnProfileRequest("Updated Name", null, null, null, null, null);

            var response = await _client.PatchAsJsonAsync("/api/users/me", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        // ─────────────────────────────────────────────────────────
        // DELETE /api/users/me  (HasPermission users.delete.own)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteMyAccount_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.DeleteAsync("/api/users/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteMyAccount_WithCustomerToken_Returns200()
        {
            // Use a fresh unique user so delete doesn't affect other tests
            var email = $"del_usr_{Guid.NewGuid()}@test.com";
            var token = await RegisterAndLoginAsync(email);
            SetBearerToken(token);

            var response = await _client.DeleteAsync("/api/users/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        // ─────────────────────────────────────────────────────────
        // Helpers — email verification
        // Seed user langsung ke DB + JWT via TestJwtHelper
        // agar bypass rate limiter (5 register / 10 menit)
        // ─────────────────────────────────────────────────────────

        private async Task<(string Token, Guid UserId)> SeedUserWithTokenAsync(
            string? email = null)
        {
            await EnsureCustomerRoleExists();
            email ??= $"ev_{Guid.NewGuid():N}@test.com";

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var role = db.Roles.First(r => r.Name == "Customer");

            var emailAddress = EmailAddress.Create(email);
            var password = PasswordHash.FromHash("hashed_for_test");
            var user = new User("Test User", emailAddress, password, role.Id, role.Id);

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var token = TestJwtHelper.GenerateToken(
                user.Id, email, "Customer",
                new[] { "users.update.own", "users.read.own" });

            return (token, user.Id);
        }

        private async Task SeedEmailVerificationAsync(Guid userId, string code)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.EmailVerifications.Add(
                new EmailVerification(userId, code, DateTime.UtcNow.AddMinutes(10), userId));
            await db.SaveChangesAsync();
        }

        private async Task<string> GetLatestOtpCodeAsync(Guid userId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var ev = await db.EmailVerifications
                .Where(x => x.UserId == userId && x.VerifiedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
            return ev?.Code ?? throw new InvalidOperationException("No pending OTP found");
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/users/me/email-verification/send
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task SendEmailVerification_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.PostAsync("/api/users/me/email-verification/send", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SendEmailVerification_UserBaru_Returns200DenganExpiresAt()
        {
            // Seed user langsung — tidak lewat HTTP register agar tidak kena rate limiter
            var (token, _) = await SeedUserWithTokenAsync();
            SetBearerToken(token);

            var response = await _client.PostAsync("/api/users/me/email-verification/send", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
            body.Message.Should().Contain("Verification code sent");
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/users/me/email-verification/verify
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task VerifyEmail_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.PostAsJsonAsync(
                "/api/users/me/email-verification/verify",
                new { code = "ABC123" });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task VerifyEmail_KodeKosong_Returns400()
        {
            var (token, _) = await SeedUserWithTokenAsync();
            SetBearerToken(token);

            var response = await _client.PostAsJsonAsync(
                "/api/users/me/email-verification/verify",
                new { code = "" });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeFalse();
        }

        [Fact]
        public async Task VerifyEmail_KodeSalah_Returns400()
        {
            var (token, userId) = await SeedUserWithTokenAsync();
            SetBearerToken(token);
            await SeedEmailVerificationAsync(userId, "ABC123");

            var response = await _client.PostAsJsonAsync(
                "/api/users/me/email-verification/verify",
                new { code = "WRONG1" }); // kode salah

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeFalse();
        }

        [Fact]
        public async Task VerifyEmail_KodeBenar_Returns200DanEmailTerverifikasi()
        {
            var (token, userId) = await SeedUserWithTokenAsync();
            SetBearerToken(token);

            // Panggil send endpoint agar OTP dibuat oleh server
            var sendResp = await _client.PostAsync("/api/users/me/email-verification/send", null);
            sendResp.StatusCode.Should().Be(HttpStatusCode.OK);

            // Baca OTP dari DB langsung
            var code = await GetLatestOtpCodeAsync(userId);

            // Verifikasi dengan kode yang benar
            var verifyResp = await _client.PostAsJsonAsync(
                "/api/users/me/email-verification/verify",
                new { code });

            verifyResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await verifyResp.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }
    }
}
