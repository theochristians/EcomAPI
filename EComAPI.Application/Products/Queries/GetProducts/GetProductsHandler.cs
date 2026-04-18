using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Constants;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.DTOs;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Categories.Entities;
using EComAPI.Domain.Common.Exceptions;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace EComAPI.Application.Products.Queries.GetProducts
{
    public class GetProductsHandler
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(3);

        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoriesRepository;
        private readonly ICacheService _cacheService;

        private sealed record ProductsQueryCacheItem(IReadOnlyList<ProductListDto> Items, int TotalCount, int TotalPages);

        public GetProductsHandler(
            IProductRepository productRepository,
            ICategoryRepository categoriesRepository,
            ICacheService cacheService)
        {
            _productRepository = productRepository;
            _categoriesRepository = categoriesRepository;
            _cacheService = cacheService;
        }

        public async Task<Result<(IReadOnlyList<ProductListDto> Items, int TotalCount, int TotalPages)>> Handle(
            GetProductsQuery getProductsQuery,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (getProductsQuery.Page < 1)
                    return Result<(IReadOnlyList<ProductListDto>, int, int)>.Failure("Page must be greater than 0");

                if (getProductsQuery.PageSize < 1)
                    return Result<(IReadOnlyList<ProductListDto>, int, int)>.Failure("Page size must be greater than 0");

                var namespaceVersions = await _cacheService.GetNamespaceVersionsAsync(
                    new[] { CacheKeys.CategoriesNamespace, CacheKeys.ProductsNamespace },
                    cancellationToken);

                var categoriesVersion = namespaceVersions.GetValueOrDefault(CacheKeys.CategoriesNamespace, 1);
                var productsVersion = namespaceVersions.GetValueOrDefault(CacheKeys.ProductsNamespace, 1);
                var cacheKey = BuildProductsQueryCacheKey(getProductsQuery, categoriesVersion, productsVersion);

                var cachedResult = await _cacheService.GetAsync<ProductsQueryCacheItem>(cacheKey, cancellationToken);
                if (cachedResult != null)
                {
                    return Result<(IReadOnlyList<ProductListDto>, int, int)>.Success(
                        (cachedResult.Items, cachedResult.TotalCount, cachedResult.TotalPages));
                }

                List<Guid>? getCategoryIdsForFilter = null;

                if (!string.IsNullOrWhiteSpace(getProductsQuery.CategorySlug))
                {
                    getCategoryIdsForFilter = await GetCategoryIdsForFilter(
                        getProductsQuery.CategorySlug,
                        getProductsQuery.IncludeSubcategories,
                        cancellationToken);

                    if (getCategoryIdsForFilter.Count == 0)
                    {
                        var emptyResult = new ProductsQueryCacheItem(new List<ProductListDto>(), 0, 0);
                        await _cacheService.SetAsync(cacheKey, emptyResult, CacheTtl, cancellationToken);

                        return Result<(IReadOnlyList<ProductListDto>, int, int)>.Success(
                            (emptyResult.Items, emptyResult.TotalCount, emptyResult.TotalPages));
                    }
                }

                var (productListDTO, totalCount) = await _productRepository.GetProductsPaginatedAsync(
                    getCategoryIdsForFilter,
                    getProductsQuery.MinPrice,
                    getProductsQuery.MaxPrice,
                    getProductsQuery.InStock,
                    getProductsQuery.IncludeInactive ? null : true,
                    getProductsQuery.SearchTerm,
                    getProductsQuery.SortBy,
                    getProductsQuery.SortOrder,
                    getProductsQuery.Page,
                    getProductsQuery.PageSize,
                    cancellationToken);

                var totalPages = (int)Math.Ceiling(totalCount / (double)getProductsQuery.PageSize);
                var productDtos = new List<ProductListDto>();

                foreach (var product in productListDTO)
                {
                    var categoryHierarchy = await BuildCategoryHierarchy(
                        product.CategoryId,
                        cancellationToken);

                    productDtos.Add(new ProductListDto(
                        product.Id,
                        product.Name,
                        product.Slug,
                        product.BasePrice,
                        product.ViewCount,
                        product.IsActive,
                        product.TotalStock > 0,
                        product.TotalStock,
                        categoryHierarchy
                    ));
                }

                var queryResult = new ProductsQueryCacheItem(productDtos, totalCount, totalPages);
                await _cacheService.SetAsync(cacheKey, queryResult, CacheTtl, cancellationToken);

                return Result<(IReadOnlyList<ProductListDto>, int, int)>.Success(
                    (queryResult.Items, queryResult.TotalCount, queryResult.TotalPages));
            }
            catch (DomainException domainException)
            {
                return Result<(IReadOnlyList<ProductListDto>, int, int)>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<(IReadOnlyList<ProductListDto>, int, int)>.Failure($"Failed to get products: {exception.Message}");
            }
        }

        private static string BuildProductsQueryCacheKey(GetProductsQuery query, long categoriesVersion, long productsVersion)
        {
            var normalized = string.Join(
                "|",
                query.CategorySlug?.Trim().ToLowerInvariant() ?? string.Empty,
                query.IncludeSubcategories,
                query.MinPrice?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                query.MaxPrice?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                query.InStock?.ToString() ?? string.Empty,
                query.IncludeInactive,
                query.SearchTerm?.Trim().ToLowerInvariant() ?? string.Empty,
                query.SortBy.Trim().ToLowerInvariant(),
                query.SortOrder.Trim().ToLowerInvariant(),
                query.Page,
                query.PageSize);

            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));

            return $"query:products:v{productsVersion}:cv{categoriesVersion}:list:{hash}";
        }

        private async Task<List<Guid>> GetCategoryIdsForFilter(
            string categorySlug,
            bool includeSubcategories,
            CancellationToken cancellationToken)
        {
            var categoryIds = new List<Guid>();
            var allCategories = await _categoriesRepository.GetAllCategoriesAsync(cancellationToken);

            var targetCategory = allCategories.FirstOrDefault(c =>
                c.Slug.Equals(categorySlug, StringComparison.OrdinalIgnoreCase));

            if (targetCategory == null)
                return categoryIds;

            categoryIds.Add(targetCategory.Id);

            if (includeSubcategories)
            {
                var childIds = GetAllChildCategoryIds(targetCategory.Id, allCategories);
                categoryIds.AddRange(childIds);
            }

            return categoryIds;
        }

        private List<Guid> GetAllChildCategoryIds(
            Guid parentId,
            IReadOnlyList<Category> getAllCategories)
        {
            var childIds = new List<Guid>();
            var children = getAllCategories.Where(category => category.ParentId == parentId).ToList();

            foreach (var child in children)
            {
                childIds.Add(child.Id);
                var grandchildIds = GetAllChildCategoryIds(child.Id, getAllCategories);
                childIds.AddRange(grandchildIds);
            }
            return childIds;
        }

        private async Task<CategoryHierarchyDto?> BuildCategoryHierarchy(
            Guid categoryId,
            CancellationToken cancellationToken)
        {
            var categoryById = await _categoriesRepository.GetCategoryByIdAsync(categoryId, cancellationToken);

            if (categoryById == null)
                return null;

            CategoryHierarchyDto? parent = null;
            if (categoryById.ParentId.HasValue)
            {
                parent = await BuildCategoryHierarchy(
                    categoryById.ParentId.Value,
                    cancellationToken);
            }
            return new CategoryHierarchyDto(
                Id: categoryById.Id,
                Name: categoryById.Name,
                Slug: categoryById.Slug,
                ImageUrl: categoryById.ImageUrl,
                Description: categoryById.Description,
                Parent: parent
            );
        }
    }
}
