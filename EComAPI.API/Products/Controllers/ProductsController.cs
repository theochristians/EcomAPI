using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using EComAPI.API.Authorization;
using EComAPI.API.Common;
using EComAPI.API.Products.Dtos.Requests;
using EComAPI.API.Products.Dtos.Responses;
using EComAPI.API.Products.Swagger.Examples;
using EComAPI.API.Products.Swagger.Examples.ProductExample.GetAllProduct.Response;
using EComAPI.API.Products.Swagger.Examples.ProductExample.GetProductBySlug.Response;
using EComAPI.API.Products.Swagger.Examples.ProductExample.CreateProduct.Request;
using EComAPI.API.Products.Swagger.Examples.ProductExample.CreateProduct.Response;
using EComAPI.API.Products.Swagger.Examples.ProductExample.UpdateProduct.Request;
using EComAPI.API.Products.Swagger.Examples.ProductExample.UpdateProduct.Response;
using EComAPI.API.Products.Swagger.Examples.ProductExample.ActivateProduct.Response;
using EComAPI.API.Products.Swagger.Examples.ProductExample.DeactivateProduct.Response;
using EComAPI.API.Products.Swagger.Examples.ProductExample.DeleteProduct.Response;
using EComAPI.API.Products.Swagger.Examples.ProductExample.RestoreProduct.Response;
using EComAPI.API.Products.Swagger.Examples.ProductVariantExample.AddVariant.Request;
using EComAPI.API.Products.Swagger.Examples.ProductVariantExample.AddVariant.Response;
using EComAPI.API.Products.Swagger.Examples.ProductVariantExample.UpdateVariant.Request;
using EComAPI.API.Products.Swagger.Examples.ProductVariantExample.UpdateVariant.Response;
using EComAPI.API.Products.Swagger.Examples.ProductVariantExample.RemoveVariant.Response;
using EComAPI.API.Products.Swagger.Examples.ProductVariantExample.RestoreVariant.Response;
using EComAPI.API.Products.Swagger.Examples.ProductImageExample.AddImage.Request;
using EComAPI.API.Products.Swagger.Examples.ProductImageExample.AddImage.Response;
using EComAPI.API.Products.Swagger.Examples.ProductImageExample.UpdateImage.Request;
using EComAPI.API.Products.Swagger.Examples.ProductImageExample.UpdateImage.Response;
using EComAPI.API.Products.Swagger.Examples.ProductImageExample.RemoveImage.Response;
using EComAPI.API.Products.Swagger.Examples.ProductImageExample.RestoreImage.Response;
using EComAPI.Application.Common.Authorization;
using EComAPI.Application.Products.Commands.ProductCommands.ActivateProduct;
using EComAPI.Application.Products.Commands.ProductCommands.CreateProduct;
using EComAPI.Application.Products.Commands.ProductCommands.DeactivateProduct;
using EComAPI.Application.Products.Commands.ProductCommands.DeleteProduct;
using EComAPI.Application.Products.Commands.ProductCommands.RecordProductView;
using EComAPI.Application.Products.Commands.ProductCommands.RestoreProduct;
using EComAPI.Application.Products.Commands.ProductCommands.UpdateProduct;
using EComAPI.Application.Products.Commands.ProductImageCommands.AddProductImage;
using EComAPI.Application.Products.Commands.ProductImageCommands.RemoveProductImage;
using EComAPI.Application.Products.Commands.ProductImageCommands.RestoreProductImage;
using EComAPI.Application.Products.Commands.ProductImageCommands.UpdateProductImage;
using EComAPI.Application.Products.Commands.ProductVariantCommands.AddProductVariant;
using EComAPI.Application.Products.Commands.ProductVariantCommands.RemoveProductVariant;
using EComAPI.Application.Products.Commands.ProductVariantCommands.RestoreProductVariant;
using EComAPI.Application.Products.Commands.ProductVariantCommands.UpdateProductVariant;
using EComAPI.Application.Products.Queries.GetProductBySlug;
using EComAPI.Application.Products.Queries.GetProducts;

