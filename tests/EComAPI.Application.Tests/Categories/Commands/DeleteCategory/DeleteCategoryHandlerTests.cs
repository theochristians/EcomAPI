using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Categories.Commands.DeleteCategories;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Categories.Entities;

namespace EComAPI.Application.Tests.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly DeleteCategoryHandler _deleteCategoryHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public DeleteCategoryHandlerTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _deleteCategoryHandler = new DeleteCategoryHandler(
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

        private Category CreateCategory() =>
            new Category("Electronics", "electronics", _userId);

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var deleteCategoryResult = await _deleteCategoryHandler.Handle(new DeleteCategoryCommand(Guid.NewGuid()));

            deleteCategoryResult.IsSuccess.Should().BeFalse();
            deleteCategoryResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var deleteCategoryResult = await _deleteCategoryHandler.Handle(new DeleteCategoryCommand(Guid.Empty));

            deleteCategoryResult.IsSuccess.Should().BeFalse();
            deleteCategoryResult.Error.Should().Be("Category ID is required");
        }

        [Fact]
        public async Task Handle_CategoryTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockCategoryRepository
                .Setup(x => x.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            var deleteCategoryResult = await _deleteCategoryHandler.Handle(new DeleteCategoryCommand(Guid.NewGuid()));

            deleteCategoryResult.IsSuccess.Should().BeFalse();
            deleteCategoryResult.Error.Should().Be("Category not found");
        }

        [Fact]
        public async Task Handle_CategorySudahDihapus_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var category = CreateCategory();
            category.Delete(_userId);

            _mockCategoryRepository
                .Setup(x => x.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            var deleteCategoryResult = await _deleteCategoryHandler.Handle(new DeleteCategoryCommand(category.Id));

            deleteCategoryResult.IsSuccess.Should().BeFalse();
            deleteCategoryResult.Error.Should().Be("Category is already deleted");
        }

        [Fact]
        public async Task Handle_CategoryMasihPunyaProdukAktif_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var category = CreateCategory();

            _mockCategoryRepository
                .Setup(x => x.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _mockCategoryRepository
                .Setup(x => x.HasActiveProductsAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockCategoryRepository
                .Setup(x => x.GetProductCountAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);

            var deleteCategoryResult = await _deleteCategoryHandler.Handle(new DeleteCategoryCommand(category.Id));

            deleteCategoryResult.IsSuccess.Should().BeFalse();
            deleteCategoryResult.Error.Should().Contain("5 active product(s)");
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldDeleteAndReturnId()
        {
            SetupAuthenticatedUser();

            var category = CreateCategory();

            _mockCategoryRepository
                .Setup(x => x.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _mockCategoryRepository
                .Setup(x => x.HasActiveProductsAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCategoryRepository
                .Setup(x => x.HasActiveSubcategoriesAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var deleteCategoryResult = await _deleteCategoryHandler.Handle(new DeleteCategoryCommand(category.Id));

            deleteCategoryResult.IsSuccess.Should().BeTrue();
            deleteCategoryResult.Value.Should().Be(category.Id);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
