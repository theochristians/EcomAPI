using EComAPI.Application.Categories.DTOs;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Results;

namespace EComAPI.Application.Categories.Queries.GetCategories
{
    public class GetCategoriesHandler
    {
        private readonly ICategoryRepository _repository;

        public GetCategoriesHandler(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
            GetCategoriesQuery query,
            CancellationToken cancellationToken)
        {
            var categories = await _repository
                .GetAllAsync(cancellationToken);

            var result = categories
                .Select(CategoryDto.From)
                .ToList();

            return Result<IReadOnlyList<CategoryDto>>.Success(result);
        }
    }
}