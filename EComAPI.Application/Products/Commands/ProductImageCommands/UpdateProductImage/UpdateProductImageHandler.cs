using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Commands.ProductImageCommands.UpdateProductImage
{
    public class UpdateProductImageHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductImageHandler(
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            UpdateProductImageCommand updateProductImageCommand,
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
                if (updateProductImageCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Image ID is required");

                // -------------------------
                // 3. FETCH DATA
                // -------------------------
                var productImage = await _productRepository.GetProductImageByIdAsync(
                    updateProductImageCommand.Id,
                    cancellationToken);

                if (productImage == null)
                    return Result<Guid>.Failure("Image not found");

                // Fetch product beserta seluruh images-nya
                // diperlukan untuk menjaga aturan: hanya 1 primary per produk
                var product = await _productRepository.GetProductByIdWithImagesAsync(
                    productImage.ProductId,
                    cancellationToken);

                if (product == null)
                    return Result<Guid>.Failure("Product not found");

                // -------------------------
                // 4. RESOLVE VALUES
                // Partial update — pakai nilai lama jika tidak dikirim
                // -------------------------
                var imageUrl    = !string.IsNullOrWhiteSpace(updateProductImageCommand.ImageUrl)
                                    ? updateProductImageCommand.ImageUrl
                                    : productImage.ImageUrl;

                var isPrimary    = updateProductImageCommand.IsPrimary ?? productImage.IsPrimary;
                var displayOrder = updateProductImageCommand.DisplayOrder ?? productImage.DisplayOrder;

                // -------------------------
                // 5. BUSINESS RULE: Hanya 1 primary image per product
                // Kondisi: request ingin set primary (isPrimary = true)
                //          DAN gambar ini sebelumnya bukan primary (!productImage.IsPrimary)
                //          → berarti ada perubahan status, maka unset semua gambar lain
                //
                // Jika gambar ini sudah primary sebelumnya → skip (tidak ada perubahan)
                // Jika isPrimary = false → skip (tidak perlu unset siapapun)
                // -------------------------
                if (isPrimary && !productImage.IsPrimary)
                {
                    foreach (var otherImage in product.Images.Where(i => i.IsPrimary && !i.IsDeleted && i.Id != productImage.Id))
                    {
                        otherImage.UnsetPrimary();
                        await _productRepository.UpdateProductImageAsync(otherImage, cancellationToken);
                    }
                }

                // -------------------------
                // 6. DOMAIN OPERATION
                // Domain akan memvalidasi ulang aturan bisnis (Guard checks).
                // Jika ada pelanggaran, DomainException akan ditangkap di catch bawah.
                // -------------------------
                productImage.Update(
                    imageUrl,
                    isPrimary,
                    displayOrder,
                    _currentUser.UserId
                );

                // -------------------------
                // 7. PERSIST
                // -------------------------
                await _productRepository.UpdateProductImageAsync(productImage, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(productImage.Id);
            }
            catch (DomainException domainException)
            {
                // Ditangkap dari Domain Guard (validasi aturan bisnis)
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                // Ditangkap dari error yang tidak terduga (DB error, dll)
                return Result<Guid>.Failure($"Failed to update image: {exception.Message}");
            }
        }
    }
}

