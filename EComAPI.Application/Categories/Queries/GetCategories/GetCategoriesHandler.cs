using EComAPI.Application.Categories.DTOs;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Results;

namespace EComAPI.Application.Categories.Queries.GetCategories
{
    public class GetCategoriesHandler
    {
        private readonly ICategoriesRepository _categoriesRepository;

        public GetCategoriesHandler(ICategoriesRepository categoriesRepository)
        {
            _categoriesRepository = categoriesRepository;
        }

        public async Task<Result<IReadOnlyList<CategoriesDTOs>>> Handle(
            GetCategoriesQuery getCategoriesQuery,
            CancellationToken cancellationToken = default)
        {
            var getCategoriesAllAsync = await _categoriesRepository.GetAllCategoriesAsync(cancellationToken);

            var categoryIds = getCategoriesAllAsync.Select(category => category.Id).ToList();
            var productCounts = await _categoriesRepository.GetProductCountsAsync(
                categoryIds,
                cancellationToken);

            var categoriesListDTOsResult = getCategoriesAllAsync
                .Select(category => CategoriesDTOs.From(
                    category,
                    productCounts.GetValueOrDefault(category.Id, 0)))
                .ToList();

            return Result<IReadOnlyList<CategoriesDTOs>>.Success(categoriesListDTOsResult);
        }
    }
}