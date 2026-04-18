using EComAPI.Application.Categories.DTOs;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Constants;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Result;

namespace EComAPI.Application.Categories.Queries.GetCategories
{
    public class GetCategoriesHandler
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        private readonly ICategoryRepository _categoriesRepository;
        private readonly ICacheService _cacheService;

        public GetCategoriesHandler(
            ICategoryRepository categoriesRepository,
            ICacheService cacheService)
        {
            _categoriesRepository = categoriesRepository;
            _cacheService = cacheService;
        }

        public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
            GetCategoriesQuery getCategoriesQuery,
            CancellationToken cancellationToken = default)
        {
            var namespaceVersions = await _cacheService.GetNamespaceVersionsAsync(
                new[] { CacheKeys.CategoriesNamespace, CacheKeys.ProductsNamespace },
                cancellationToken);

            var categoriesVersion = namespaceVersions.GetValueOrDefault(CacheKeys.CategoriesNamespace, 1);
            var productsVersion = namespaceVersions.GetValueOrDefault(CacheKeys.ProductsNamespace, 1);
            var cacheKey = $"query:categories:v{categoriesVersion}:pv{productsVersion}:all";

            var cachedCategories = await _cacheService.GetAsync<List<CategoryDto>>(cacheKey, cancellationToken);
            if (cachedCategories != null)
                return Result<IReadOnlyList<CategoryDto>>.Success(cachedCategories);

            var categories = await _categoriesRepository.GetAllCategoriesAsync(cancellationToken);

            var categoryIds = categories.Select(category => category.Id).ToList();
            var productCounts = await _categoriesRepository.GetProductCountsAsync(
                categoryIds,
                cancellationToken);

            var categoryDtos = categories
                .Select(category => new CategoryDto(
                    category.Id,
                    category.Name,
                    category.Slug,
                    category.ParentId,
                    category.ImageUrl,
                    category.Description,
                    productCounts.GetValueOrDefault(category.Id, 0),
                    category.CreatedAt,
                    category.CreatedBy,
                    category.UpdatedAt,
                    category.UpdatedBy))
                .ToList();

            await _cacheService.SetAsync(cacheKey, categoryDtos, CacheTtl, cancellationToken);

            return Result<IReadOnlyList<CategoryDto>>.Success(categoryDtos);
        }
    }
}
