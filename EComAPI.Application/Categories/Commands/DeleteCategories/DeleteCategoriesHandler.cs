using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Categories.Commands.DeleteCategories
{
    public class DeleteCategoryHandler
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCategoryHandler(
            ICategoryRepository categoryRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            DeleteCategoryCommand deleteCategoriesCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (deleteCategoriesCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Category ID is required");

                var category = await _categoryRepository.GetCategoryByIdAsync(deleteCategoriesCommand.Id, cancellationToken);

                if (category == null)
                    return Result<Guid>.Failure("Category not found");

                if (category.IsDeleted)
                    return Result<Guid>.Failure("Category is already deleted");

                var hasActiveProductsAsync = await _categoryRepository.HasActiveProductsAsync(
                    deleteCategoriesCommand.Id,
                    cancellationToken);

                if (hasActiveProductsAsync)
                {
                    var getProductCountAsync = await _categoryRepository.GetProductCountAsync(
                        deleteCategoriesCommand.Id,
                        cancellationToken);

                    return Result<Guid>.Failure(
                        $"Cannot delete category. It contains {getProductCountAsync} active product(s). " +
                        "Please move or delete all products first.");
                }

                var hasActiveSubcategoriesAsync = await _categoryRepository.HasActiveSubcategoriesAsync(
                    deleteCategoriesCommand.Id,
                    cancellationToken);

                if (hasActiveSubcategoriesAsync)
                {
                    var getActiveSubcategoriesCountAsync = await _categoryRepository.GetActiveSubcategoriesCountAsync(
                        deleteCategoriesCommand.Id,
                        cancellationToken);

                    return Result<Guid>.Failure(
                        $"Cannot delete category. It has {getActiveSubcategoriesCountAsync} active subcategory(ies). " +
                        "Please move or delete all subcategories first.");
                }

                try
                {
                    category.Delete(_currentUser.UserId);
                }
                catch (DomainException domainException)
                {
                    return Result<Guid>.Failure(domainException.Message);
                }

                await _categoryRepository.UpdateCategoryAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(category.Id);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to delete category: {exception.Message}");
            }
        }
    }
}