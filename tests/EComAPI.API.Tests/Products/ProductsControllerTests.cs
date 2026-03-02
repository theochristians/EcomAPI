using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using EComAPI.API.Products.Dtos.Requests;
using EComAPI.API.Tests.Infrastructure;
using EComAPI.Domain.Categories.Entities;
using EComAPI.Domain.Products.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.API.Tests.Products
{
    public class ProductsControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly Guid _seedUserId = Guid.NewGuid();

        public ProductsControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
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

        private void ClearAuth()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        private async Task<Guid> SeedCategoryAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var slug = $"cat-{Guid.NewGuid():N}"[..20];
            var category = new Category($"Cat {Guid.NewGuid():N}"[..20], slug, _seedUserId);
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return category.Id;
        }

        private async Task<(Guid ProductId, string Slug)> SeedProductAsync(Guid categoryId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var slug = $"prd-{Guid.NewGuid():N}"[..25];
            var product = new Product(categoryId, $"Product {Guid.NewGuid():N}"[..20], slug, 99.99m, _seedUserId);
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return (product.Id, slug);
        }

        private async Task<Guid> SeedInactiveProductAsync(Guid categoryId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var slug = $"prd-{Guid.NewGuid():N}"[..25];
            var product = new Product(categoryId, $"Product {Guid.NewGuid():N}"[..20], slug, 99.99m, _seedUserId);
            product.Deactivate(_seedUserId);
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return product.Id;
        }

        private async Task<Guid> SeedDeletedProductAsync(Guid categoryId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var slug = $"prd-{Guid.NewGuid():N}"[..25];
            var product = new Product(categoryId, $"Product {Guid.NewGuid():N}"[..20], slug, 99.99m, _seedUserId);
            product.Delete(_seedUserId);
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return product.Id;
        }

        private async Task<Guid> SeedVariantAsync(Guid productId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var sku = $"SKU-{Guid.NewGuid():N}"[..15];
            var variant = new ProductVariant(productId, sku, 5, _seedUserId);
            db.Set<ProductVariant>().Add(variant);
            await db.SaveChangesAsync();
            return variant.Id;
        }

        private async Task<Guid> SeedDeletedVariantAsync(Guid productId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var sku = $"SKU-{Guid.NewGuid():N}"[..15];
            var variant = new ProductVariant(productId, sku, 5, _seedUserId);
            variant.Delete(_seedUserId);
            db.Set<ProductVariant>().Add(variant);
            await db.SaveChangesAsync();
            return variant.Id;
        }

        private async Task<Guid> SeedImageAsync(Guid productId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var image = new ProductImage(productId, "https://example.com/img.jpg", _seedUserId);
            db.Set<ProductImage>().Add(image);
            await db.SaveChangesAsync();
            return image.Id;
        }

        private async Task<Guid> SeedDeletedImageAsync(Guid productId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var image = new ProductImage(productId, "https://example.com/img-deleted.jpg", _seedUserId);
            image.Delete(_seedUserId);
            db.Set<ProductImage>().Add(image);
            await db.SaveChangesAsync();
            return image.Id;
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/products  (AllowAnonymous)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_TanpaToken_Returns200()
        {
            ClearAuth();

            var response = await _client.GetAsync("/api/products");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_WithPagination_Returns200()
        {
            ClearAuth();

            var response = await _client.GetAsync("/api/products?page=1&pageSize=5");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/products/{slug}  (AllowAnonymous)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetBySlug_SlugValid_Returns200()
        {
            ClearAuth();
            var catId = await SeedCategoryAsync();
            var (_, slug) = await SeedProductAsync(catId);

            var response = await _client.GetAsync($"/api/products/{slug}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetBySlug_SlugTidakAda_Returns404()
        {
            ClearAuth();

            var response = await _client.GetAsync("/api/products/slug-yang-tidak-ada-sama-sekali-xyz");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetBySlug_ProductInaktif_TanpaAdmin_Returns404()
        {
            ClearAuth();
            var catId = await SeedCategoryAsync();
            var inactiveId = await SeedInactiveProductAsync(catId);

            // Need the slug — read it back
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var product = db.Products.Find(inactiveId)!;

            var response = await _client.GetAsync($"/api/products/{product.Slug}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/products  (HasPermission products.create)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_TanpaToken_Returns401()
        {
            ClearAuth();

            var catId = await SeedCategoryAsync();
            var request = new CreateProductRequest(catId, "New Product", $"new-prd-{Guid.NewGuid():N}"[..25], 10.00m, null);

            var response = await _client.PostAsJsonAsync("/api/products", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_WithAdminToken_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var slug = $"new-prd-{Guid.NewGuid():N}"[..25];
            var request = new CreateProductRequest(catId, "Create Product", slug, 50.00m, "desc");

            var response = await _client.PostAsJsonAsync("/api/products", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Create_SlugDuplikat_Returns400()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (_, slug) = await SeedProductAsync(catId);

            var request = new CreateProductRequest(catId, "Dup Slug Product", slug, 10.00m, null);

            var response = await _client.PostAsJsonAsync("/api/products", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // PATCH /api/products/{id}/activate  (HasPermission products.update)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Activate_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.PatchAsync($"/api/products/{Guid.NewGuid()}/activate", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Activate_WithAdminToken_ProductInaktif_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var inactiveId = await SeedInactiveProductAsync(catId);

            var response = await _client.PatchAsync($"/api/products/{inactiveId}/activate", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // PATCH /api/products/{id}/deactivate  (HasPermission products.update)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Deactivate_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.PatchAsync($"/api/products/{Guid.NewGuid()}/deactivate", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Deactivate_WithAdminToken_ProductAktif_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (productId, _) = await SeedProductAsync(catId);

            var response = await _client.PatchAsync($"/api/products/{productId}/deactivate", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // DELETE /api/products/{id}  (HasPermission products.delete)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Delete_WithAdminToken_ProductAda_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (productId, _) = await SeedProductAsync(catId);

            var response = await _client.DeleteAsync($"/api/products/{productId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Delete_ProductTidakAda_Returns404()
        {
            SetAdminToken();

            var response = await _client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/products/{id}/variants  (HasPermission products.create)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task AddVariant_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new AddProductVariantRequest("SKU-001", 10, 0m, null, null);

            var response = await _client.PostAsJsonAsync($"/api/products/{Guid.NewGuid()}/variants", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AddVariant_WithAdminToken_ProductAda_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (productId, _) = await SeedProductAsync(catId);

            var request = new AddProductVariantRequest($"SKU-{Guid.NewGuid():N}"[..15], 10, 0m, "L", "Red");

            var response = await _client.PostAsJsonAsync($"/api/products/{productId}/variants", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/products/{id}/images  (HasPermission products.create)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task AddImage_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new AddProductImageRequest("https://example.com/img.jpg", false, 1);

            var response = await _client.PostAsJsonAsync($"/api/products/{Guid.NewGuid()}/images", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AddImage_WithAdminToken_ProductAda_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (productId, _) = await SeedProductAsync(catId);

            var request = new AddProductImageRequest("https://example.com/image.jpg", true, 1);

            var response = await _client.PostAsJsonAsync($"/api/products/{productId}/images", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // PATCH /api/products/{id}  (HasPermission products.update)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new UpdateProductRequest(null, null, null, null, null);
            var response = await _client.PatchAsJsonAsync($"/api/products/{Guid.NewGuid()}", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Update_WithAdminToken_ProductAda_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (productId, _) = await SeedProductAsync(catId);

            var newSlug = $"upd-prd-{Guid.NewGuid():N}"[..25];
            var request = new UpdateProductRequest("Updated Name", newSlug, 79.99m, null, null);

            var response = await _client.PatchAsJsonAsync($"/api/products/{productId}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/products/{id}/restore  (HasPermission products.restore)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Restore_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.PostAsync($"/api/products/{Guid.NewGuid()}/restore", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Restore_WithAdminToken_ProductDeleted_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var deletedId = await SeedDeletedProductAsync(catId);

            var response = await _client.PostAsync($"/api/products/{deletedId}/restore", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Restore_ProductTidakDihapus_Returns400()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (productId, _) = await SeedProductAsync(catId);

            var response = await _client.PostAsync($"/api/products/{productId}/restore", null);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // PATCH /api/products/variants/{id}  (HasPermission products.update)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateVariant_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new UpdateProductVariantRequest(null, null, null, null, null);
            var response = await _client.PatchAsJsonAsync($"/api/products/variants/{Guid.NewGuid()}", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateVariant_WithAdminToken_VariantAda_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (productId, _) = await SeedProductAsync(catId);
            var variantId = await SeedVariantAsync(productId);

            var request = new UpdateProductVariantRequest(null, 20, null, "XL", "Blue");

            var response = await _client.PatchAsJsonAsync($"/api/products/variants/{variantId}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // DELETE /api/products/variants/{id}  (HasPermission products.delete)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveVariant_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.DeleteAsync($"/api/products/variants/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RemoveVariant_WithAdminToken_VariantAda_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (productId, _) = await SeedProductAsync(catId);
            // Product must have at least 2 active variants to remove one
            var variantId1 = await SeedVariantAsync(productId);
            await SeedVariantAsync(productId);

            var response = await _client.DeleteAsync($"/api/products/variants/{variantId1}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/products/variants/{id}/restore  (HasPermission products.update)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task RestoreVariant_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.PostAsync($"/api/products/variants/{Guid.NewGuid()}/restore", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RestoreVariant_WithAdminToken_VariantDeleted_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (productId, _) = await SeedProductAsync(catId);
            var deletedVariantId = await SeedDeletedVariantAsync(productId);

            var response = await _client.PostAsync($"/api/products/variants/{deletedVariantId}/restore", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // PATCH /api/products/images/{id}  (HasPermission products.update)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateImage_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new UpdateProductImageRequest(null, null, null);
            var response = await _client.PatchAsJsonAsync($"/api/products/images/{Guid.NewGuid()}", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateImage_WithAdminToken_ImageAda_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (productId, _) = await SeedProductAsync(catId);
            var imageId = await SeedImageAsync(productId);

            var request = new UpdateProductImageRequest("https://example.com/new.jpg", false, 2);

            var response = await _client.PatchAsJsonAsync($"/api/products/images/{imageId}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // DELETE /api/products/images/{id}  (HasPermission products.delete)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveImage_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.DeleteAsync($"/api/products/images/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RemoveImage_WithAdminToken_ImageAda_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (productId, _) = await SeedProductAsync(catId);
            // Product must have at least 2 active images to remove one
            var imageId1 = await SeedImageAsync(productId);
            await SeedImageAsync(productId);

            var response = await _client.DeleteAsync($"/api/products/images/{imageId1}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/products/images/{id}/restore  (HasPermission products.update)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task RestoreImage_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.PostAsync($"/api/products/images/{Guid.NewGuid()}/restore", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RestoreImage_WithAdminToken_ImageDeleted_Returns200()
        {
            SetAdminToken();

            var catId = await SeedCategoryAsync();
            var (productId, _) = await SeedProductAsync(catId);
            var deletedImageId = await SeedDeletedImageAsync(productId);

            var response = await _client.PostAsync($"/api/products/images/{deletedImageId}/restore", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
