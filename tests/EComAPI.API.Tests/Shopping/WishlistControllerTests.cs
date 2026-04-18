using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using EComAPI.API.Shopping.Dtos.Requests;
using EComAPI.API.Tests.Infrastructure;
using EComAPI.Domain.Shopping.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.API.Tests.Shopping
{
    public class WishlistControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public WishlistControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
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

        private async Task<Wishlist> SeedWishlistItemAsync(Guid userId, Guid productId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var wishlistItem = new Wishlist(userId, productId, userId);
            db.Wishlists.Add(wishlistItem);
            await db.SaveChangesAsync();
            return wishlistItem;
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/wishlist
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWishlist_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.GetAsync("/api/wishlist");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetWishlist_DenganToken_WishlistKosong_Returns200()
        {
            SetCustomerToken(Guid.NewGuid());

            var response = await _client.GetAsync("/api/wishlist");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task GetWishlist_AdaItem_Returns200()
        {
            var userId = Guid.NewGuid();
            SetCustomerToken(userId);
            await SeedWishlistItemAsync(userId, Guid.NewGuid());

            var response = await _client.GetAsync("/api/wishlist");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/wishlist
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task AddToWishlist_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new AddToWishlistRequest(Guid.NewGuid());

            var response = await _client.PostAsJsonAsync("/api/wishlist", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AddToWishlist_ProductTidakAda_Returns400()
        {
            SetCustomerToken(Guid.NewGuid());

            var request = new AddToWishlistRequest(Guid.NewGuid());

            var response = await _client.PostAsJsonAsync("/api/wishlist", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // DELETE /api/wishlist/{productId}
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveFromWishlist_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.DeleteAsync($"/api/wishlist/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RemoveFromWishlist_ItemTidakAda_Returns404()
        {
            SetCustomerToken(Guid.NewGuid());

            var response = await _client.DeleteAsync($"/api/wishlist/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RemoveFromWishlist_ItemAda_Returns200()
        {
            var userId = Guid.NewGuid();
            SetCustomerToken(userId);
            var wishlistItem = await SeedWishlistItemAsync(userId, Guid.NewGuid());

            var response = await _client.DeleteAsync($"/api/wishlist/{wishlistItem.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
