using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using EComAPI.API.Tests.Infrastructure;

namespace EComAPI.API.Tests.Transaction
{
    public class OrderStatusLogControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OrderStatusLogControllerTests(TestWebApplicationFactory factory)
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

        private void ClearAuth()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/orders/{orderId}/status-logs
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetOrderStatusLogs_TanpaToken_Returns401()
        {
            ClearAuth();
            var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}/status-logs");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetOrderStatusLogs_OrderTidakAda_Returns400()
        {
            SetCustomerToken(Guid.NewGuid());
            var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}/status-logs");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
