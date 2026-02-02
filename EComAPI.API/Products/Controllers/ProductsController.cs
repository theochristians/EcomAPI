using EComAPI.API.Auth.Swagger.Examples;
using EComAPI.API.Authorization;
using EComAPI.API.Common;
using EComAPI.API.Products.Dtos.Requests;
using EComAPI.API.Products.Dtos.Responses;
using EComAPI.API.Products.Swagger.Examples.AddImage.Request;
using EComAPI.API.Products.Swagger.Examples.AddImage.Response;
using EComAPI.API.Products.Swagger.Examples.AddVariant.Request;
using EComAPI.API.Products.Swagger.Examples.AddVariant.Response;
using EComAPI.API.Products.Swagger.Examples.CreateProduct.Request;
using EComAPI.API.Products.Swagger.Examples.CreateProduct.Response;
using EComAPI.API.Products.Swagger.Examples.GetAllProducts.Response;
using EComAPI.API.Products.Swagger.Examples.GetProductBySlug.Response;
using EComAPI.Application.Common.Authorization;
using EComAPI.Application.Products.Commands.AddProductImage;
using EComAPI.Application.Products.Commands.AddProductVariant;
using EComAPI.Application.Products.Commands.CreateProduct;
using EComAPI.Application.Products.Commands.DeleteProduct;
using EComAPI.Application.Products.Commands.RestoreProduct;
using EComAPI.Application.Products.Queries.GetProductBySlug;
using EComAPI.Application.Products.Queries.GetProducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Threading;

