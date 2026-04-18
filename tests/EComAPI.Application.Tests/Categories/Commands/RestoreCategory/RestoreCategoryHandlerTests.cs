using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Categories.Commands.RestoreCategories;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Categories.Entities;

namespace EComAPI.Application.Tests.Categories.Commands.RestoreCategory
{
    public class RestoreCategoryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly RestoreCategoriesHandler _restoreCategoryHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public RestoreCategoryHandlerTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _restoreCategoryHandler = new RestoreCategoriesHandler(
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

        private Category CreateDeletedCategory()
        {
            var category = new Category("Electronics", "electronics", _userId);
            category.Delete(_userId);
            return category;
        }

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var restoreCategoryResult = await _restoreCategoryHandler.Handle(new RestoreCategoryCommand(Guid.NewGuid()));

            restoreCategoryResult.IsSuccess.Should().BeFalse();
            restoreCategoryResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var restoreCategoryResult = await _restoreCategoryHandler.Handle(new RestoreCategoryCommand(Guid.Empty));

            restoreCategoryResult.IsSuccess.Should().BeFalse();
            restoreCategoryResult.Error.Should().Be("Category ID is required");
        }

        [Fact]
        public async Task Handle_CategoryTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockCategoryRepository
                .Setup(x => x.GetCategoryByIdIncludeDeletedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            var restoreCategoryResult = await _restoreCategoryHandler.Handle(new RestoreCategoryCommand(Guid.NewGuid()));

            restoreCategoryResult.IsSuccess.Should().BeFalse();
            restoreCategoryResult.Error.Should().Be("Category not found");
        }

        [Fact]
        public async Task Handle_CategoryTidakDihapus_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var category = new Category("Electronics", "electronics", _userId); // Not deleted

            _mockCategoryRepository
                .Setup(x => x.GetCategoryByIdIncludeDeletedAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            var restoreCategoryResult = await _restoreCategoryHandler.Handle(new RestoreCategoryCommand(category.Id));

            restoreCategoryResult.IsSuccess.Should().BeFalse();
            restoreCategoryResult.Error.Should().Be("Category is not deleted");
        }

        [Fact]
        public async Task Handle_CategoryDihapus_ShouldRestoreAndReturnId()
        {
            SetupAuthenticatedUser();

            var category = CreateDeletedCategory();

            _mockCategoryRepository
                .Setup(x => x.GetCategoryByIdIncludeDeletedAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            var restoreCategoryResult = await _restoreCategoryHandler.Handle(new RestoreCategoryCommand(category.Id));

            restoreCategoryResult.IsSuccess.Should().BeTrue();
            restoreCategoryResult.Value.Should().Be(category.Id);
            category.IsDeleted.Should().BeFalse();
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
