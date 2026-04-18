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
    public class ReturnControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ReturnControllerTests(TestWebApplicationFactory factory)
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

        private void SetAdminToken()
        {
            var token = TestJwtHelper.GenerateAdminToken();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private void ClearAuth()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        private static CreateReturnRequest BuildCreateReturnRequest() => new(
            OrderId: Guid.NewGuid(),
            Reason: "Product defective",
            Items: new List<CreateReturnItemRequest>
            {
                new(Guid.NewGuid(), 1)
            });

        // ─────────────────────────────────────────────────────────
        // POST /api/returns
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateReturn_TanpaToken_Returns401()
        {
            ClearAuth();
            var response = await _client.PostAsJsonAsync("/api/returns", BuildCreateReturnRequest());
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateReturn_OrderTidakAda_Returns400()
        {
            SetCustomerToken(Guid.NewGuid());
            var response = await _client.PostAsJsonAsync("/api/returns", BuildCreateReturnRequest());
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/returns
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetMyReturns_TanpaToken_Returns401()
        {
            ClearAuth();
            var response = await _client.GetAsync("/api/returns");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMyReturns_DenganToken_Returns200()
        {
            SetCustomerToken(Guid.NewGuid());
            var response = await _client.GetAsync("/api/returns");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/returns/{id}
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetReturnById_TanpaToken_Returns401()
        {
            ClearAuth();
            var response = await _client.GetAsync($"/api/returns/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetReturnById_ReturnTidakAda_Returns400()
        {
            SetCustomerToken(Guid.NewGuid());
            var response = await _client.GetAsync($"/api/returns/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/returns/{id}/approve
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task ApproveReturn_TanpaToken_Returns401()
        {
            ClearAuth();
            var request = new ApproveReturnRequest(50000, null);
            var response = await _client.PostAsJsonAsync($"/api/returns/{Guid.NewGuid()}/approve", request);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ApproveReturn_ReturnTidakAda_Returns400()
        {
            SetAdminToken();
            var request = new ApproveReturnRequest(50000, null);
            var response = await _client.PostAsJsonAsync($"/api/returns/{Guid.NewGuid()}/approve", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/returns/{id}/reject
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task RejectReturn_TanpaToken_Returns401()
        {
            ClearAuth();
            var response = await _client.PostAsJsonAsync($"/api/returns/{Guid.NewGuid()}/reject", new { });
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RejectReturn_ReturnTidakAda_Returns400()
        {
            SetAdminToken();
            var response = await _client.PostAsJsonAsync($"/api/returns/{Guid.NewGuid()}/reject", new { });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/returns/{id}/refund
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task MarkReturnRefunded_TanpaToken_Returns401()
        {
            ClearAuth();
            var response = await _client.PostAsJsonAsync($"/api/returns/{Guid.NewGuid()}/refund", new { });
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task MarkReturnRefunded_ReturnTidakAda_Returns400()
        {
            SetAdminToken();
            var response = await _client.PostAsJsonAsync($"/api/returns/{Guid.NewGuid()}/refund", new { });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
