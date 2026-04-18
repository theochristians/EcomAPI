using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Constants;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.DTOs;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Queries.GetProductBySlug
{
    public class GetProductBySlugHandler
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(3);

        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoriesRepository;
        private readonly ICacheService _cacheService;

        public GetProductBySlugHandler(
            IProductRepository productRepository,
            ICategoryRepository categoriesRepository,
            ICacheService cacheService)
        {
            _productRepository = productRepository;
            _categoriesRepository = categoriesRepository;
            _cacheService = cacheService;
        }

        public async Task<Result<ProductDetailDto>> Handle(
            GetProductBySlugQuery getProductBySlugQuery,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(getProductBySlugQuery.Slug))
                    return Result<ProductDetailDto>.Failure("Slug is required");

                var normalizedSlug = getProductBySlugQuery.Slug.Trim().ToLowerInvariant();
                var namespaceVersions = await _cacheService.GetNamespaceVersionsAsync(
                    new[] { CacheKeys.CategoriesNamespace, CacheKeys.ProductsNamespace },
                    cancellationToken);

                var categoriesVersion = namespaceVersions.GetValueOrDefault(CacheKeys.CategoriesNamespace, 1);
                var productsVersion = namespaceVersions.GetValueOrDefault(CacheKeys.ProductsNamespace, 1);
                var cacheKey = $"query:products:v{productsVersion}:cv{categoriesVersion}:slug:{normalizedSlug}";

                var cachedProduct = await _cacheService.GetAsync<ProductDetailDto>(cacheKey, cancellationToken);
                if (cachedProduct != null)
                    return Result<ProductDetailDto>.Success(cachedProduct);

                var productBySlug = await _productRepository.GetProductBySlugAsync(getProductBySlugQuery.Slug, cancellationToken);

                if (productBySlug is null)
                    return Result<ProductDetailDto>.Failure("Product not found");

                var categoryHierarchy = await BuildCategoryHierarchy(productBySlug.CategoryId, cancellationToken);

                var productDetailDto = new ProductDetailDto(
                    Id: productBySlug.Id,
                    Name: productBySlug.Name,
                    Slug: productBySlug.Slug,
                    BasePrice: productBySlug.BasePrice,
                    Description: productBySlug.Description,
                    ViewCount: productBySlug.ViewCount,
                    TotalStock: productBySlug.TotalStock,
                    IsActive: productBySlug.IsActive,
                    Category: categoryHierarchy,
                    Variants: productBySlug.Variants.Select(v => new ProductVariantDto(
                        Id: v.Id,
                        Sku: v.Sku,
                        Stock: v.Stock,
                        PriceAdjustment: v.PriceAdjustment ?? 0,
                        FinalPrice: productBySlug.BasePrice + (v.PriceAdjustment ?? 0),
                        Size: v.Size,
                        Color: v.Color,
                        IsActive: v.IsActive
                    )).ToList(),
                    Images: productBySlug.Images.Select(i => new ProductImageDto(
                        Id: i.Id,
                        ImageUrl: i.ImageUrl,
                        IsPrimary: i.IsPrimary,
                        DisplayOrder: i.DisplayOrder
                    )).ToList()
                );

                await _cacheService.SetAsync(cacheKey, productDetailDto, CacheTtl, cancellationToken);

                return Result<ProductDetailDto>.Success(productDetailDto);
            }
            catch (DomainException domainException)
            {
                return Result<ProductDetailDto>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<ProductDetailDto>.Failure($"Failed to get product: {exception.Message}");
            }
        }

        private async Task<CategoryHierarchyDto?> BuildCategoryHierarchy(
            Guid categoryId,
            CancellationToken cancellationToken)
        {
            var categoryById = await _categoriesRepository.GetCategoryByIdAsync(categoryId, cancellationToken);

            if (categoryById == null)
                return null;

            CategoryHierarchyDto? parentCategory = null;
            if (categoryById.ParentId.HasValue)
            {
                parentCategory = await BuildCategoryHierarchy(categoryById.ParentId.Value, cancellationToken);
            }

            return new CategoryHierarchyDto(
                Id: categoryById.Id,
                Name: categoryById.Name,
                Slug: categoryById.Slug,
                ImageUrl: categoryById.ImageUrl,
                Description: categoryById.Description,
                Parent: parentCategory
            );
        }
    }
}