namespace EComAPI.API.Products.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : BaseController
    {
        private readonly CreateProductHandler _createProductHandler;
        private readonly AddProductVariantHandler _addProductVariantHandler;
        private readonly AddProductImageHandler _addProductImageHandler;
        private readonly GetProductsHandler _getProductsHandler;
        private readonly GetProductBySlugHandler _getProductBySlugHandler;
        private readonly DeleteProductHandler _deleteProductHandler;
        private readonly RestoreProductHandler _restoreProductHandler;

        public ProductsController(
            CreateProductHandler createProductHandler,
            AddProductVariantHandler addProductVariantHandler,
            AddProductImageHandler addProductImageHandler,
            GetProductsHandler getProductsHandler,
            GetProductBySlugHandler getProductBySlugHandler,
            DeleteProductHandler deleteProductHandler,
            RestoreProductHandler restoreProductHandler)
        {
            _createProductHandler = createProductHandler;
            _addProductVariantHandler = addProductVariantHandler;
            _addProductImageHandler = addProductImageHandler;
            _getProductsHandler = getProductsHandler;
            _getProductBySlugHandler = getProductBySlugHandler;
            _deleteProductHandler = deleteProductHandler;
            _restoreProductHandler = restoreProductHandler;
        }

        /// <summary>
        /// Add new product
        /// </summary>
        /// <remarks>
        ///     POST api/products/createProduct
        /// </remarks>
        /// <response code="200">Product created success</response>
        /// <response code="400">Product create failed</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Route("createProduct")]
        [HasPermission(Permissions.Products.Create)]
        // Response
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        // SwaggerExample
        [SwaggerRequestExample(typeof(CreateProductRequest), typeof(CreateProductRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CreateProductSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(CreateProductBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> Create(
            CreateProductRequest createProductRequest,
            CancellationToken cancellationToken)
        {
            var createProductCommand = new CreateProductCommand(
                createProductRequest.CategoryId,
                createProductRequest.Name,
                createProductRequest.Slug,
                createProductRequest.BasePrice,
                createProductRequest.Description
            );

            var createProductResult = await _createProductHandler.Handle(createProductCommand, cancellationToken);

            if (!createProductResult.IsSuccess)
                return BadRequestResponse("Product create failed");

            return SuccesResponse<Guid>(createProductResult.Value!, "Product created success");
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <remarks>
        ///     GET api/products/getAllProducts
        /// </remarks>
        /// <response code="200">Success list product</response>
        /// <response code="400">Failed list product</response>
        [HttpGet]
        [Route("getAllProducts")]
        [HasPermission(Permissions.Products.Read)]
        // Response
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        // SwaggerExample
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetAllProductsSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GetAllProductsBadRequestExample))]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var getProductResult = await _getProductsHandler.Handle(
                new GetProductsQuery(),
                cancellationToken
            );

            var getResponseList = getProductResult.Value!
                .Select(productListResponse => new ProductListResponse(
                    productListResponse.Id,
                    productListResponse.Name,
                    productListResponse.Slug,
                    productListResponse.BasePrice,
                    productListResponse.ViewCount
                ));
            return SuccesResponse<IEnumerable<ProductListResponse>>(getResponseList, "Success list product");
        }

        /// <summary>
        /// Get product detail by slug
        /// </summary>
        /// <remarks>
        ///     GET api/products/{slug}
        /// </remarks>
        /// <response code="200">Product detail success</response>
        /// <response code="404">Product not found</response>
        [HttpGet]
        [Route("{slug}")]
        [HasPermission(Permissions.Products.Read)]
        // Response
        [ProducesResponseType(typeof(ApiResponse<ProductDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        // SwaggerExample
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetProductBySlugSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GetProductBySlugNotFoundExample))]
        public async Task<IActionResult> GetBySlug(
            string slug,
            CancellationToken cancellationToken)
        {
            var getProductResult = await _getProductBySlugHandler.Handle(
                new GetProductBySlugQuery(slug),
                cancellationToken
            );

            if (!getProductResult.IsSuccess)
                return NotFound(getProductResult.Error);

            var productDetailSlugDto = getProductResult.Value!;

            var productDetailResponse = new ProductDetailResponse(
                productDetailSlugDto.Id,
                productDetailSlugDto.Name,
                productDetailSlugDto.Slug,
                productDetailSlugDto.BasePrice,
                productDetailSlugDto.Description,
                productDetailSlugDto.Variants.Select(productVariantDetailDto =>
                    new ProductVariantResponse(
                        productVariantDetailDto.Id,
                        productVariantDetailDto.Sku,
                        productVariantDetailDto.Stock,
                        productVariantDetailDto.PriceAdjustment,
                        productVariantDetailDto.Size,
                        productVariantDetailDto.Color
                    )).ToList(),
                productDetailSlugDto.Images.Select(productImageDetailDto =>
                    new ProductImageResponse(
                        productImageDetailDto.Id,
                        productImageDetailDto.ImageUrl,
                        productImageDetailDto.IsPrimary,
                        productImageDetailDto.DisplayOrder
                    )).ToList()
            );

            if (!getProductResult.IsSuccess)
                return BadRequestResponse("Product detail failed");
            return SuccesResponse<ProductDetailResponse>(productDetailResponse, "Product detail success");
        }

        /// <summary>
        /// Add variant to product
        /// </summary>
        /// <remarks>
        ///     POST api/products/{id}/variants
        /// </remarks>
        /// <response code="200">Product variant added success</response>
        /// <response code="400">Product variant failed</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Route("{id}/variants")]
        [HasPermission(Permissions.Products.Create)]
        // Response
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        // SwaggerExample
        [SwaggerRequestExample(typeof(AddProductVariantRequest), typeof(AddVariantRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddVariantSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(AddVariantBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> AddVariant(
            Guid id,
            AddProductVariantRequest addProductVariantRequest,
            CancellationToken cancellationToken)
        {
            var addProductVariantCommand = new AddProductVariantCommand(
                id,
                addProductVariantRequest.Sku,
                addProductVariantRequest.Stock,
                addProductVariantRequest.PriceAdjustment,
                addProductVariantRequest.Size,
                addProductVariantRequest.Color
            );

            var addProductResult = await _addProductVariantHandler.Handle(addProductVariantCommand, cancellationToken);

            if (!addProductResult.IsSuccess)
                return BadRequestResponse("Product variant failed add");

            return SuccesResponse<object>(null, "Product Variant success added");
        }

        /// <summary>
        /// Add image to product
        /// </summary>
        /// <remarks>
        ///     POST api/products/{id}/images
        /// </remarks>
        /// <response code="200">Image added success</response>
        /// <response code="400">Image add failed</response>
        /// <response code="401">Unauthorized</response>
        // Response
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        // SwaggerExample
        [SwaggerRequestExample(typeof(AddProductImageRequest), typeof(AddImageRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddImageSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(AddImageBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        [HttpPost]
        [Route("{id}/images")]
        [HasPermission(Permissions.Products.Create)]
        public async Task<IActionResult> AddImage(
            Guid id,
            AddProductImageRequest addProductImageRequest,
            CancellationToken cancellationToken)
        {
            var addProductImageCommand = new AddProductImageCommand(
                id,
                addProductImageRequest.ImageUrl,
                addProductImageRequest.IsPrimary,
                addProductImageRequest.DisplayOrder
            );

            var addImageResult = await _addProductImageHandler.Handle(addProductImageCommand, cancellationToken);

            if (!addImageResult.IsSuccess)
                return BadRequestResponse("Failed Image added");

            return SuccesResponse<object>(null, "Success Image added");
        }

        [HttpDelete("{id:guid}")]
        [HasPermission(Permissions.Products.Delete)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _deleteProductHandler.Handle(
                new DeleteProductCommand(id),
                ct);

            if (!result.IsSuccess)
                return BadRequestResponse(result.Error!);

            return SuccesResponse(
                new { id = result.Value },
                "Product deleted successfully");
        }

        [HttpPut("{id:guid}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
        {
            var result = await _restoreProductHandler.Handle(
                new RestoreProductCommand(id),
                ct);

            if (!result.IsSuccess)
                return BadRequestResponse(result.Error!);

            return SuccesResponse(
                new { id = result.Value },
                "Product restored successfully");
        }

    }
}
