using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductImageCommands.UpdateProductImage;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Products.Commands.ProductImageCommands.UpdateProductImage
{
    public class UpdateProductImageHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UpdateProductImageHandler _updateProductImageHandler;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _categoryId = Guid.NewGuid();

        public UpdateProductImageHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _updateProductImageHandler = new UpdateProductImageHandler(
                _mockProductRepository.Object,
                _mockCurrentUser.Object,
                _mockUnitOfWork.Object
            );
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        private ProductImage BuildImage(Guid productId, bool isPrimary = false) =>
            new ProductImage(productId, "https://img.example.com/old.jpg", _userId, isPrimary, 1);

        // ─────────────────────────────────────────────────────────
        // Authentication & input validation
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var result = await _updateProductImageHandler.Handle(
                new UpdateProductImageCommand(Guid.NewGuid(), "https://new.jpg", false, 1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _updateProductImageHandler.Handle(
                new UpdateProductImageCommand(Guid.Empty, "https://new.jpg", false, 1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Image ID is required");
        }

        // ─────────────────────────────────────────────────────────
        // Data not found
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ImageTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            _mockProductRepository
                .Setup(x => x.GetProductImageByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductImage?)null);

            var result = await _updateProductImageHandler.Handle(
                new UpdateProductImageCommand(Guid.NewGuid(), null, null, null));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Image not found");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var productId = Guid.NewGuid();
            var image = BuildImage(productId);
            _mockProductRepository
                .Setup(x => x.GetProductImageByIdAsync(image.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(image);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithImagesAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var result = await _updateProductImageHandler.Handle(
                new UpdateProductImageCommand(image.Id, null, null, null));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product not found");
        }

        // ─────────────────────────────────────────────────────────
        // Success — update penuh
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_UpdateNilaiLengkap_ShouldUpdateImageAndReturnId()
        {
            SetupAuthenticatedUser();
            var product = new Product(_categoryId, "Laptop", "laptop", 1000m, _userId);
            var image = BuildImage(product.Id, isPrimary: false);
            product.AddImage(image);

            _mockProductRepository
                .Setup(x => x.GetProductImageByIdAsync(image.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(image);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithImagesAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _updateProductImageHandler.Handle(
                new UpdateProductImageCommand(image.Id, "https://img.example.com/new.jpg", false, 5));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(image.Id);
            image.ImageUrl.Should().Be("https://img.example.com/new.jpg");
            image.DisplayOrder.Should().Be(5);
            _mockProductRepository.Verify(x => x.UpdateProductImageAsync(image, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // ─────────────────────────────────────────────────────────
        // Success — partial update (null fields keep existing values)
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_NilaiNull_ShouldKeepExistingValues()
        {
            SetupAuthenticatedUser();
            var product = new Product(_categoryId, "Laptop", "laptop", 1000m, _userId);
            var image = BuildImage(product.Id, isPrimary: false);
            product.AddImage(image);
            var originalUrl = image.ImageUrl;
            var originalOrder = image.DisplayOrder;

            _mockProductRepository
                .Setup(x => x.GetProductImageByIdAsync(image.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(image);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithImagesAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // Semua null → tidak ada yang berubah
            var result = await _updateProductImageHandler.Handle(
                new UpdateProductImageCommand(image.Id, null, null, null));

            result.IsSuccess.Should().BeTrue();
            image.ImageUrl.Should().Be(originalUrl);
            image.DisplayOrder.Should().Be(originalOrder);
        }

        // ─────────────────────────────────────────────────────────
        // Business rule: set as primary → unset other primary images
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_SetSebagaiPrimary_ShouldUnsetGambarPrimaryLain()
        {
            SetupAuthenticatedUser();
            var product = new Product(_categoryId, "Laptop", "laptop", 1000m, _userId);
            // image1 saat ini primary, image2 ingin dijadikan primary
            var image1 = new ProductImage(product.Id, "https://img.example.com/1.jpg", _userId, true, 1);
            var image2 = new ProductImage(product.Id, "https://img.example.com/2.jpg", _userId, false, 2);
            product.AddImage(image1);
            product.AddImage(image2);

            _mockProductRepository
                .Setup(x => x.GetProductImageByIdAsync(image2.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(image2);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithImagesAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _updateProductImageHandler.Handle(
                new UpdateProductImageCommand(image2.Id, null, true, null));

            result.IsSuccess.Should().BeTrue();
            // image1 harus di-unset
            image1.IsPrimary.Should().BeFalse();
            // image2 harus jadi primary
            image2.IsPrimary.Should().BeTrue();
            // UpdateProductImageAsync dipanggil untuk image1 (unset) dan image2 (update)
            _mockProductRepository.Verify(x => x.UpdateProductImageAsync(image1, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductRepository.Verify(x => x.UpdateProductImageAsync(image2, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_SudahPrimary_TidakPerluUnsetLain()
        {
            SetupAuthenticatedUser();
            var product = new Product(_categoryId, "Laptop", "laptop", 1000m, _userId);
            // image sudah primary sejak awal → tidak ada yang perlu di-unset
            var image = new ProductImage(product.Id, "https://img.example.com/1.jpg", _userId, true, 1);
            product.AddImage(image);

            _mockProductRepository
                .Setup(x => x.GetProductImageByIdAsync(image.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(image);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithImagesAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _updateProductImageHandler.Handle(
                new UpdateProductImageCommand(image.Id, null, true, null));

            result.IsSuccess.Should().BeTrue();
            // Tidak ada gambar lain yang di-update (unset tidak terjadi)
            _mockProductRepository.Verify(x => x.UpdateProductImageAsync(
                It.Is<ProductImage>(i => i.Id != image.Id), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}