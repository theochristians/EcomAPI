using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Results;
using EComAPI.Domain.Categories.Entities;

namespace EComAPI.Application.Categories.Commands.CreateCategory
{
    public class CreateCategoryHandler
    {
        private readonly ICategoryRepository _repository;

        public CreateCategoryHandler(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Guid>> Handle(
            CreateCategoryCommand command,
            CancellationToken cancellationToken)
        {
            var category = new Category(
                command.Name,
                command.Slug,
                command.ParentId
            );

            if (await _repository.ExistsBySlugAsync(command.Slug, cancellationToken))
                return Result<Guid>.Failure("Category slug already exists");

            await _repository.AddAsync(category, cancellationToken);

            return Result<Guid>.Success(category.Id);
        }
    }
}