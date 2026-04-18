using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Categories.Queries.GetCategories;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Domain.Categories.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Categories.Queries.GetCategories
{
    public class GetCategoryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly GetCategoriesHandler _getCategoryHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public GetCategoryHandlerTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockCacheService = new Mock<ICacheService>();

            _mockCacheService
                .Setup(x => x.GetNamespaceVersionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, long>
                {
                    ["categories"] = 1,
                    ["products"] = 1
                });

            _getCategoryHandler = new GetCategoriesHandler(
                _mockCategoryRepository.Object,
                _mockCacheService.Object);
        }

        [Fact]
        public async Task Handle_NoCategoriesExist_ShouldReturnEmptyList()
        {
            _mockCategoryRepository
                .Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Category>().AsReadOnly());

            _mockCategoryRepository
                .Setup(x => x.GetProductCountsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, int>());

            var getCategoryResult = await _getCategoryHandler.Handle(new GetCategoriesQuery(), CancellationToken.None);

            getCategoryResult.IsSuccess.Should().BeTrue();
            getCategoryResult.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CategoriesExist_ShouldReturnDtoList()
        {
            var categories = new List<Category>
            {
                new Category("Electronics", "electronics", _userId),
                new Category("Clothing", "clothing", _userId)
            };

            var productCounts = new Dictionary<Guid, int>
            {
                { categories[0].Id, 5 },
                { categories[1].Id, 3 }
            };

            _mockCategoryRepository
                .Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories.AsReadOnly());

            _mockCategoryRepository
                .Setup(x => x.GetProductCountsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(productCounts);

            var getCategoryResult = await _getCategoryHandler.Handle(new GetCategoriesQuery(), CancellationToken.None);

            getCategoryResult.IsSuccess.Should().BeTrue();
            getCategoryResult.Value.Should().HaveCount(2);
            getCategoryResult.Value!.First(c => c.Slug == "electronics").ProductCount.Should().Be(5);
            getCategoryResult.Value!.First(c => c.Slug == "clothing").ProductCount.Should().Be(3);
        }
    }
}
