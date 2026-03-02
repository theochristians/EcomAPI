using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Commands.ProductCommands.UpdateProduct
{
    public class UpdateProductHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoriesRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductHandler(
            IProductRepository productRepository,
            ICategoryRepository categoriesRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _categoriesRepository = categoriesRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            UpdateProductCommand updateProductCommand,
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
                // Validasi dasar dilakukan di sini agar tidak membuang
                // round-trip ke database jika input sudah jelas tidak valid.
                // Validasi detail (max length, format, dll) tetap ada di Domain.
                // -------------------------
                if (updateProductCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Product ID is required");

                // Fail-fast: Domain juga cek ini via Guard.AgainstNegative,
                // tapi dicek di sini agar tidak perlu query DB dulu.
                if (updateProductCommand.BasePrice.HasValue && updateProductCommand.BasePrice < 0)
                    return Result<Guid>.Failure("Base price cannot be negative");

                // -------------------------
                // 3. FETCH DATA
                // -------------------------
                var product = await _productRepository.GetProductByIdAsync(
                    updateProductCommand.Id,
                    cancellationToken);

                if (product == null)
                    return Result<Guid>.Failure("Product not found");

                // -------------------------
                // 4. RESOLVE VALUES
                // Jika field tidak dikirim (null/empty), gunakan nilai lama dari DB.
                // Ini memungkinkan partial update — client hanya kirim field yang berubah.
                // -------------------------
                var name        = !string.IsNullOrWhiteSpace(updateProductCommand.Name)
                                    ? updateProductCommand.Name
                                    : product.Name;

                var slug        = !string.IsNullOrWhiteSpace(updateProductCommand.Slug)
                                    ? updateProductCommand.Slug
                                    : product.Slug;

                var basePrice   = updateProductCommand.BasePrice ?? product.BasePrice;

                var categoryId  = updateProductCommand.CategoryId ?? product.CategoryId;

                // Description: null = tidak dikirim (pakai lama), "" = sengaja dikosongkan (set null)
                var description = updateProductCommand.Description != null
                                    ? (string.IsNullOrWhiteSpace(updateProductCommand.Description) ? null : updateProductCommand.Description)
                                    : product.Description;

                // -------------------------
                // 5. UNIQUENESS CHECKS (hanya cek jika nilai benar-benar berubah)
                // -------------------------

                // Cek slug hanya jika slug baru berbeda dari yang lama
                if (!string.IsNullOrWhiteSpace(updateProductCommand.Slug) &&
                    updateProductCommand.Slug != product.Slug)
                {
                    var slugExists = await _productRepository.ProductExistsBySlugAsync(
                        updateProductCommand.Slug,
                        cancellationToken);

                    if (slugExists)
                        return Result<Guid>.Failure("Product slug already exists");
                }

                // Cek category hanya jika category baru berbeda dari yang lama
                if (updateProductCommand.CategoryId.HasValue &&
                    updateProductCommand.CategoryId != product.CategoryId)
                {
                    var categoryExists = await _categoriesRepository.CategoryExistsAsync(
                        updateProductCommand.CategoryId.Value,
                        cancellationToken);

                    if (!categoryExists)
                        return Result<Guid>.Failure("Category not found");
                }

                // -------------------------
                // 6. DOMAIN OPERATION
                // Domain akan memvalidasi ulang aturan bisnis (Guard checks).
                // Jika ada pelanggaran, DomainException akan ditangkap di catch bawah.
                // -------------------------
                product.Update(
                    name,
                    slug,
                    basePrice,
                    _currentUser.UserId,
                    description,
                    categoryId
                );

                // -------------------------
                // 7. PERSIST
                // -------------------------
                await _productRepository.UpdateProductAsync(product, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(product.Id);
            }
            catch (DomainException domainException)
            {
                // Ditangkap dari Domain Guard (validasi aturan bisnis)
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                // Ditangkap dari error yang tidak terduga (DB error, dll)
                return Result<Guid>.Failure($"Failed to update product: {exception.Message}");
            }
        }
    }
}
