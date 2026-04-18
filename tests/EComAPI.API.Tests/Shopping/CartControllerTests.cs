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
    public class CartControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CartControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        // ─────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────

        private static readonly Guid _testUserId = Guid.NewGuid();

        private void SetCustomerToken(Guid? userId = null)
        {
            var token = TestJwtHelper.GenerateCustomerToken(userId ?? _testUserId);
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private void ClearAuth()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        private async Task<Cart> SeedCartAsync(Guid userId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cart = new Cart(userId, userId);
            db.Carts.Add(cart);
            await db.SaveChangesAsync();
            return cart;
        }

        private async Task<(Cart cart, CartItem item)> SeedCartWithItemAsync(Guid userId, Guid variantId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cart = new Cart(userId, userId);
            db.Carts.Add(cart);
            await db.SaveChangesAsync();

            var cartItem = new CartItem(cart.Id, variantId, 2, userId);
            db.CartItems.Add(cartItem);
            await db.SaveChangesAsync();

            return (cart, cartItem);
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/cart
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetCart_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.GetAsync("/api/cart");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetCart_CartTidakAda_Returns400()
        {
            var userId = Guid.NewGuid();
            SetCustomerToken(userId);

            // tidak ada cart yang di-seed untuk userId ini
            var response = await _client.GetAsync("/api/cart");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetCart_CartAda_Returns200()
        {
            var userId = Guid.NewGuid();
            SetCustomerToken(userId);
            await SeedCartAsync(userId);

            var response = await _client.GetAsync("/api/cart");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/cart/items
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task AddItem_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new AddToCartRequest(Guid.NewGuid(), 1);

            var response = await _client.PostAsJsonAsync("/api/cart/items", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AddItem_VariantTidakAda_Returns400()
        {
            var userId = Guid.NewGuid();
            SetCustomerToken(userId);

            var request = new AddToCartRequest(Guid.NewGuid(), 1);

            var response = await _client.PostAsJsonAsync("/api/cart/items", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // PATCH /api/cart/items/{id}
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateItem_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new UpdateCartItemRequest(3);

            var response = await _client.PatchAsJsonAsync($"/api/cart/items/{Guid.NewGuid()}", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateItem_CartItemTidakAda_Returns404()
        {
            var userId = Guid.NewGuid();
            SetCustomerToken(userId);
            await SeedCartAsync(userId);

            var request = new UpdateCartItemRequest(3);

            var response = await _client.PatchAsJsonAsync($"/api/cart/items/{Guid.NewGuid()}", request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateItem_CartItemAda_Returns200()
        {
            var userId = Guid.NewGuid();
            SetCustomerToken(userId);
            var (_, cartItem) = await SeedCartWithItemAsync(userId, Guid.NewGuid());

            var request = new UpdateCartItemRequest(5);

            var response = await _client.PatchAsJsonAsync($"/api/cart/items/{cartItem.Id}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // DELETE /api/cart/items/{id}
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveItem_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.DeleteAsync($"/api/cart/items/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RemoveItem_CartItemTidakAda_Returns404()
        {
            var userId = Guid.NewGuid();
            SetCustomerToken(userId);
            await SeedCartAsync(userId);

            var response = await _client.DeleteAsync($"/api/cart/items/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RemoveItem_CartItemAda_Returns200()
        {
            var userId = Guid.NewGuid();
            SetCustomerToken(userId);
            var (_, cartItem) = await SeedCartWithItemAsync(userId, Guid.NewGuid());

            var response = await _client.DeleteAsync($"/api/cart/items/{cartItem.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // DELETE /api/cart
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task ClearCart_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.DeleteAsync("/api/cart");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ClearCart_CartAda_Returns200()
        {
            var userId = Guid.NewGuid();
            SetCustomerToken(userId);
            await SeedCartAsync(userId);

            var response = await _client.DeleteAsync("/api/cart");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