namespace EComAPI.API.Products.Controllers
{
    /// <summary>
    /// Endpoint manajemen produk, varian produk, dan gambar produk.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. Endpoint baca publik dapat diakses tanpa autentikasi.
    /// 2. Endpoint tulis menggunakan permission berbasis role.
    /// 3. Controller memetakan request ke command/query Application Layer.
    /// 4. Hasil dikembalikan sebagai ApiResponse yang konsisten.
    /// </remarks>
    [ApiController]
    [Route("api/products")]
    public class ProductsController : BaseController
    {
        private readonly GetProductsHandler _getProductsHandler;
        private readonly GetProductBySlugHandler _getProductBySlugHandler;
        private readonly CreateProductHandler _createProductHandler;
        private readonly UpdateProductHandler _updateProductHandler;
        private readonly ActivateProductHandler _activateProductHandler;
        private readonly DeactivateProductHandler _deactivateProductHandler;
        private readonly DeleteProductHandler _deleteProductHandler;
        private readonly RecordProductViewHandler _recordProductViewHandler;
        private readonly RestoreProductHandler _restoreProductHandler;

        private readonly AddProductVariantHandler _addProductVariantHandler;
        private readonly UpdateProductVariantHandler _updateProductVariantHandler;
        private readonly RemoveProductVariantHandler _removeProductVariantHandler;
        private readonly RestoreProductVariantHandler _restoreProductVariantHandler;

        private readonly AddProductImageHandler _addProductImageHandler;
        private readonly UpdateProductImageHandler _updateProductImageHandler;
        private readonly RemoveProductImageHandler _removeProductImageHandler;
        private readonly RestoreProductImageHandler _restoreProductImageHandler;

        /// <summary>
        /// Inisialisasi ProductsController dengan seluruh handler produk.
        /// </summary>
        /// <param name="getProductsHandler">Handler untuk daftar produk dengan filter/paginasi.</param>
        /// <param name="getProductBySlugHandler">Handler untuk detail produk berdasarkan slug.</param>
        /// <param name="createProductHandler">Handler untuk membuat produk baru.</param>
        /// <param name="updateProductHandler">Handler untuk memperbarui produk.</param>
        /// <param name="activateProductHandler">Handler untuk mengaktifkan produk.</param>
        /// <param name="deactivateProductHandler">Handler untuk menonaktifkan produk.</param>
        /// <param name="deleteProductHandler">Handler untuk menghapus produk (soft delete).</param>
        /// <param name="recordProductViewHandler">Handler untuk mencatat view produk.</param>
        /// <param name="restoreProductHandler">Handler untuk me-restore produk yang dihapus.</param>
        /// <param name="addProductVariantHandler">Handler untuk menambah varian produk.</param>
        /// <param name="updateProductVariantHandler">Handler untuk memperbarui varian produk.</param>
        /// <param name="removeProductVariantHandler">Handler untuk menghapus varian produk.</param>
        /// <param name="restoreProductVariantHandler">Handler untuk me-restore varian produk.</param>
        /// <param name="addProductImageHandler">Handler untuk menambah gambar produk.</param>
        /// <param name="updateProductImageHandler">Handler untuk memperbarui gambar produk.</param>
        /// <param name="removeProductImageHandler">Handler untuk menghapus gambar produk.</param>
        /// <param name="restoreProductImageHandler">Handler untuk me-restore gambar produk.</param>
        public ProductsController(
            GetProductsHandler getProductsHandler,
            GetProductBySlugHandler getProductBySlugHandler,
            CreateProductHandler createProductHandler,
            UpdateProductHandler updateProductHandler,
            ActivateProductHandler activateProductHandler,
            DeactivateProductHandler deactivateProductHandler,
            DeleteProductHandler deleteProductHandler,
            RecordProductViewHandler recordProductViewHandler,
            RestoreProductHandler restoreProductHandler,
            AddProductVariantHandler addProductVariantHandler,
            UpdateProductVariantHandler updateProductVariantHandler,
            RemoveProductVariantHandler removeProductVariantHandler,
            RestoreProductVariantHandler restoreProductVariantHandler,
            AddProductImageHandler addProductImageHandler,
            UpdateProductImageHandler updateProductImageHandler,
            RemoveProductImageHandler removeProductImageHandler,
            RestoreProductImageHandler restoreProductImageHandler)
        {
            _getProductsHandler = getProductsHandler;
            _getProductBySlugHandler = getProductBySlugHandler;
            _createProductHandler = createProductHandler;
            _updateProductHandler = updateProductHandler;
            _activateProductHandler = activateProductHandler;
            _deactivateProductHandler = deactivateProductHandler;
            _deleteProductHandler = deleteProductHandler;
            _recordProductViewHandler = recordProductViewHandler;
            _restoreProductHandler = restoreProductHandler;
            _addProductVariantHandler = addProductVariantHandler;
            _updateProductVariantHandler = updateProductVariantHandler;
            _removeProductVariantHandler = removeProductVariantHandler;
            _restoreProductVariantHandler = restoreProductVariantHandler;
            _addProductImageHandler = addProductImageHandler;
            _updateProductImageHandler = updateProductImageHandler;
            _removeProductImageHandler = removeProductImageHandler;
            _restoreProductImageHandler = restoreProductImageHandler;
        }

