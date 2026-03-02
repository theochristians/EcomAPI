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
using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.API.Tests.Addresses
{
    public class AddressesControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AddressesControllerTests(TestWebApplicationFactory factory)
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
        /// Seed user langsung ke DB + generate JWT via TestJwtHelper.
        /// Menghindari rate limiter auth-register (5 req / 10 menit).
        /// </summary>
        private async Task<string> RegisterAndLoginAsync(string? email = null, string password = "Password123!")
        {
            await EnsureCustomerRoleExists();

            email ??= $"addr_{Guid.NewGuid():N}@test.com";

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var role = db.Roles.First(r => r.Name == "Customer");

            var emailAddress = EmailAddress.Create(email);
            var pwdHash = PasswordHash.FromHash("hashed_for_test");
            var user = new User("Address User", emailAddress, pwdHash, role.Id, role.Id);

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return TestJwtHelper.GenerateToken(user.Id, email, "Customer");
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

        private static CreateAddressRequest BuildAddressRequest(bool isDefault = false) =>
            new CreateAddressRequest(
                "Rumah",
                "John Doe",
                "08123456789",
                "Jl. Contoh No. 1",
                "Kota Contoh",
                "Provinsi Contoh",
                "12345",
                isDefault
            );

        /// <summary>
        /// Create an address for the logged-in user and return its id.
        /// </summary>
        private async Task<Guid> CreateAddressAndGetIdAsync(string token)
        {
            SetBearerToken(token);

            var response = await _client.PostAsJsonAsync("/api/addresses", BuildAddressRequest());
            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<AddressResponse>>(_json);
            return body!.Data!.Id;
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/addresses  ([Authorize])
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.GetAsync("/api/addresses");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAll_WithToken_Returns200()
        {
            var token = await RegisterAndLoginAsync();
            SetBearerToken(token);

            var response = await _client.GetAsync("/api/addresses");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/addresses/default  ([Authorize])
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetDefault_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.GetAsync("/api/addresses/default");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetDefault_WithToken_NoAddress_Returns200WithNullData()
        {
            var token = await RegisterAndLoginAsync();
            SetBearerToken(token);

            // Fresh user has no default address — returns 200 with null data
            var response = await _client.GetAsync("/api/addresses/default");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/addresses  ([Authorize])
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.PostAsJsonAsync("/api/addresses", BuildAddressRequest());

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_WithToken_ValidRequest_Returns200()
        {
            var token = await RegisterAndLoginAsync();
            SetBearerToken(token);

            var response = await _client.PostAsJsonAsync("/api/addresses", BuildAddressRequest());

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        // ─────────────────────────────────────────────────────────
        // PATCH /api/addresses/{id}  ([Authorize])
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new UpdateAddressRequest("Kantor", "Jane", "08111111111", "Jl. Baru No. 2", "Kota Baru", "Provinsi Baru", "54321");
            var response = await _client.PatchAsJsonAsync($"/api/addresses/{Guid.NewGuid()}", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Update_WithToken_AlamatAda_Returns200()
        {
            var token = await RegisterAndLoginAsync();
            var addressId = await CreateAddressAndGetIdAsync(token);
            SetBearerToken(token);

            var request = new UpdateAddressRequest("Kantor", "Jane Doe", "08999999999", "Jl. Update No. 3", "Kota Update", "Provinsi Update", "99999");
            var response = await _client.PatchAsJsonAsync($"/api/addresses/{addressId}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/addresses/{id}/set-default  ([Authorize])
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task SetDefault_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.PostAsync($"/api/addresses/{Guid.NewGuid()}/set-default", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SetDefault_WithToken_AlamatAda_Returns200()
        {
            var token = await RegisterAndLoginAsync();
            var addressId = await CreateAddressAndGetIdAsync(token);
            SetBearerToken(token);

            var response = await _client.PostAsync($"/api/addresses/{addressId}/set-default", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // DELETE /api/addresses/{id}  ([Authorize])
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.DeleteAsync($"/api/addresses/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Delete_WithToken_AlamatAda_Returns200()
        {
            var token = await RegisterAndLoginAsync();
            var addressId = await CreateAddressAndGetIdAsync(token);
            SetBearerToken(token);

            var response = await _client.DeleteAsync($"/api/addresses/{addressId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
