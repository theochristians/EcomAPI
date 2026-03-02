using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using EComAPI.API.Categories.Dtos.Requests;
using EComAPI.API.Tests.Infrastructure;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Categories.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.API.Tests.Categories
{
    public class CategoriesControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CategoriesControllerTests(TestWebApplicationFactory factory)
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

        private static readonly Guid _seedUserId = Guid.NewGuid();

        private async Task<Guid> SeedCategoryAsync(string name, string slug)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var category = new Category(name, slug, _seedUserId);
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return category.Id;
        }

        private async Task<Guid> SeedDeletedCategoryAsync(string name, string slug)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var category = new Category(name, slug, _seedUserId);
            category.Delete(_seedUserId);
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return category.Id;
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/categories  (AllowAnonymous)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_TanpaToken_Returns200()
        {
            ClearAuth();

            var response = await _client.GetAsync("/api/categories");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task GetAll_AdaKategori_ReturnsData()
        {
            ClearAuth();
            await SeedCategoryAsync($"Cat_{Guid.NewGuid():N}"[..20], $"cat-{Guid.NewGuid():N}"[..20]);

            var response = await _client.GetAsync("/api/categories");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ─────────────────────────────────────────────────────────
        // GET /api/categories/{slug}  (AllowAnonymous)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetBySlug_SlugValid_Returns200()
        {
            ClearAuth();
            var slug = $"slug-{Guid.NewGuid():N}"[..20];
            await SeedCategoryAsync("Slug Category", slug);

            var response = await _client.GetAsync($"/api/categories/{slug}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetBySlug_SlugTidakAda_Returns404()
        {
            ClearAuth();

            var response = await _client.GetAsync("/api/categories/slug-yang-tidak-ada-sama-sekali");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/categories  (HasPermission categories.create)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_TanpaToken_Returns401()
        {
            ClearAuth();

            var request = new CreateCategoryRequest(
                "New Category",
                "new-category",
                null, null, null);

            var response = await _client.PostAsJsonAsync("/api/categories", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_WithAdminToken_Returns200()
        {
            SetAdminToken();

            var slug = $"admin-cat-{Guid.NewGuid():N}"[..20];
            var request = new CreateCategoryRequest(
                "Admin Category",
                slug,
                null, null, null);

            var response = await _client.PostAsJsonAsync("/api/categories", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<TestApiResponse<object>>(_json);
            body!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Create_SlugSudahAda_Returns400()
        {
            SetAdminToken();

            var slug = $"dup-cat-{Guid.NewGuid():N}"[..20];
            await SeedCategoryAsync("Existing Cat", slug);

            var request = new CreateCategoryRequest("Duplicate", slug, null, null, null);

            var response = await _client.PostAsJsonAsync("/api/categories", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ─────────────────────────────────────────────────────────
        // PATCH /api/categories/{id}  (HasPermission categories.update)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_TanpaToken_Returns401()
        {
            ClearAuth();

            var id = Guid.NewGuid();
            var request = new UpdateCategoryRequest("New Name", null, null, null, null);

            var response = await _client.PatchAsJsonAsync($"/api/categories/{id}", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Update_WithAdminToken_KategoriAda_Returns200()
        {
            SetAdminToken();

            var slug = $"upd-cat-{Guid.NewGuid():N}"[..20];
            var id = await SeedCategoryAsync("Update Me", slug);

            var request = new UpdateCategoryRequest("Updated Name", null, null, null, null);

            var response = await _client.PatchAsJsonAsync($"/api/categories/{id}", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Update_KategoriTidakAda_Returns404()
        {
            SetAdminToken();

            var request = new UpdateCategoryRequest("X", null, null, null, null);

            var response = await _client.PatchAsJsonAsync($"/api/categories/{Guid.NewGuid()}", request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ─────────────────────────────────────────────────────────
        // DELETE /api/categories/{id}  (HasPermission categories.delete)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.DeleteAsync($"/api/categories/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Delete_WithAdminToken_KategoriAda_Returns200()
        {
            SetAdminToken();

            var slug = $"del-cat-{Guid.NewGuid():N}"[..20];
            var id = await SeedCategoryAsync("Delete Me", slug);

            var response = await _client.DeleteAsync($"/api/categories/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Delete_KategoriTidakAda_Returns404()
        {
            SetAdminToken();

            var response = await _client.DeleteAsync($"/api/categories/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ─────────────────────────────────────────────────────────
        // POST /api/categories/{id}/restore  (HasPermission categories.restore)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Restore_TanpaToken_Returns401()
        {
            ClearAuth();

            var response = await _client.PostAsync($"/api/categories/{Guid.NewGuid()}/restore", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Restore_WithAdminToken_KategoriDeleted_Returns200()
        {
            SetAdminToken();

            var slug = $"rst-cat-{Guid.NewGuid():N}"[..20];
            var id = await SeedDeletedCategoryAsync("Restore Me", slug);

            var response = await _client.PostAsync($"/api/categories/{id}/restore", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Restore_KategoriTidakDihapus_Returns400()
        {
            SetAdminToken();

            // Seed a normal (non-deleted) category
            var slug = $"act-cat-{Guid.NewGuid():N}"[..20];
            var id = await SeedCategoryAsync("Active Category", slug);

            // Attempting to restore a category that isn't deleted should fail
            var response = await _client.PostAsync($"/api/categories/{id}/restore", null);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
