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
            DeleteCategoryCommand deleteCategoryCommand,
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
                if (deleteCategoryCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Category ID is required");

                // -------------------------
                // 3. FETCH DATA
                // -------------------------
                var getCategoryByIdAsync = await _categoryRepository.GetCategoryByIdAsync(deleteCategoryCommand.Id, cancellationToken);

                if (getCategoryByIdAsync == null)
                    return Result<Guid>.Failure("Category not found");

                if (getCategoryByIdAsync.IsDeleted)
                    return Result<Guid>.Failure("Category is already deleted");

                // -------------------------
                // 4. BUSINESS RULE: Tidak boleh hapus kategori yang masih punya produk/subkategori aktif
                // -------------------------
                var hasActiveProductsAsync = await _categoryRepository.HasActiveProductsAsync(
                    deleteCategoryCommand.Id,
                    cancellationToken);

                if (hasActiveProductsAsync)
                {
                    var getProductCountAsync = await _categoryRepository.GetProductCountAsync(
                        deleteCategoryCommand.Id,
                        cancellationToken);

                    return Result<Guid>.Failure(
                        $"Cannot delete category. It contains {getProductCountAsync} active product(s). " +
                        "Please move or delete all products first.");
                }

                var hasActiveSubcategoriesAsync = await _categoryRepository.HasActiveSubcategoriesAsync(
                    deleteCategoryCommand.Id,
                    cancellationToken);

                if (hasActiveSubcategoriesAsync)
                {
                    var getActiveSubcategoriesCountAsync = await _categoryRepository.GetActiveSubcategoriesCountAsync(
                        deleteCategoryCommand.Id,
                        cancellationToken);

                    return Result<Guid>.Failure(
                        $"Cannot delete category. It has {getActiveSubcategoriesCountAsync} active subcategory(ies). " +
                        "Please move or delete all subcategories first.");
                }

                // -------------------------
                // 5. DOMAIN OPERATION
                // Domain akan memvalidasi ulang aturan bisnis (Guard checks).
                // Jika ada pelanggaran, DomainException akan ditangkap di catch bawah.
                // -------------------------
                getCategoryByIdAsync.Delete(_currentUser.UserId);

                // -------------------------
                // 6. PERSIST
                // -------------------------
                await _categoryRepository.UpdateCategoryAsync(getCategoryByIdAsync, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(getCategoryByIdAsync.Id);
            }
            catch (DomainException domainException)
            {
                // Ditangkap dari Domain Guard (validasi aturan bisnis)
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                // Ditangkap dari error yang tidak terduga (DB error, dll)
                return Result<Guid>.Failure($"Failed to delete category: {exception.Message}");
            }
        }
    }
}