using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Categories.Commands.RestoreCategories
{
    public class RestoreCategoriesHandler
    {
        private readonly ICategoryRepository _categoriesRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreCategoriesHandler(
            ICategoryRepository categoriesRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _categoriesRepository = categoriesRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            RestoreCategoryCommand restoreCategoryCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (restoreCategoryCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Category ID is required");

                var getByIdIncludeDeletedAsync = await _categoriesRepository.GetCategoryByIdIncludeDeletedAsync(
                    restoreCategoryCommand.Id,
                    cancellationToken);

                if (getByIdIncludeDeletedAsync == null)
                    return Result<Guid>.Failure("Category not found");

                if (!getByIdIncludeDeletedAsync.IsDeleted)
                    return Result<Guid>.Failure("Category is not deleted");

                try
                {
                    getByIdIncludeDeletedAsync.Restore();
                }
                catch (DomainException domainException)
                {
                    return Result<Guid>.Failure(domainException.Message);
                }

                await _categoriesRepository.UpdateCategoryAsync(getByIdIncludeDeletedAsync, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(getByIdIncludeDeletedAsync.Id);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to restore category: {exception.Message}");
            }
        }
    }
}