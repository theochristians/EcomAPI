using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Categories.Queries.GetCategoriesBySlug;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Domain.Categories.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Categories.Queries.GetCategoriesBySlug
{
    public class GetCategoryBySlugHandlerTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly GetCategoryBySlugHandler _getCategoryBySlugHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public GetCategoryBySlugHandlerTests()
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

            _getCategoryBySlugHandler = new GetCategoryBySlugHandler(
                _mockCategoryRepository.Object,
                _mockCacheService.Object
            );
        }

        private Category BuildActiveCategory() => new Category(
            name: "Electronics",
            slug: "electronics",
            createdBy: _userId
        );

        [Fact]
        public async Task Handle_SlugKosong_ShouldReturnFailure()
        {
            var getCategoryBySlugQuery = new GetCategoryBySlugQuery(Slug: "");

            var getCategoryBySlugResult = await _getCategoryBySlugHandler.Handle(getCategoryBySlugQuery);

            getCategoryBySlugResult.IsSuccess.Should().BeFalse();
            getCategoryBySlugResult.Error.Should().Be("Slug is required");
        }

        [Fact]
        public async Task Handle_CategoryTidakDitemukan_ShouldReturnFailure()
        {
            _mockCategoryRepository
                .Setup(x => x.GetCategoryBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            var getCategoryBySlugQuery = new GetCategoryBySlugQuery(Slug: "tidak-ada");

            var getCategoryBySlugResult = await _getCategoryBySlugHandler.Handle(getCategoryBySlugQuery);

            getCategoryBySlugResult.IsSuccess.Should().BeFalse();
            getCategoryBySlugResult.Error.Should().Be("Category not found");
        }

        [Fact]
        public async Task Handle_Valid_ShouldReturnCategoryDto()
        {
            var category = BuildActiveCategory();

            _mockCategoryRepository
                .Setup(x => x.GetCategoryBySlugAsync("electronics", It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _mockCategoryRepository
                .Setup(x => x.GetProductCountAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);

            var getCategoryBySlugQuery = new GetCategoryBySlugQuery(Slug: "electronics");

            var getCategoryBySlugResult = await _getCategoryBySlugHandler.Handle(getCategoryBySlugQuery);

            getCategoryBySlugResult.IsSuccess.Should().BeTrue();
            getCategoryBySlugResult.Value.Should().NotBeNull();
            getCategoryBySlugResult.Value!.Name.Should().Be("Electronics");
            getCategoryBySlugResult.Value.Slug.Should().Be("electronics");
        }
    }
}
