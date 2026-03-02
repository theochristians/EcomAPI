using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Categories.Entities;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Categories.Commands.CreateCategories
{
    public class CreateCategoryHandler
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCategoryHandler(
            ICategoryRepository categoryRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateCategoryCommand createCategoryCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (string.IsNullOrWhiteSpace(createCategoryCommand.Name))
                    return Result<Guid>.Failure("Category name is required");

                if (string.IsNullOrWhiteSpace(createCategoryCommand.Slug))
                    return Result<Guid>.Failure("Category slug is required");

                var categoryExistsBySlugAsync = await _categoryRepository.CategoryExistsBySlugAsync(
                    createCategoryCommand.Slug,
                    cancellationToken);

                if (categoryExistsBySlugAsync)
                    return Result<Guid>.Failure("Category slug already exists");

                if (createCategoryCommand.ParentId.HasValue)
                {
                    var parentCategoryExistsAsync = await _categoryRepository.CategoryExistsAsync(
                        createCategoryCommand.ParentId.Value,
                        cancellationToken);

                    if (!parentCategoryExistsAsync)
                        return Result<Guid>.Failure("Parent category not found");
                }

                var category = new Category(
                    createCategoryCommand.Name,
                    createCategoryCommand.Slug,
                    _currentUser.UserId,
                    createCategoryCommand.ParentId,
                    createCategoryCommand.ImageUrl,
                    createCategoryCommand.Description
                );

                await _categoryRepository.AddCategoryAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(category.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to create category: {exception.Message}");
            }
        }
    }
}