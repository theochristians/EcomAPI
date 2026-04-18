using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using EComAPI.API.Tests.Infrastructure;

namespace EComAPI.API.Tests.Transaction
{
    public class StockLogControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public StockLogControllerTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // ─────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────

        private void SetCustomerToken(Guid? userId = null)
        {
            var token = TestJwtHelper.GenerateCustomerToken(userId);
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private void SetAdminToken(Guid? userId = null)
        {
            var token = TestJwtHelper.GenerateAdminToken(userId);
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private void ClearAuth()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/stock-logs/variant/{variantId}
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetStockLogs_TanpaToken_Returns401()
        {
            ClearAuth();
            var response = await _client.GetAsync($"/api/stock-logs/variant/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetStockLogs_DenganCustomerToken_Returns403()
        {
            SetCustomerToken(Guid.NewGuid());
            var response = await _client.GetAsync($"/api/stock-logs/variant/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetStockLogs_DenganAdminToken_Returns200()
        {
            SetAdminToken(Guid.NewGuid());
            var response = await _client.GetAsync($"/api/stock-logs/variant/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }
    }
}
