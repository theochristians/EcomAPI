using EComAPI.Application.Categories.DTOs;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Constants;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Categories.Queries.GetCategoriesBySlug
{
    public class GetCategoryBySlugHandler
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        private readonly ICategoryRepository _categoryRepository;
        private readonly ICacheService _cacheService;

        public GetCategoryBySlugHandler(
            ICategoryRepository categoryRepository,
            ICacheService cacheService)
        {
            _categoryRepository = categoryRepository;
            _cacheService = cacheService;
        }

        public async Task<Result<CategoryDto>> Handle(
            GetCategoryBySlugQuery getCategoriesBySlugQuery,
            CancellationToken cancellationToken = default)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(getCategoriesBySlugQuery.Slug))
                    return Result<CategoryDto>.Failure("Slug is required");

                var normalizedSlug = getCategoriesBySlugQuery.Slug.Trim().ToLowerInvariant();
                var namespaceVersions = await _cacheService.GetNamespaceVersionsAsync(
                    new[] { CacheKeys.CategoriesNamespace, CacheKeys.ProductsNamespace },
                    cancellationToken);

                var categoriesVersion = namespaceVersions.GetValueOrDefault(CacheKeys.CategoriesNamespace, 1);
                var productsVersion = namespaceVersions.GetValueOrDefault(CacheKeys.ProductsNamespace, 1);
                var cacheKey = $"query:categories:v{categoriesVersion}:pv{productsVersion}:slug:{normalizedSlug}";

                var cachedCategory = await _cacheService.GetAsync<CategoryDto>(cacheKey, cancellationToken);
                if (cachedCategory != null)
                    return Result<CategoryDto>.Success(cachedCategory);

                var getCategoryBySlugAsync = await _categoryRepository.GetCategoryBySlugAsync(
                    getCategoriesBySlugQuery.Slug,
                    cancellationToken);

                if (getCategoryBySlugAsync is null)
                    return Result<CategoryDto>.Failure("Category not found");

                var getProductCountAsync = await _categoryRepository.GetProductCountAsync(
                    getCategoryBySlugAsync.Id,
                    cancellationToken);

                var categoriesDTOsResult = new CategoryDto(
                    getCategoryBySlugAsync.Id,
                    getCategoryBySlugAsync.Name,
                    getCategoryBySlugAsync.Slug,
                    getCategoryBySlugAsync.ParentId,
                    getCategoryBySlugAsync.ImageUrl,
                    getCategoryBySlugAsync.Description,
                    getProductCountAsync,
                    getCategoryBySlugAsync.CreatedAt,
                    getCategoryBySlugAsync.CreatedBy,
                    getCategoryBySlugAsync.UpdatedAt,
                    getCategoryBySlugAsync.UpdatedBy);

                await _cacheService.SetAsync(cacheKey, categoriesDTOsResult, CacheTtl, cancellationToken);

                return Result<CategoryDto>.Success(categoriesDTOsResult);
            }
            catch (DomainException domainException)
            {
                return Result<CategoryDto>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<CategoryDto>.Failure($"Failed to get category by slug: {exception.Message}");
            }
        }
    }
}
