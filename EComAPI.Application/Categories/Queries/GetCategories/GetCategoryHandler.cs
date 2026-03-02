using EComAPI.Application.Categories.DTOs;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Categories.Queries.GetCategories
{
    public class GetCategoryHandler
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetCategoryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var getCategoryAllAsync = await _categoryRepository.GetAllCategoriesAsync(cancellationToken);

                var categoryIdsAsync = getCategoryAllAsync.Select(category => category.Id).ToList();
                var getProductCountsAsync = await _categoryRepository.GetProductCountsAsync(
                    categoryIdsAsync,
                    cancellationToken);

                var categoriesListDTOsResult = getCategoryAllAsync
                    .Select(category => new CategoryDto(
                        category.Id,
                        category.Name,
                        category.Slug,
                        category.ParentId,
                        category.ImageUrl,
                        category.Description,
                        getProductCountsAsync.GetValueOrDefault(category.Id, 0),
                        category.CreatedAt,
                        category.CreatedBy,
                        category.UpdatedAt,
                        category.UpdatedBy))
                    .ToList();

                return Result<IReadOnlyList<CategoryDto>>.Success(categoriesListDTOsResult);
            }
            catch (DomainException domainException)
            {
                return Result<IReadOnlyList<CategoryDto>>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<IReadOnlyList<CategoryDto>>.Failure($"Failed to get categories: {exception.Message}");
            }
        }
    }
}