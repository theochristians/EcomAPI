using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using EComAPI.API.Authorization;
using EComAPI.API.Common;
using EComAPI.API.Categories.Dtos.Requests;
using EComAPI.API.Categories.Dtos.Responses;
using EComAPI.API.Categories.Swagger.Examples;
using EComAPI.API.Categories.Swagger.Examples.GetAllCategories.Response;
using EComAPI.API.Categories.Swagger.Examples.GetCategoryBySlug.Response;
using EComAPI.API.Categories.Swagger.Examples.CreateCategories.Request;
using EComAPI.API.Categories.Swagger.Examples.CreateCategories.Response;
using EComAPI.API.Categories.Swagger.Examples.UpdateCategories.Request;
using EComAPI.API.Categories.Swagger.Examples.UpdateCategories.Response;
using EComAPI.API.Categories.Swagger.Examples.DeleteCategories.Response;
using EComAPI.API.Categories.Swagger.Examples.RestoreCategories.Response;
using EComAPI.Application.Common.Authorization;
using EComAPI.Application.Categories.Queries.GetCategories;
using EComAPI.Application.Categories.Queries.GetCategoriesBySlug;
using EComAPI.Application.Categories.Commands.CreateCategories;
using EComAPI.Application.Categories.Commands.UpdateCategories;
using EComAPI.Application.Categories.Commands.DeleteCategories;
using EComAPI.Application.Categories.Commands.RestoreCategories;

namespace EComAPI.API.Categories.Controllers
{
    /// <summary>
    /// Endpoint manajemen kategori produk.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. Endpoint publik digunakan untuk membaca kategori.
    /// 2. Endpoint tulis membutuhkan permission kategori tertentu.
    /// 3. Controller memetakan request ke command/query Application Layer.
    /// 4. Hasil dikembalikan dalam format ApiResponse yang konsisten.
    /// </remarks>
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : BaseController
    {
        private readonly CreateCategoryHandler _createCategoriesHandler;
        private readonly GetCategoryHandler _getCategoriesHandler;
        private readonly GetCategoryBySlugHandler _getCategoriesBySlugHandler;
        private readonly UpdateCategoryHandler _updateCategoriesHandler;
        private readonly DeleteCategoryHandler _deleteCategoriesHandler;
        private readonly RestoreCategoryHandler _restoreCategoriesHandler;

        /// <summary>
        /// Inisialisasi CategoryController dengan seluruh handler kategori.
        /// </summary>
        /// <param name="createCategoriesHandler">Handler untuk membuat kategori baru.</param>
        /// <param name="getCategoriesHandler">Handler untuk mengambil daftar kategori.</param>
        /// <param name="getCategoriesBySlugHandler">Handler untuk mengambil detail kategori berdasarkan slug.</param>
        /// <param name="updateCategoriesHandler">Handler untuk memperbarui kategori.</param>
        /// <param name="deleteCategoriesHandler">Handler untuk soft delete kategori.</param>
        /// <param name="restoreCategoriesHandler">Handler untuk restore kategori yang terhapus.</param>
        public CategoryController(
            CreateCategoryHandler createCategoriesHandler,
            GetCategoryHandler getCategoriesHandler,
            GetCategoryBySlugHandler getCategoriesBySlugHandler,
            UpdateCategoryHandler updateCategoriesHandler,
            DeleteCategoryHandler deleteCategoriesHandler,
            RestoreCategoryHandler restoreCategoriesHandler)
        {
            _createCategoriesHandler = createCategoriesHandler;
            _getCategoriesHandler = getCategoriesHandler;
            _getCategoriesBySlugHandler = getCategoriesBySlugHandler;
            _updateCategoriesHandler = updateCategoriesHandler;
            _deleteCategoriesHandler = deleteCategoriesHandler;
            _restoreCategoriesHandler = restoreCategoriesHandler;
        }

        /// <summary>
        /// Mengambil seluruh kategori produk.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/categories
        ///
        /// Flow get all categories:
        /// 1. Endpoint dapat diakses tanpa autentikasi.
        /// 2. Controller memanggil query handler untuk daftar kategori.
        /// 3. Jika gagal, kembalikan HTTP 400.
        /// 4. Jika berhasil, mapping DTO kategori ke CategoryResponse.
        /// </remarks>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi daftar kategori.</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetAllCategorySuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GetAllCategoryBadRequestExample))]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            // Step 1: Ambil daftar kategori.
            var getCategoriesResult = await _getCategoriesHandler.Handle(cancellationToken);

            // Step 2: Jika gagal, kirim response error.
            if (!getCategoriesResult.IsSuccess)
                return BadRequestResponse("Failed list categories");

            // Step 3: Mapping DTO kategori ke response API.
            var categoriesListResponse = getCategoriesResult.Value!
                .Select(categoriesDto => new CategoryResponse(
                    categoriesDto.Id,
                    categoriesDto.Name,
                    categoriesDto.Slug,
                    categoriesDto.ParentId,
                    categoriesDto.ImageUrl,
                    categoriesDto.Description,
                    categoriesDto.ProductCount,
                    categoriesDto.CreatedAt,
                    categoriesDto.UpdatedAt
                ))
                .ToList();

            // Step 4: Kembalikan response sukses.
            return SuccessResponse(new
            {
                product_categories = categoriesListResponse
            }, "Success list categories");
        }