        #region Product
        /// <summary>
        /// Mengambil daftar produk dengan pagination, filtering, dan sorting.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/products
        ///
        /// Flow get all products:
        /// 1. Endpoint publik (AllowAnonymous).
        /// 2. Query parameter dipetakan ke GetProductsQuery.
        /// 3. Jika includeInactive=true dan user bukan Admin, flag akan diabaikan.
        /// 4. Hasil dikembalikan dalam format PaginatedApiResponse.
        /// </remarks>
        /// <param name="page">Nomor halaman data (default: 1).</param>
        /// <param name="pageSize">Jumlah item per halaman (default: 10).</param>
        /// <param name="categorySlug">Filter berdasarkan slug kategori.</param>
        /// <param name="includeSubcategories">Jika true, sertakan produk dari subkategori.</param>
        /// <param name="includeInactive">Jika true, sertakan produk non-aktif (khusus Admin).</param>
        /// <param name="minPrice">Filter harga minimum.</param>
        /// <param name="maxPrice">Filter harga maksimum.</param>
        /// <param name="inStock">Filter ketersediaan stok.</param>
        /// <param name="searchTerm">Kata kunci pencarian nama/slug produk.</param>
        /// <param name="sortBy">Field sorting (contoh: createdAt, price).</param>
        /// <param name="sortOrder">Arah sorting (asc/desc).</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>PaginatedApiResponse daftar produk.</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedApiResponse<IReadOnlyList<ProductListResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetAllProductsSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GetAllProductsBadRequestExample))]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? categorySlug = null,
            [FromQuery] bool includeSubcategories = false,
            [FromQuery] bool includeInactive = false,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? inStock = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] string sortOrder = "desc",
            CancellationToken cancellationToken = default)
        {
            if (User.IsInRole("Admin"))
            {
                includeInactive = true;
            }
            else
            {
                includeInactive = false;
            }

            var query = new GetProductsQuery(
                categorySlug,
                includeSubcategories,
                includeInactive,
                minPrice,
                maxPrice,
                inStock,
                searchTerm,
                sortBy,
                sortOrder,
                page,
                pageSize
            );

            var result = await _getProductsHandler.Handle(query, cancellationToken);

            if (!result.IsSuccess)
                return BadRequestResponse(result.Error ?? "Failed to list products");

            var (items, totalCount, _) = result.Value;

            var productListResponses = items
                .Select(ProductListResponse.FromDto)
                .ToList();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var queryString = Request.QueryString.Value ?? string.Empty;

            var response = PaginatedApiResponse<IReadOnlyList<ProductListResponse>>.Create(
                productListResponses,
                totalCount,
                page,
                pageSize,
                baseUrl,
                queryString,
                "Success list products"
            );

            return Ok(response);
        }

        /// <summary>
        /// Mengambil detail produk berdasarkan slug.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/products/{slug}
        ///
        /// Flow get by slug:
        /// 1. Endpoint publik (AllowAnonymous).
        /// 2. Controller memanggil query handler berdasarkan slug.
        /// 3. Jika produk tidak ditemukan, kembalikan HTTP 404.
        /// 4. Jika produk non-aktif dan requester bukan Admin, kembalikan HTTP 404.
        /// 5. Jika sukses, hitung view dan kembalikan detail produk.
        /// </remarks>
        /// <param name="slug">Slug produk yang dicari.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi detail produk.</returns>
        [HttpGet("{slug}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProductDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetProductBySlugSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GetProductBySlugBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GetProductBySlugNotFoundExample))]
        public async Task<IActionResult> GetBySlug(
            string slug,
            CancellationToken cancellationToken = default)
        {
            var getProductBySlugResult = await _getProductBySlugHandler.Handle(
                new GetProductBySlugQuery(slug),
                cancellationToken);

            if (!getProductBySlugResult.IsSuccess)
            {
                if (IsProductNotFound(getProductBySlugResult.Error))
                    return NotFoundResponse(getProductBySlugResult.Error!);

                return BadRequestResponse(getProductBySlugResult.Error ?? "Failed to get product");
            }

            if (!User.IsInRole("Admin") && !getProductBySlugResult.Value!.IsActive)
                return NotFoundResponse("Product not found");

            _ = await _recordProductViewHandler.Handle(
                new RecordProductViewCommand(getProductBySlugResult.Value!.Id),
                cancellationToken);

            var productDetailResponse = ProductDetailResponse.FromDto(getProductBySlugResult.Value!);

            return SuccessResponse(productDetailResponse, "Success get product");
        }

        /// <summary>
        /// Membuat produk baru.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/products
        ///
        /// Flow create product:
        /// 1. Endpoint membutuhkan permission Products.Create.
        /// 2. Terima payload produk dari request body.
        /// 3. Mapping payload ke CreateProductCommand.
        /// 4. Handler memvalidasi lalu menyimpan produk baru.
        /// 5. Jika gagal, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan productId.
        /// </remarks>
        /// <param name="createProductRequest">Payload data produk baru.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status pembuatan produk.</returns>
        [HttpPost]
        [HasPermission(Permissions.Products.Create)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerRequestExample(typeof(CreateProductRequest), typeof(CreateProductRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CreateProductSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(CreateProductBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        public async Task<IActionResult> Create(
            CreateProductRequest createProductRequest,
            CancellationToken cancellationToken = default)
        {
            var createProductCommand = new CreateProductCommand(
                createProductRequest.CategoryId,
                createProductRequest.Name,
                createProductRequest.Slug,
                createProductRequest.BasePrice,
                createProductRequest.Description
            );

            var createProductResult = await _createProductHandler.Handle(
                createProductCommand,
                cancellationToken);

            if (!createProductResult.IsSuccess)
                return BadRequestResponse(createProductResult.Error ?? "Product create failed");

            return SuccessResponse(
                new { productId = createProductResult.Value },
                "Product created success"
            );
        }

        /// <summary>
        /// Memperbarui data produk.
        /// </summary>
        /// <remarks>
        /// Endpoint: PATCH api/products/{id}
        ///
        /// Flow update product:
        /// 1. Endpoint membutuhkan permission Products.Update.
        /// 2. Terima id produk dari route dan payload perubahan dari body.
        /// 3. Mapping ke UpdateProductCommand.
        /// 4. Jika produk tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika validasi/proses gagal, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan productId.
        /// </remarks>
        /// <param name="id">Id produk yang akan diperbarui.</param>
        /// <param name="updateProductRequest">Payload perubahan data produk.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status pembaruan produk.</returns>
        [HttpPatch("{id}")]
        [HasPermission(Permissions.Products.Update)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(UpdateProductRequest), typeof(UpdateProductRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UpdateProductSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(UpdateProductBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ProductNotFoundExample))]
        public async Task<IActionResult> Update(
            Guid id,
            UpdateProductRequest updateProductRequest,
            CancellationToken cancellationToken)
        {
            var command = new UpdateProductCommand(
                id,
                updateProductRequest.Name,
                updateProductRequest.Slug,
                updateProductRequest.BasePrice,
                updateProductRequest.CategoryId,
                updateProductRequest.Description
            );

            var result = await _updateProductHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (IsProductNotFound(result.Error))
                    return NotFoundResponse(result.Error!);

                return BadRequestResponse(result.Error ?? "Update product failed");
            }

            return SuccessResponse(
                new { productId = result.Value },
                "Product updated successfully"
            );
        }

        /// <summary>
        /// Menonaktifkan produk.
        /// </summary>
        /// <remarks>
        /// Endpoint: PATCH api/products/{id}/deactivate
        ///
        /// Flow deactivate product:
        /// 1. Endpoint membutuhkan permission Products.Update.
        /// 2. Terima id produk dari route.
        /// 3. Handler mengubah status produk menjadi nonaktif.
        /// 4. Jika produk tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// </remarks>
        /// <param name="id">Id produk yang akan dinonaktifkan.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status deaktivasi produk.</returns>
        [HttpPatch("{id}/deactivate")]
        [HasPermission(Permissions.Products.Update)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DeactivateProductSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(DeactivateProductBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ProductNotFoundExample))]
        public async Task<IActionResult> Deactivate(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var command = new DeactivateProductCommand(id);
            var result = await _deactivateProductHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (IsProductNotFound(result.Error))
                    return NotFoundResponse(result.Error!);

                return BadRequestResponse(result.Error ?? "Deactivate product failed");
            }

            return SuccessResponse(
                new { productId = result.Value },
                "Product deactivated successfully"
            );
        }

        /// <summary>
        /// Mengaktifkan produk.
        /// </summary>
        /// <remarks>
        /// Endpoint: PATCH api/products/{id}/activate
        ///
        /// Flow activate product:
        /// 1. Endpoint membutuhkan permission Products.Update.
        /// 2. Terima id produk dari route.
        /// 3. Handler mengubah status produk menjadi aktif.
        /// 4. Jika produk tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// </remarks>
        /// <param name="id">Id produk yang akan diaktifkan.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status aktivasi produk.</returns>
        [HttpPatch("{id}/activate")]
        [HasPermission(Permissions.Products.Update)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ActivateProductSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ActivateProductBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ProductNotFoundExample))]
        public async Task<IActionResult> Activate(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var command = new ActivateProductCommand(id);
            var result = await _activateProductHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (IsProductNotFound(result.Error))
                    return NotFoundResponse(result.Error!);

                return BadRequestResponse(result.Error ?? "Activate product failed");
            }

            return SuccessResponse(
                new { productId = result.Value },
                "Product activated successfully"
            );
        }

        /// <summary>
        /// Menghapus produk (soft delete).
        /// </summary>
        /// <remarks>
        /// Endpoint: DELETE api/products/{id}
        ///
        /// Flow delete product:
        /// 1. Endpoint membutuhkan permission Products.Delete.
        /// 2. Terima id produk dari route.
        /// 3. Handler menjalankan proses soft delete produk.
        /// 4. Jika produk tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan productId.
        /// </remarks>
        /// <param name="id">Id produk yang akan dihapus.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status penghapusan produk.</returns>
        [HttpDelete("{id}")]
        [HasPermission(Permissions.Products.Delete)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DeleteProductSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(DeleteProductBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ProductNotFoundExample))]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            var deleteProductCommand = new DeleteProductCommand(id);

            var deleteProductResult = await _deleteProductHandler.Handle(deleteProductCommand, cancellationToken);

            if (!deleteProductResult.IsSuccess)
            {
                if (IsProductNotFound(deleteProductResult.Error))
                    return NotFoundResponse(deleteProductResult.Error!);

                return BadRequestResponse(deleteProductResult.Error ?? "Delete product failed");
            }

            return SuccessResponse(
                new { productId = deleteProductResult.Value },
                "Product deleted and deactivated successfully"
            );
        }

        /// <summary>
        /// Me-restore produk yang sudah dihapus.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/products/{id}/restore
        ///
        /// Flow restore product:
        /// 1. Endpoint membutuhkan permission Products.Restore.
        /// 2. Terima id produk dari route.
        /// 3. Handler menjalankan proses restore produk.
        /// 4. Jika produk tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan productId.
        /// </remarks>
        /// <param name="id">Id produk yang akan direstore.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status restore produk.</returns>
        [HttpPost("{id}/restore")]
        [HasPermission(Permissions.Products.Restore)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RestoreProductSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(RestoreProductBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ProductNotFoundExample))]
        public async Task<IActionResult> Restore(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var restoreProductCommand = new RestoreProductCommand(id);

            var restoreProductResult = await _restoreProductHandler.Handle(restoreProductCommand, cancellationToken);

            if (!restoreProductResult.IsSuccess)
            {
                if (IsProductNotFound(restoreProductResult.Error))
                    return NotFoundResponse(restoreProductResult.Error!);

                return BadRequestResponse(restoreProductResult.Error ?? "Restore product failed");
            }

            return SuccessResponse(
                new { productId = restoreProductResult.Value },
                "Product restored success"
            );
        }
        #endregion

        #region Product Variant
        /// <summary>
        /// Menambah varian baru ke sebuah produk.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/products/{productId}/variants
        ///
        /// Flow add variant:
        /// 1. Endpoint membutuhkan permission Products.Create.
        /// 2. Terima productId dari route dan payload varian dari body.
        /// 3. Mapping ke AddProductVariantCommand.
        /// 4. Jika produk tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan variantId.
        /// </remarks>
        /// <param name="productId">Id produk target untuk varian baru.</param>
        /// <param name="addProductVariantRequest">Payload data varian baru.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status penambahan varian.</returns>
        [HttpPost("{productId}/variants")]
        [HasPermission(Permissions.Products.Create)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(AddProductVariantRequest), typeof(AddProductVariantRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddProductVariantSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(AddProductVariantBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ProductNotFoundExample))]
        public async Task<IActionResult> AddVariant(
            Guid productId,
            AddProductVariantRequest addProductVariantRequest,
            CancellationToken cancellationToken = default)
        {
            var addProductVariantCommand = new AddProductVariantCommand(
                productId,
                addProductVariantRequest.Sku,
                addProductVariantRequest.Stock,
                addProductVariantRequest.PriceAdjustment,
                addProductVariantRequest.Size,
                addProductVariantRequest.Color
            );

            var addProductResult = await _addProductVariantHandler.Handle(addProductVariantCommand, cancellationToken);

            if (!addProductResult.IsSuccess)
            {
                if (IsProductNotFound(addProductResult.Error))
                    return NotFoundResponse(addProductResult.Error!);

                return BadRequestResponse(addProductResult.Error ?? "Add variant failed");
            }

            return SuccessResponse(
                new { variantId = addProductResult.Value },
                "Variant added success"
            );
        }

        /// <summary>
        /// Memperbarui varian produk.
        /// </summary>
        /// <remarks>
        /// Endpoint: PATCH api/products/variants/{id}
        ///
        /// Flow update variant:
        /// 1. Endpoint membutuhkan permission Products.Update.
        /// 2. Terima id varian dari route dan payload perubahan dari body.
        /// 3. Mapping ke UpdateProductVariantCommand.
        /// 4. Jika varian/produk tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan variantId.
        /// </remarks>
        /// <param name="id">Id varian yang akan diperbarui.</param>
        /// <param name="request">Payload perubahan data varian.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status pembaruan varian.</returns>
        [HttpPatch("variants/{id}")]
        [HasPermission(Permissions.Products.Update)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(UpdateProductVariantRequest), typeof(UpdateProductVariantRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UpdateProductVariantSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(UpdateProductVariantBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(VariantNotFoundExample))]
        public async Task<IActionResult> UpdateVariant(
            Guid id,
            UpdateProductVariantRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateProductVariantCommand(
                id,
                request.Size,
                request.Color,
                request.PriceAdjustment,
                request.Sku,
                request.Stock
            );

            var result = await _updateProductVariantHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (IsVariantNotFound(result.Error) || IsProductNotFound(result.Error))
                    return NotFoundResponse(result.Error!);

                return BadRequestResponse(result.Error ?? "Update variant failed");
            }

            return SuccessResponse(
                new { variantId = result.Value },
                "Variant updated successfully"
            );
        }

        /// <summary>
        /// Menghapus varian produk (soft delete).
        /// </summary>
        /// <remarks>
        /// Endpoint: DELETE api/products/variants/{id}
        ///
        /// Flow remove variant:
        /// 1. Endpoint membutuhkan permission Products.Delete.
        /// 2. Terima id varian dari route.
        /// 3. Handler menjalankan proses soft delete varian.
        /// 4. Jika varian/produk tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan variantId.
        /// </remarks>
        /// <param name="id">Id varian yang akan dihapus.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status penghapusan varian.</returns>
        [HttpDelete("variants/{id}")]
        [HasPermission(Permissions.Products.Delete)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RemoveProductVariantSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(RemoveProductVariantBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(VariantNotFoundExample))]
        public async Task<IActionResult> RemoveVariant(
            Guid id,
            CancellationToken cancellationToken)
        {
            var command = new RemoveProductVariantCommand(id);
            var result = await _removeProductVariantHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (IsVariantNotFound(result.Error) || IsProductNotFound(result.Error))
                    return NotFoundResponse(result.Error!);

                return BadRequestResponse(result.Error ?? "Remove variant failed");
            }

            return SuccessResponse(
                new { variantId = result.Value },
                "Variant removed successfully"
            );
        }

        /// <summary>
        /// Me-restore varian produk yang sudah dihapus.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/products/variants/{id}/restore
        ///
        /// Flow restore variant:
        /// 1. Endpoint membutuhkan permission Products.Update.
        /// 2. Terima id varian dari route.
        /// 3. Handler menjalankan proses restore varian.
        /// 4. Jika varian/produk tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan variantId.
        /// </remarks>
        /// <param name="id">Id varian yang akan direstore.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status restore varian.</returns>
        [HttpPost("variants/{id}/restore")]
        [HasPermission(Permissions.Products.Update)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RestoreProductVariantSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(RestoreProductVariantBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(VariantNotFoundExample))]
        public async Task<IActionResult> RestoreVariant(
            Guid id,
            CancellationToken cancellationToken)
        {
            var command = new RestoreProductVariantCommand(id);
            var result = await _restoreProductVariantHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (IsVariantNotFound(result.Error) || IsProductNotFound(result.Error))
                    return NotFoundResponse(result.Error!);

                return BadRequestResponse(result.Error ?? "Restore variant failed");
            }

            return SuccessResponse(
                new { variantId = result.Value },
                "Variant restored successfully"
            );
        }
        #endregion

        #region Product Image
        /// <summary>
        /// Menambah gambar ke sebuah produk.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/products/{productId}/images
        ///
        /// Flow add image:
        /// 1. Endpoint membutuhkan permission Products.Create.
        /// 2. Terima productId dari route dan payload gambar dari body.
        /// 3. Mapping ke AddProductImageCommand.
        /// 4. Jika produk tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan imageId.
        /// </remarks>
        /// <param name="productId">Id produk target untuk gambar baru.</param>
        /// <param name="addProductImageRequest">Payload data gambar baru.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status penambahan gambar.</returns>
        [HttpPost("{productId}/images")]
        [HasPermission(Permissions.Products.Create)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(AddProductImageRequest), typeof(AddProductImageRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddProductImageSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(AddProductImageBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ProductNotFoundExample))]
        public async Task<IActionResult> AddImage(
            Guid productId,
            AddProductImageRequest addProductImageRequest,
            CancellationToken cancellationToken = default)
        {
            var addProductImageCommand = new AddProductImageCommand(
                productId,
                addProductImageRequest.ImageUrl,
                addProductImageRequest.IsPrimary,
                addProductImageRequest.DisplayOrder
            );

            var addProductImageResult = await _addProductImageHandler.Handle(addProductImageCommand, cancellationToken);

            if (!addProductImageResult.IsSuccess)
            {
                if (IsProductNotFound(addProductImageResult.Error))
                    return NotFoundResponse(addProductImageResult.Error!);

                return BadRequestResponse(addProductImageResult.Error ?? "Add image failed");
            }

            return SuccessResponse(
                new { imageId = addProductImageResult.Value },
                "Image added success"
            );
        }

        /// <summary>
        /// Memperbarui gambar produk.
        /// </summary>
        /// <remarks>
        /// Endpoint: PATCH api/products/images/{id}
        ///
        /// Flow update image:
        /// 1. Endpoint membutuhkan permission Products.Update.
        /// 2. Terima id gambar dari route dan payload perubahan dari body.
        /// 3. Mapping ke UpdateProductImageCommand.
        /// 4. Jika image/produk tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan imageId.
        /// </remarks>
        /// <param name="id">Id gambar yang akan diperbarui.</param>
        /// <param name="request">Payload perubahan data gambar.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status pembaruan gambar.</returns>
        [HttpPatch("images/{id}")]
        [HasPermission(Permissions.Products.Update)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerRequestExample(typeof(UpdateProductImageRequest), typeof(UpdateProductImageRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UpdateProductImageSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(UpdateProductImageBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ImageNotFoundExample))]
        public async Task<IActionResult> UpdateImage(
            Guid id,
            UpdateProductImageRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateProductImageCommand(
                id,
                request.ImageUrl,
                request.IsPrimary,
                request.DisplayOrder
            );

            var result = await _updateProductImageHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (IsImageNotFound(result.Error) || IsProductNotFound(result.Error))
                    return NotFoundResponse(result.Error!);

                return BadRequestResponse(result.Error ?? "Update image failed");
            }

            return SuccessResponse(
                new { imageId = result.Value },
                "Image updated successfully"
            );
        }

        /// <summary>
        /// Menghapus gambar produk (soft delete).
        /// </summary>
        /// <remarks>
        /// Endpoint: DELETE api/products/images/{id}
        ///
        /// Flow remove image:
        /// 1. Endpoint membutuhkan permission Products.Delete.
        /// 2. Terima id gambar dari route.
        /// 3. Handler menjalankan proses soft delete gambar.
        /// 4. Jika image/produk tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan imageId.
        /// </remarks>
        /// <param name="id">Id gambar yang akan dihapus.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status penghapusan gambar.</returns>
        [HttpDelete("images/{id}")]
        [HasPermission(Permissions.Products.Delete)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RemoveProductImageSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(RemoveProductImageBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ImageNotFoundExample))]
        public async Task<IActionResult> RemoveImage(
            Guid id,
            CancellationToken cancellationToken)
        {
            var command = new RemoveProductImageCommand(id);
            var result = await _removeProductImageHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (IsImageNotFound(result.Error) || IsProductNotFound(result.Error))
                    return NotFoundResponse(result.Error!);

                return BadRequestResponse(result.Error ?? "Remove image failed");
            }

            return SuccessResponse(
                new { imageId = result.Value },
                "Image removed successfully"
            );
        }

        /// <summary>
        /// Me-restore gambar produk yang sudah dihapus.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/products/images/{id}/restore
        ///
        /// Flow restore image:
        /// 1. Endpoint membutuhkan permission Products.Update.
        /// 2. Terima id gambar dari route.
        /// 3. Handler menjalankan proses restore gambar.
        /// 4. Jika image tidak ditemukan, kembalikan HTTP 404.
        /// 5. Jika gagal validasi/proses, kembalikan HTTP 400.
        /// 6. Jika berhasil, kembalikan imageId.
        /// </remarks>
        /// <param name="id">Id gambar yang akan direstore.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status restore gambar.</returns>
        [HttpPost("images/{id}/restore")]
        [HasPermission(Permissions.Products.Update)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RestoreProductImageSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(RestoreProductImageBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ImageNotFoundExample))]
        public async Task<IActionResult> RestoreImage(
            Guid id,
            CancellationToken cancellationToken)
        {
            var command = new RestoreProductImageCommand(id);
            var result = await _restoreProductImageHandler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (IsImageNotFound(result.Error))
                    return NotFoundResponse(result.Error!);

                return BadRequestResponse(result.Error ?? "Restore image failed");
            }

            return SuccessResponse(
                new { imageId = result.Value },
                "Image restored successfully"
            );
        }
        #endregion

        #region Helper Methods
        private static bool IsProductNotFound(string? error)
            => string.Equals(error, "Product not found", StringComparison.OrdinalIgnoreCase);

        private static bool IsVariantNotFound(string? error)
            => string.Equals(error, "Variant not found", StringComparison.OrdinalIgnoreCase);

        private static bool IsImageNotFound(string? error)
            => string.Equals(error, "Image not found", StringComparison.OrdinalIgnoreCase);
        #endregion
    }
}
