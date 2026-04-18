using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using EComAPI.API.Tests.Infrastructure;
using EComAPI.API.Transaction.Dtos.Requests;

namespace EComAPI.API.Tests.Transaction
{
    public class CouponControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CouponControllerTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // ─────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────

        private void SetAdminToken()
        {
            var token = TestJwtHelper.GenerateAdminToken();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private void SetCustomerToken()
        {
            var token = TestJwtHelper.GenerateCustomerToken();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private void ClearAuth()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        private static CreateCouponRequest BuildCouponRequest(string code) => new(
            Code: code,
            DiscountAmount: 10000,
            DiscountType: "fixed",
            MaxUsage: 100,
            ValidFrom: DateTime.UtcNow,
            ValidUntil: DateTime.UtcNow.AddDays(30),
            MinimumPurchase: 100000,
            MaxDiscount: null);

        // ─────────────────────────────────────────────────────────
        // POST /api/coupons
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateCoupon_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = BuildCouponRequest("SAVE10");

            var response = await _client.PostAsJsonAsync("/api/coupons", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateCoupon_DenganAdminToken_Returns200()
        {
            SetAdminToken();

            var code = $"TEST{Guid.NewGuid():N}"[..10].ToUpper();
            var request = BuildCouponRequest(code);

            var response = await _client.PostAsJsonAsync("/api/coupons", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task CreateCoupon_KodeSudahAda_Returns400()
        {
            SetAdminToken();

            var code = $"DUP{Guid.NewGuid():N}"[..10].ToUpper();
            var request = BuildCouponRequest(code);

            // Buat pertama kali → sukses
            await _client.PostAsJsonAsync("/api/coupons", request);

            // Buat dengan kode yang sama → error
            var response = await _client.PostAsJsonAsync("/api/coupons", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCoupon_DiscountTypePascalCase_Returns200()
        {
            SetAdminToken();

            // "Fixed" PascalCase seharusnya diterima (dinormalisasi ke "fixed")
            var code = $"PASCAL{Guid.NewGuid():N}"[..10].ToUpper();
            var request = new CreateCouponRequest(
                Code: code,
                DiscountAmount: 5000,
                DiscountType: "Fixed",
                MaxUsage: 10,
                ValidFrom: DateTime.UtcNow,
                ValidUntil: DateTime.UtcNow.AddDays(30),
                MinimumPurchase: 0,
                MaxDiscount: null);

            var response = await _client.PostAsJsonAsync("/api/coupons", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/coupons
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllCoupons_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.GetAsync("/api/coupons");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllCoupons_DenganAdminToken_Returns200()
        {
            SetAdminToken();

            var response = await _client.GetAsync("/api/coupons");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllCoupons_DenganCustomerToken_Returns200()
        {
            SetCustomerToken();

            var response = await _client.GetAsync("/api/coupons");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllCoupons_SetelahCreateCoupon_MengandungDataBaru()
        {
            SetAdminToken();

            var code = $"LIST{Guid.NewGuid():N}"[..10].ToUpper();
            var createRequest = BuildCouponRequest(code);
            await _client.PostAsJsonAsync("/api/coupons", createRequest);

            var response = await _client.GetAsync("/api/coupons");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<List<object>>>(_json);
            body!.Success.Should().BeTrue();
            body.Data.Should().NotBeNull();
            body.Data!.Count.Should().BeGreaterThan(0);
        }
    }
}