        /// <summary>
        /// Mengambil detail kategori berdasarkan slug.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/categories/{slug}
        ///
        /// Flow get category by slug:
        /// 1. Endpoint dapat diakses tanpa autentikasi.
        /// 2. Controller memanggil query handler menggunakan slug dari route.
        /// 3. Jika kategori tidak ditemukan, kembalikan HTTP 404.
        /// 4. Jika validasi/input gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan detail kategori.
        /// </remarks>
        /// <param name="slug">Slug kategori yang dicari.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi detail kategori.</returns>
        [HttpGet("{slug}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetCategoryBySlugSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GetCategoryBySlugBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GetCategoryBySlugNotFoundExample))]
        public async Task<IActionResult> GetBySlug(
            string slug,
            CancellationToken cancellationToken)
        {
            // Step 1: Bentuk query berdasarkan slug.
            var getCategoriesBySlugQuery = new GetCategoryBySlugQuery(slug);

            // Step 2: Eksekusi query handler.
            var categoriesDTOsBySlugResult = await _getCategoriesBySlugHandler
                .Handle(getCategoriesBySlugQuery, cancellationToken);

            // Step 3: Jika gagal, petakan ke 404 (not found) atau 400 (bad request).
            if (!categoriesDTOsBySlugResult.IsSuccess)
            {
                if (IsCategoryNotFound(categoriesDTOsBySlugResult.Error))
                    return NotFoundResponse(categoriesDTOsBySlugResult.Error!);

                return BadRequestResponse(categoriesDTOsBySlugResult.Error ?? "Failed to get category");
            }

            // Step 4: Mapping DTO ke response API.
            var categoriesResponse = new CategoryResponse(
                categoriesDTOsBySlugResult.Value!.Id,
                categoriesDTOsBySlugResult.Value.Name,
                categoriesDTOsBySlugResult.Value.Slug,
                categoriesDTOsBySlugResult.Value.ParentId,
                categoriesDTOsBySlugResult.Value.ImageUrl,
                categoriesDTOsBySlugResult.Value.Description,
                categoriesDTOsBySlugResult.Value.ProductCount,
                categoriesDTOsBySlugResult.Value.CreatedAt,
                categoriesDTOsBySlugResult.Value.UpdatedAt
            );

            // Step 5: Kembalikan response sukses.
            return SuccessResponse(categoriesResponse, "Success get category");
        }

        /// <summary>
        /// Membuat kategori baru.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/categories
        ///
        /// Flow create category:
        /// 1. Endpoint membutuhkan permission Categories.Create.
        /// 2. Terima payload kategori dari request body.
        /// 3. Mapping payload ke CreateCategoryCommand.
        /// 4. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan ID kategori baru.
        /// </remarks>
        /// <param name="createCategoriesRequest">Payload pembuatan kategori.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status pembuatan kategori.</returns>
        [HttpPost]
        [HasPermission(Permissions.Categories.Create)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerRequestExample(typeof(CreateCategoryRequest), typeof(CreateCategoryRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CreateCategorySuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(CreateCategoryBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        public async Task<IActionResult> Create(
            CreateCategoryRequest createCategoriesRequest,
            CancellationToken cancellationToken)
        {
            // Step 1: Mapping request ke command aplikasi.
            var createCategoriesCommand = new CreateCategoryCommand(
                createCategoriesRequest.Name,
                createCategoriesRequest.Slug,
                createCategoriesRequest.ParentId,
                createCategoriesRequest.ImageUrl,
                createCategoriesRequest.Description
            );

            // Step 2: Eksekusi proses create kategori.
            var createCategoriesResult = await _createCategoriesHandler
                .Handle(createCategoriesCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!createCategoriesResult.IsSuccess)
                return BadRequestResponse(createCategoriesResult.Error ?? "Create category failed");

            // Step 4: Kembalikan ID kategori yang berhasil dibuat.
            return SuccessResponse(
                new { categoryId = createCategoriesResult.Value },
                "Category created successfully"
            );
        }

        /// <summary>
        /// Memperbarui data kategori.
        /// </summary>
        /// <remarks>
        /// Endpoint: PATCH api/categories/{id}
        ///
        /// Flow update category:
        /// 1. Endpoint membutuhkan permission Categories.Update.
        /// 2. Terima id kategori dari route dan payload perubahan dari body.
        /// 3. Mapping ke UpdateCategoryCommand.
        /// 4. Jika kategori tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika validasi/proses gagal, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan ID kategori yang diperbarui.
        /// </remarks>
        /// <param name="id">ID kategori yang akan diperbarui.</param>
        /// <param name="updateCategoriesRequest">Payload perubahan kategori.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status pembaruan kategori.</returns>
        [HttpPatch("{id}")]
        [HasPermission(Permissions.Categories.Update)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(UpdateCategoryRequest), typeof(UpdateCategoryRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UpdateCategorySuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(UpdateCategoryBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GetCategoryBySlugNotFoundExample))]
        public async Task<IActionResult> Update(
            Guid id,
            UpdateCategoryRequest updateCategoriesRequest,
            CancellationToken cancellationToken)
        {
            // Step 1: Mapping route + request ke command aplikasi.
            var updateCategoriesCommand = new UpdateCategoryCommand(
                id,
                updateCategoriesRequest.Name,
                updateCategoriesRequest.Slug,
                updateCategoriesRequest.ParentId,
                updateCategoriesRequest.ImageUrl,
                updateCategoriesRequest.Description
            );

            // Step 2: Eksekusi proses update kategori.
            var updateCategoriesResult = await _updateCategoriesHandler
                .Handle(updateCategoriesCommand, cancellationToken);

            // Step 3: Jika gagal, petakan ke 404 atau 400.
            if (!updateCategoriesResult.IsSuccess)
            {
                if (IsCategoryNotFound(updateCategoriesResult.Error))
                    return NotFoundResponse(updateCategoriesResult.Error!);

                return BadRequestResponse(updateCategoriesResult.Error ?? "Update category failed");
            }

            // Step 4: Kembalikan ID kategori yang berhasil diperbarui.
            return SuccessResponse(
                new { categoryId = updateCategoriesResult.Value },
                "Category updated successfully"
            );
        }

        /// <summary>
        /// Menghapus kategori (soft delete).
        /// </summary>
        /// <remarks>
        /// Endpoint: DELETE api/categories/{id}
        ///
        /// Flow delete category:
        /// 1. Endpoint membutuhkan permission Categories.Delete.
        /// 2. Terima id kategori dari route.
        /// 3. Mapping ke DeleteCategoryCommand.
        /// 4. Jika kategori tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika aturan bisnis gagal, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan ID kategori yang dihapus.
        /// </remarks>
        /// <param name="id">ID kategori yang akan dihapus.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status penghapusan kategori.</returns>
        [HttpDelete("{id}")]
        [HasPermission(Permissions.Categories.Delete)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DeleteCategorySuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(DeleteCategoryBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GetCategoryBySlugNotFoundExample))]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            // Step 1: Bentuk command delete kategori.
            var deleteCategoriesCommand = new DeleteCategoryCommand(id);

            // Step 2: Eksekusi proses delete kategori.
            var deleteCategoriesResult = await _deleteCategoriesHandler
                .Handle(deleteCategoriesCommand, cancellationToken);

            // Step 3: Jika gagal, petakan ke 404 atau 400.
            if (!deleteCategoriesResult.IsSuccess)
            {
                if (IsCategoryNotFound(deleteCategoriesResult.Error))
                    return NotFoundResponse(deleteCategoriesResult.Error!);

                return BadRequestResponse(deleteCategoriesResult.Error ?? "Delete category failed");
            }

            // Step 4: Kembalikan ID kategori yang berhasil dihapus.
            return SuccessResponse(
                new CategoryIdResponse(deleteCategoriesResult.Value),
                "Category deleted success"
            );
        }

        /// <summary>
        /// Me-restore kategori yang sudah di-soft-delete.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/categories/{id}/restore
        ///
        /// Flow restore category:
        /// 1. Endpoint membutuhkan permission Categories.Restore.
        /// 2. Terima id kategori dari route.
        /// 3. Mapping ke RestoreCategoryCommand.
        /// 4. Jika kategori tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika aturan bisnis gagal, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan ID kategori yang direstore.
        /// </remarks>
        /// <param name="id">ID kategori yang akan direstore.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status restore kategori.</returns>
        [HttpPost("{id}/restore")]
        [HasPermission(Permissions.Categories.Restore)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RestoreCategorySuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(RestoreCategoryBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GetCategoryBySlugNotFoundExample))]
        public async Task<IActionResult> Restore(
            Guid id,
            CancellationToken cancellationToken)
        {
            // Step 1: Bentuk command restore kategori.
            var restoreCategoriesCommand = new RestoreCategoryCommand(id);

            // Step 2: Eksekusi proses restore kategori.
            var restoreCategoriesResult = await _restoreCategoriesHandler
                .Handle(restoreCategoriesCommand, cancellationToken);

            // Step 3: Jika gagal, petakan ke 404 atau 400.
            if (!restoreCategoriesResult.IsSuccess)
            {
                if (IsCategoryNotFound(restoreCategoriesResult.Error))
                    return NotFoundResponse(restoreCategoriesResult.Error!);

                return BadRequestResponse(restoreCategoriesResult.Error ?? "Restore category failed");
            }

            // Step 4: Kembalikan ID kategori yang berhasil direstore.
            return SuccessResponse(
                new CategoryIdResponse(restoreCategoriesResult.Value),
                "Category restored success"
            );
        }

        private static bool IsCategoryNotFound(string? error)
            => string.Equals(error, "Category not found", StringComparison.OrdinalIgnoreCase);
    }
}
