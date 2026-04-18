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
    public class ReviewControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ReviewControllerTests(TestWebApplicationFactory factory)
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

        private static CreateReviewRequest BuildCreateReviewRequest() => new(
            OrderId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            Rating: 5,
            Comment: "Great product!");

        // ─────────────────────────────────────────────────────────
        // GET /api/reviews/product/{productId} — Public
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetReviewsByProduct_TanpaToken_Returns200()
        {
            ClearAuth();
            var response = await _client.GetAsync($"/api/reviews/product/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetReviewsByProduct_DenganToken_Returns200()
        {
            SetCustomerToken(Guid.NewGuid());
            var response = await _client.GetAsync($"/api/reviews/product/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/reviews
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateReview_TanpaToken_Returns401()
        {
            ClearAuth();
            var response = await _client.PostAsJsonAsync("/api/reviews", BuildCreateReviewRequest());
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateReview_DenganToken_ProductTidakAda_Returns400()
        {
            SetCustomerToken(Guid.NewGuid());
            var response = await _client.PostAsJsonAsync("/api/reviews", BuildCreateReviewRequest());
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // PUT /api/reviews/{id}
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateReview_TanpaToken_Returns401()
        {
            ClearAuth();
            var request = new UpdateReviewRequest(4, "Updated review");
            var response = await _client.PutAsJsonAsync($"/api/reviews/{Guid.NewGuid()}", request);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateReview_ReviewTidakAda_Returns400()
        {
            SetCustomerToken(Guid.NewGuid());
            var request = new UpdateReviewRequest(4, "Updated review");
            var response = await _client.PutAsJsonAsync($"/api/reviews/{Guid.NewGuid()}", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // DELETE /api/reviews/{id}
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteReview_TanpaToken_Returns401()
        {
            ClearAuth();
            var response = await _client.DeleteAsync($"/api/reviews/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteReview_ReviewTidakAda_Returns400()
        {
            SetCustomerToken(Guid.NewGuid());
            var response = await _client.DeleteAsync($"/api/reviews/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
