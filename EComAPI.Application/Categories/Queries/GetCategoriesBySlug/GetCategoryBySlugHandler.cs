using EComAPI.Application.Categories.DTOs;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Categories.Queries.GetCategoriesBySlug
{
    public class GetCategoryBySlugHandler
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetCategoryBySlugHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<CategoryDto>> Handle(
            GetCategoryBySlugQuery getCategoriesBySlugQuery,
            CancellationToken cancellationToken = default)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(getCategoriesBySlugQuery.Slug))
                    return Result<CategoryDto>.Failure("Slug is required");

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