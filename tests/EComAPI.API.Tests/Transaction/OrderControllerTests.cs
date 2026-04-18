using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using EComAPI.API.Tests.Infrastructure;
using EComAPI.API.Transaction.Dtos.Requests;
using EComAPI.Domain.Transaction.Enums;

namespace EComAPI.API.Tests.Transaction
{
    public class OrderControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OrderControllerTests(TestWebApplicationFactory factory)
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

        private static CreateOrderRequest BuildCreateOrderRequest() => new(
            AddressId: Guid.NewGuid(),
            ShippingType: ShippingType.Regular,
            Items: new List<CreateOrderItemRequest>
            {
                new(Guid.NewGuid(), 2)
            },
            CouponCode: null,
            CustomerNote: null);

        // ─────────────────────────────────────────────────────────
        // POST /api/orders
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateOrder_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.PostAsJsonAsync("/api/orders", BuildCreateOrderRequest());

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/orders
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetMyOrders_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.GetAsync("/api/orders");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMyOrders_DenganToken_Returns200()
        {
            SetCustomerToken(Guid.NewGuid());

            var response = await _client.GetAsync("/api/orders");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/orders/{id}
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetOrderById_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetOrderById_OrderTidakAda_Returns400()
        {
            SetCustomerToken(Guid.NewGuid());

            var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/orders/{id}/payment/proof
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task SubmitPaymentProof_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new SubmitPaymentProofRequest("https://cdn.example.com/proof.jpg", null);

            var response = await _client.PostAsJsonAsync($"/api/orders/{Guid.NewGuid()}/payment/proof", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ─────────────────────────────────────────────────────────
        // PATCH /api/orders/{id}/status
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateOrderStatus_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new UpdateOrderStatusRequest("processing", null, null, null);

            var response = await _client.PatchAsJsonAsync($"/api/orders/{Guid.NewGuid()}/status", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateOrderStatus_OrderTidakAda_Returns400()
        {
            SetAdminToken();

            var request = new UpdateOrderStatusRequest("processing", null, null, null);

            var response = await _client.PatchAsJsonAsync($"/api/orders/{Guid.NewGuid()}/status", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/orders/{id}/payment/confirm
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task ConfirmPayment_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new ConfirmPaymentRequest(null);

            var response = await _client.PostAsJsonAsync($"/api/orders/{Guid.NewGuid()}/payment/confirm", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ConfirmPayment_OrderTidakAda_Returns400()
        {
            SetAdminToken();

            var request = new ConfirmPaymentRequest("Confirmed");

            var response = await _client.PostAsJsonAsync($"/api/orders/{Guid.NewGuid()}/payment/confirm", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
