using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Products.Queries.GetProductBySlug;
using EComAPI.Domain.Categories.Entities;
using EComAPI.Domain.Products.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Products.Queries.GetProductBySlug
{
    public class GetProductBySlugHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly GetProductBySlugHandler _getProductBySlugHandler;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _categoryId = Guid.NewGuid();

        public GetProductBySlugHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();

            _getProductBySlugHandler = new GetProductBySlugHandler(
                _mockProductRepository.Object,
                _mockCategoryRepository.Object
            );
        }

        [Fact]
        public async Task Handle_SlugKosong_ShouldReturnFailure()
        {
            var result = await _getProductBySlugHandler.Handle(new GetProductBySlugQuery(""));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Slug is required");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            _mockProductRepository
                .Setup(x => x.GetProductBySlugAsync("tidak-ada", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var result = await _getProductBySlugHandler.Handle(new GetProductBySlugQuery("tidak-ada"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product not found");
        }

        [Fact]
        public async Task Handle_Valid_ShouldReturnProductDetailDto()
        {
            var product = new Product(_categoryId, "Gaming Laptop", "gaming-laptop", 2000m, _userId);
            var category = new Category("Electronics", "electronics", _userId);
            _mockProductRepository
                .Setup(x => x.GetProductBySlugAsync("gaming-laptop", It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            _mockCategoryRepository
                .Setup(x => x.GetCategoryByIdAsync(_categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            var result = await _getProductBySlugHandler.Handle(new GetProductBySlugQuery("gaming-laptop"));

            result.IsSuccess.Should().BeTrue();
            var dto = result.Value!;
            dto.Id.Should().Be(product.Id);
            dto.Name.Should().Be("Gaming Laptop");
            dto.Slug.Should().Be("gaming-laptop");
            dto.BasePrice.Should().Be(2000m);
            dto.Category.Should().NotBeNull();
            dto.Category!.Name.Should().Be("Electronics");
        }
    }
}
