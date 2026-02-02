using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Results;

namespace EComAPI.Application.Categories.Commands.RestoreCategory
{
    public class RestoreCategoryHandler
    {
        private readonly ICategoryRepository _categoryRepository;

        public RestoreCategoryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<Guid>> Handle(
            RestoreCategoryCommand restoreCategoryCommand,
            CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdIncludeDeletedAsync(restoreCategoryCommand.Id, cancellationToken);

            if (category == null)
                return Result<Guid>.Failure("Category not found");

            if (!category.IsDeleted)
                return Result<Guid>.Failure("Category is not deleted");

            category.Restore();

            await _categoryRepository.UpdateAsync(category, cancellationToken);

            return Result<Guid>.Success(category.Id);
        }
    }
}