using EComAPI.Application.Categories.Commands.UpdateCategory;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Results;

namespace EComAPI.Application.Categories.Commands.CreateCategory
{
    public class UpdateCategoryHandler
    {
        private readonly ICategoryRepository _categoryRepository;

        public UpdateCategoryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<Guid>> Handle(
            UpdateCategoryCommand command,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Ambil category lama
            var category = await _categoryRepository
                .GetByIdAsync(command.Id, cancellationToken);

            if (category is null)
                return Result<Guid>.Failure("Category not found");

            // 2️⃣ Cek slug duplikat (kecuali milik sendiri)
            var slugExists = await _categoryRepository
                .ExistsBySlugAsync(command.Slug, cancellationToken);

            if (slugExists && category.Slug != command.Slug)
                return Result<Guid>.Failure("Category slug already exists");

            // 3️⃣ Update via domain method
            category.Update(
                command.Name,
                command.Slug,
                command.ParentId
            );

            // 4️⃣ Save
            await _categoryRepository.UpdateAsync(category, cancellationToken);

            return Result<Guid>.Success(category.Id);
        }
    }
}