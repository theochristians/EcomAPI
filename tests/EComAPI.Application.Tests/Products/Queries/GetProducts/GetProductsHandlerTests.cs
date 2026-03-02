using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Products.Queries.GetProducts;
using EComAPI.Domain.Categories.Entities;
using EComAPI.Domain.Products.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Products.Queries.GetProducts
{
    public class GetProductsHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly GetProductsHandler _getProductsHandler;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _categoryId = Guid.NewGuid();

        public GetProductsHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();

            _getProductsHandler = new GetProductsHandler(
                _mockProductRepository.Object,
                _mockCategoryRepository.Object
            );
        }

        [Fact]
        public async Task Handle_PageKurangDariSatu_ShouldReturnFailure()
        {
            var result = await _getProductsHandler.Handle(new GetProductsQuery(Page: 0));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Page must be greater than 0");
        }

        [Fact]
        public async Task Handle_PageSizeKurangDariSatu_ShouldReturnFailure()
        {
            var result = await _getProductsHandler.Handle(new GetProductsQuery(PageSize: 0));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Page size must be greater than 0");
        }

        [Fact]
        public async Task Handle_CategorySlugTidakDitemukan_ShouldReturnEmptyList()
        {
            _mockCategoryRepository
                .Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Category>());

            var result = await _getProductsHandler.Handle(new GetProductsQuery(CategorySlug: "tidak-ada"));

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
            result.Value.TotalPages.Should().Be(0);
        }

        [Fact]
        public async Task Handle_Valid_ShouldReturnPaginatedProductList()
        {
            var product = new Product(_categoryId, "Gaming Laptop", "gaming-laptop", 2000m, _userId);
            var category = new Category("Electronics", "electronics", _userId);

            _mockProductRepository
                .Setup(x => x.GetProductsPaginatedAsync(
                    It.IsAny<List<Guid>?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<Product>)new List<Product> { product }, 1));

            _mockCategoryRepository
                .Setup(x => x.GetCategoryByIdAsync(_categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            var result = await _getProductsHandler.Handle(new GetProductsQuery());

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCount(1);
            result.Value.TotalCount.Should().Be(1);
            result.Value.TotalPages.Should().Be(1);
            result.Value.Items[0].Name.Should().Be("Gaming Laptop");
            result.Value.Items[0].Slug.Should().Be("gaming-laptop");
        }
    }
}
