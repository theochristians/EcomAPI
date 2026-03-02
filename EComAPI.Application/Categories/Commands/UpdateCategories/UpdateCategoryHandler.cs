using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Categories.Commands.UpdateCategories
{
    public class UpdateCategoryHandler
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCategoryHandler(
            ICategoryRepository categoryRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            UpdateCategoryCommand updateCategoryCommand,
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
                if (updateCategoryCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Category ID is required");

                // -------------------------
                // 3. FETCH DATA
                // -------------------------
                var getCategoriesByIdAsync = await _categoryRepository.GetCategoryByIdAsync(updateCategoryCommand.Id, cancellationToken);

                if (getCategoriesByIdAsync is null)
                    return Result<Guid>.Failure("Category not found");

                // -------------------------
                // 4. RESOLVE VALUES
                // Partial update — pakai nilai lama jika tidak dikirim
                // -------------------------
                var name = !string.IsNullOrWhiteSpace(updateCategoryCommand.Name) ? updateCategoryCommand.Name : getCategoriesByIdAsync.Name;
                var slug = !string.IsNullOrWhiteSpace(updateCategoryCommand.Slug) ? updateCategoryCommand.Slug : getCategoriesByIdAsync.Slug;
                var parentId = updateCategoryCommand.ParentId ?? getCategoriesByIdAsync.ParentId;

                // ImageUrl/Description: null = tidak dikirim (pakai lama), "" = sengaja dikosongkan (set null)
                string? imageUrl = getCategoriesByIdAsync.ImageUrl;
                if (updateCategoryCommand.ImageUrl != null)
                {
                    imageUrl = string.IsNullOrWhiteSpace(updateCategoryCommand.ImageUrl) ? null : updateCategoryCommand.ImageUrl;
                }

                string? description = getCategoriesByIdAsync.Description;
                if (updateCategoryCommand.Description != null)
                {
                    description = string.IsNullOrWhiteSpace(updateCategoryCommand.Description) ? null : updateCategoryCommand.Description;
                }

                // -------------------------
                // 5. UNIQUENESS CHECKS (hanya cek jika nilai benar-benar berubah)
                // -------------------------
                if (!string.IsNullOrWhiteSpace(updateCategoryCommand.Slug) && updateCategoryCommand.Slug != getCategoriesByIdAsync.Slug)
                {
                    var categoriesExistsBySlugAsync = await _categoryRepository.CategoryExistsBySlugAsync(
                        updateCategoryCommand.Slug,
                        cancellationToken);

                    if (categoriesExistsBySlugAsync)
                        return Result<Guid>.Failure("Category slug already exists");
                }

                if (updateCategoryCommand.ParentId.HasValue)
                {
                    if (updateCategoryCommand.ParentId == updateCategoryCommand.Id)
                        return Result<Guid>.Failure("Category cannot be its own parent");

                    var categoriesParExistsAsync = await _categoryRepository.CategoryExistsAsync(
                        updateCategoryCommand.ParentId.Value,
                        cancellationToken);

                    if (!categoriesParExistsAsync)
                        return Result<Guid>.Failure("Parent category not found");
                }

                // -------------------------
                // 6. DOMAIN OPERATION
                // Domain akan memvalidasi ulang aturan bisnis (Guard checks).
                // Jika ada pelanggaran, DomainException akan ditangkap di catch bawah.
                // -------------------------
                getCategoriesByIdAsync.Update(
                    name,
                    slug,
                    _currentUser.UserId,
                    parentId,
                    imageUrl,
                    description
                );

                // -------------------------
                // 7. PERSIST
                // -------------------------
                await _categoryRepository.UpdateCategoryAsync(getCategoriesByIdAsync, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(getCategoriesByIdAsync.Id);
            }
            catch (DomainException domainException)
            {
                // Ditangkap dari Domain Guard (validasi aturan bisnis)
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                // Ditangkap dari error yang tidak terduga (DB error, dll)
                return Result<Guid>.Failure($"Failed to update category: {exception.Message}");
            }
        }
    }
}