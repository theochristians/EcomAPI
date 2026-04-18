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
using EComAPI.Infrastructure.Auth.Security;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.API.Tests.Auth
{
    public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        // ─────────────────────────────────────────────────────────
        // Helper: seed a Customer role into the in-memory DB
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

        private async Task EnsureAdminRoleExists()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!db.Roles.Any(r => r.Name == "Admin"))
            {
                db.Roles.Add(new Role("Admin", Guid.NewGuid()));
                await db.SaveChangesAsync();
            }
        }

        private async Task<string> GetLatestOtpCodeAsync(string email)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await db.Users.FirstAsync(x => x.Email.Value == email.ToLowerInvariant());
            var emailVerification = await db.EmailVerifications
                .Where(x => x.UserId == user.Id && x.VerifiedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .FirstAsync();

            return emailVerification.Code;
        }

        private async Task VerifyEmailByApiAsync(string email)
        {
            var sendResponse = await _client.PostAsJsonAsync(
                "/api/auth/email-verification/send",
                new SendEmailVerificationByEmailRequest(email));

            sendResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var code = await GetLatestOtpCodeAsync(email);
            var verifyResponse = await _client.PostAsJsonAsync(
                "/api/auth/email-verification/verify",
                new VerifyEmailByEmailRequest(email, code));

            verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/auth/register
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Register_ValidRequest_Returns200WithUserData()
        {
            await EnsureCustomerRoleExists();

            var request = new RegisterRequest("Test User", $"user_{Guid.NewGuid()}@test.com", "Password123!");

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<RegisterResponse>>(_json);
            body.Should().NotBeNull();
            body!.Success.Should().BeTrue();
            body.Data!.Email.Should().Be(request.Email.ToLowerInvariant());
            body.Data.FullName.Should().Be(request.FullName);
        }

        [Fact]
        public async Task Register_EmailKosong_Returns400()
        {
            var request = new RegisterRequest("Test User", "", "Password123!");

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Register_PasswordTerlalupendek_Returns400()
        {
            var request = new RegisterRequest("Test User", "shortpw@test.com", "123");

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Register_EmailSudahTerdaftar_Returns400()
        {
            await EnsureCustomerRoleExists();

            var email = $"dup_{Guid.NewGuid()}@test.com";
            var request = new RegisterRequest("Test User", email, "Password123!");

            // Register pertama - berhasil
            await _client.PostAsJsonAsync("/api/auth/register", request);

            // Register kedua dengan email yang sama - harus gagal
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeFalse();
            body.Message.Should().Contain("already registered");
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/auth/login
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Login_CredentialValid_Returns200WithTokens()
        {
            await EnsureCustomerRoleExists();

            // Register user dulu
            var email = $"login_{Guid.NewGuid()}@test.com";
            var password = "Password123!";
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Login User", email, password));
            await VerifyEmailByApiAsync(email);

            // Login
            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password, null));

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<LoginResponse>>(_json);
            body!.Success.Should().BeTrue();
            body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
            body.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
            body.Data.AccessTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task Login_EmailBelumTerverifikasi_Returns400()
        {
            await EnsureCustomerRoleExists();

            var email = $"unverified_{Guid.NewGuid()}@test.com";
            var password = "Password123!";
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Unverified User", email, password));

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password, null));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeFalse();
            body.Message.Should().Contain("Email is not verified");
        }

        [Fact]
        public async Task Login_PasswordSalah_Returns400()
        {
            await EnsureCustomerRoleExists();

            var email = $"wrongpw_{Guid.NewGuid()}@test.com";
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Wrong PW", email, "Password123!"));
            await VerifyEmailByApiAsync(email);

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassword!", null));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Login_EmailTidakTerdaftar_Returns400()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login",
                new LoginRequest("ghost@test.com", "Password123!", null));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/auth/refresh-token  (AllowAnonymous)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task RefreshToken_WithValidRefreshToken_Returns200()
        {
            await EnsureCustomerRoleExists();

            // Register + login to obtain a real refresh token
            var email = $"refresh_{Guid.NewGuid()}@test.com";
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Refresh User", email, "Password123!"));
            await VerifyEmailByApiAsync(email);
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password123!", null));
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<TestApiResponse<LoginResponse>>(_json);

            var request = new RefreshTokenRequest(loginBody!.Data!.RefreshToken);

            var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<LoginResponse>>(_json);
            body!.Success.Should().BeTrue();
            body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
            body.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task RefreshToken_WithInvalidToken_Returns400()
        {
            var request = new RefreshTokenRequest("invalid-refresh-token-xyz");

            var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeFalse();
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/auth/logout  ([Authorize])
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Logout_TanpaToken_Returns401()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var response = await _client.PostAsJsonAsync("/api/auth/logout", new LogoutRequest(null));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Logout_WithValidToken_Returns200()
        {
            await EnsureCustomerRoleExists();

            var email = $"logout_{Guid.NewGuid()}@test.com";
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Logout User", email, "Password123!"));
            await VerifyEmailByApiAsync(email);
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password123!", null));
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<TestApiResponse<LoginResponse>>(_json);

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody!.Data!.AccessToken);

            var request = new LogoutRequest(loginBody.Data.RefreshToken);

            var response = await _client.PostAsJsonAsync("/api/auth/logout", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
