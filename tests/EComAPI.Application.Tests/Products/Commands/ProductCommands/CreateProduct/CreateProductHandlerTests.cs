using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductCommands.CreateProduct;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;

namespace EComAPI.Application.Tests.Products.Commands.ProductCommands.CreateProduct
{
    public class CreateProductHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CreateProductHandler _createProductHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public CreateProductHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _createProductHandler = new CreateProductHandler(
                _mockProductRepository.Object,
                _mockCategoryRepository.Object,
                _mockCurrentUser.Object,
                _mockUnitOfWork.Object
            );
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var createProductResult = await _createProductHandler.Handle(new CreateProductCommand(Guid.NewGuid(), "Laptop", "laptop", 1000m));

            createProductResult.IsSuccess.Should().BeFalse();
            createProductResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_CategoryIdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var createProductResult = await _createProductHandler.Handle(new CreateProductCommand(Guid.Empty, "Laptop", "laptop", 1000m));

            createProductResult.IsSuccess.Should().BeFalse();
            createProductResult.Error.Should().Be("Category ID is required");
        }

        [Fact]
        public async Task Handle_NamaKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var createProductResult = await _createProductHandler.Handle(new CreateProductCommand(Guid.NewGuid(), "", "laptop", 1000m));

            createProductResult.IsSuccess.Should().BeFalse();
            createProductResult.Error.Should().Be("Product name is required");
        }

        [Fact]
        public async Task Handle_SlugKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var createProductResult = await _createProductHandler.Handle(new CreateProductCommand(Guid.NewGuid(), "Laptop", "", 1000m));

            createProductResult.IsSuccess.Should().BeFalse();
            createProductResult.Error.Should().Be("Product slug is required");
        }

        [Fact]
        public async Task Handle_HargaNegatif_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var createProductResult = await _createProductHandler.Handle(new CreateProductCommand(Guid.NewGuid(), "Laptop", "laptop", -1m));

            createProductResult.IsSuccess.Should().BeFalse();
            createProductResult.Error.Should().Be("Base price cannot be negative");
        }

        [Fact]
        public async Task Handle_CategoryTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockCategoryRepository
                .Setup(x => x.CategoryExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var createProductResult = await _createProductHandler.Handle(new CreateProductCommand(Guid.NewGuid(), "Laptop", "laptop", 1000m));

            createProductResult.IsSuccess.Should().BeFalse();
            createProductResult.Error.Should().Be("Category not found");
        }

        [Fact]
        public async Task Handle_SlugSudahAda_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockCategoryRepository
                .Setup(x => x.CategoryExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockProductRepository
                .Setup(x => x.ProductExistsBySlugAsync("laptop", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var createProductResult = await _createProductHandler.Handle(new CreateProductCommand(Guid.NewGuid(), "Laptop", "laptop", 1000m));

            createProductResult.IsSuccess.Should().BeFalse();
            createProductResult.Error.Should().Be("Product slug already exists");
        }

        [Fact]
        public async Task Handle_ValidInput_ShouldReturnProductId()
        {
            SetupAuthenticatedUser();

            _mockCategoryRepository
                .Setup(x => x.CategoryExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockProductRepository
                .Setup(x => x.ProductExistsBySlugAsync("laptop", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var createProductResult = await _createProductHandler.Handle(new CreateProductCommand(Guid.NewGuid(), "Laptop", "laptop", 1000m));

            createProductResult.IsSuccess.Should().BeTrue();
            createProductResult.Value.Should().NotBeEmpty();

            _mockProductRepository.Verify(
                x => x.AddProductAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
