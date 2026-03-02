using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Categories.Commands.CreateCategories;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Categories.Entities;

namespace EComAPI.Application.Tests.Categories.Commands.CreateCategory
{
    public class CreateCategoryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CreateCategoryHandler _createCategoryHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public CreateCategoryHandlerTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _createCategoryHandler = new CreateCategoryHandler(
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

            var createCategoryResult = await _createCategoryHandler.Handle(new CreateCategoryCommand("Electronics", "electronics", null, null, null));

            createCategoryResult.IsSuccess.Should().BeFalse();
            createCategoryResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_NamaKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var createCategoryResult = await _createCategoryHandler.Handle(new CreateCategoryCommand("", "electronics", null, null, null));

            createCategoryResult.IsSuccess.Should().BeFalse();
            createCategoryResult.Error.Should().Be("Category name is required");
        }

        [Fact]
        public async Task Handle_SlugKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var createCategoryResult = await _createCategoryHandler.Handle(new CreateCategoryCommand("Electronics", "", null, null, null));

            createCategoryResult.IsSuccess.Should().BeFalse();
            createCategoryResult.Error.Should().Be("Category slug is required");
        }

        [Fact]
        public async Task Handle_SlugSudahAda_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockCategoryRepository
                .Setup(x => x.CategoryExistsBySlugAsync("electronics", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var createCategoryResult = await _createCategoryHandler.Handle(new CreateCategoryCommand("Electronics", "electronics", null, null, null));

            createCategoryResult.IsSuccess.Should().BeFalse();
            createCategoryResult.Error.Should().Be("Category slug already exists");
        }

        [Fact]
        public async Task Handle_ParentCategoryTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var parentId = Guid.NewGuid();

            _mockCategoryRepository
                .Setup(x => x.CategoryExistsBySlugAsync("electronics", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(x => x.CategoryExistsAsync(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var createCategoryResult = await _createCategoryHandler.Handle(new CreateCategoryCommand("Electronics", "electronics", parentId, null, null));

            createCategoryResult.IsSuccess.Should().BeFalse();
            createCategoryResult.Error.Should().Be("Parent category not found");
        }

        [Fact]
        public async Task Handle_ValidInput_ShouldReturnCategoryId()
        {
            SetupAuthenticatedUser();

            _mockCategoryRepository
                .Setup(x => x.CategoryExistsBySlugAsync("electronics", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var createCategoryResult = await _createCategoryHandler.Handle(new CreateCategoryCommand("Electronics", "electronics", null, null, null));

            createCategoryResult.IsSuccess.Should().BeTrue();
            createCategoryResult.Value.Should().NotBeEmpty();

            _mockCategoryRepository.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
