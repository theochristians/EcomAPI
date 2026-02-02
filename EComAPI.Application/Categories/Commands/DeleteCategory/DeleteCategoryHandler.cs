using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Results;

namespace EComAPI.Application.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryHandler
    {
        private readonly ICategoryRepository _repository;

        public DeleteCategoryHandler(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Guid>> Handle(
            DeleteCategoryCommand command,
            CancellationToken cancellationToken)
        {
            var category = await _repository.GetByIdAsync(command.Id, cancellationToken);

            if (category == null)
                return Result<Guid>.Failure("Category not found");

            if (category.IsDeleted)
                return Result<Guid>.Failure("Category already deleted");

            category.SoftDelete(null);

            await _repository.UpdateAsync(category, cancellationToken);

            return Result<Guid>.Success(category.Id);
        }
    }
}