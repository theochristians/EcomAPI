using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Categories.Commands.UpdateCategories
{
    public class UpdateCategoryHandler
    {
        private readonly ICategoryRepository _categoriesRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCategoryHandler(
            ICategoryRepository categoriesRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _categoriesRepository = categoriesRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            UpdateCategoryCommand updateCategoriesCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (updateCategoriesCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Category ID is required");

                var getCategoriesByIdAsync = await _categoriesRepository.GetCategoryByIdAsync(updateCategoriesCommand.Id, cancellationToken);

                if (getCategoriesByIdAsync is null)
                    return Result<Guid>.Failure("Category not found");

                var name = !string.IsNullOrWhiteSpace(updateCategoriesCommand.Name) ? updateCategoriesCommand.Name : getCategoriesByIdAsync.Name;
                var slug = !string.IsNullOrWhiteSpace(updateCategoriesCommand.Slug) ? updateCategoriesCommand.Slug : getCategoriesByIdAsync.Slug;
                var parentId = updateCategoriesCommand.ParentId ?? getCategoriesByIdAsync.ParentId;

                string? imageUrl = getCategoriesByIdAsync.ImageUrl;
                if (updateCategoriesCommand.ImageUrl != null)
                {
                    imageUrl = string.IsNullOrWhiteSpace(updateCategoriesCommand.ImageUrl) ? null : updateCategoriesCommand.ImageUrl;
                }

                string? description = getCategoriesByIdAsync.Description;
                if (updateCategoriesCommand.Description != null)
                {
                    description = string.IsNullOrWhiteSpace(updateCategoriesCommand.Description) ? null : updateCategoriesCommand.Description;
                }

                if (!string.IsNullOrWhiteSpace(updateCategoriesCommand.Slug) && updateCategoriesCommand.Slug != getCategoriesByIdAsync.Slug)
                {
                    var categoriesExistsBySlugAsync = await _categoriesRepository.CategoryExistsBySlugAsync(
                        updateCategoriesCommand.Slug,
                        cancellationToken);

                    if (categoriesExistsBySlugAsync)
                        return Result<Guid>.Failure("Category slug already exists");
                }

                if (updateCategoriesCommand.ParentId.HasValue)
                {
                    if (updateCategoriesCommand.ParentId == updateCategoriesCommand.Id)
                        return Result<Guid>.Failure("Category cannot be its own parent");

                    var categoryParExistsAsync = await _categoriesRepository.CategoryExistsAsync(
                        updateCategoriesCommand.ParentId.Value,
                        cancellationToken);

                    if (!categoryParExistsAsync)
                        return Result<Guid>.Failure("Parent category not found");
                }

                try
                {
                    getCategoriesByIdAsync.Update(
                        name,
                        slug,
                        _currentUser.UserId,
                        parentId,
                        imageUrl,
                        description
                    );
                }
                catch (DomainException domainException)
                {
                    return Result<Guid>.Failure(domainException.Message);
                }

                await _categoriesRepository.UpdateCategoryAsync(getCategoriesByIdAsync, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(getCategoriesByIdAsync.Id);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to update category: {exception.Message}");
            }
        }
    }
}