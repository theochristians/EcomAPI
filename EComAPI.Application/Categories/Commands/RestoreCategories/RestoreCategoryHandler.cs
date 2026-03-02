using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Categories.Commands.RestoreCategories
{
    public class RestoreCategoryHandler
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreCategoryHandler(
            ICategoryRepository categoryRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            RestoreCategoryCommand restoreCategoryCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // -------------------------
                // 1. AUTHENTICATION CHECK
                // -------------------------
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                // -------------------------
                // 2. INPUT VALIDATION (fail-fast: sebelum query DB)
                // -------------------------
                if (restoreCategoryCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Category ID is required");

                // -------------------------
                // 3. FETCH DATA
                // -------------------------
                var getCategoryByIdIncludeDeletedAsync = await _categoryRepository.GetCategoryByIdIncludeDeletedAsync(
                    restoreCategoryCommand.Id,
                    cancellationToken);

                if (getCategoryByIdIncludeDeletedAsync == null)
                    return Result<Guid>.Failure("Category not found");

                if (!getCategoryByIdIncludeDeletedAsync.IsDeleted)
                    return Result<Guid>.Failure("Category is not deleted");

                // -------------------------
                // 4. DOMAIN OPERATION
                // Domain akan memvalidasi ulang aturan bisnis (Guard checks).
                // Jika ada pelanggaran, DomainException akan ditangkap di catch bawah.
                // -------------------------
                getCategoryByIdIncludeDeletedAsync.Restore();

                // -------------------------
                // 5. PERSIST
                // -------------------------
                await _categoryRepository.UpdateCategoryAsync(getCategoryByIdIncludeDeletedAsync, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(getCategoryByIdIncludeDeletedAsync.Id);
            }
            catch (DomainException domainException)
            {
                // Ditangkap dari Domain Guard (validasi aturan bisnis)
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                // Ditangkap dari error yang tidak terduga (DB error, dll)
                return Result<Guid>.Failure($"Failed to restore category: {exception.Message}");
            }
        }
    }
}