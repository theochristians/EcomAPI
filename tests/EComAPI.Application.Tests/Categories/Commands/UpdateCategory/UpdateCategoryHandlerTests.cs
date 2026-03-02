using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Categories.Commands.UpdateCategories;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Categories.Entities;

namespace EComAPI.Application.Tests.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UpdateCategoryHandler _updateCategoryHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public UpdateCategoryHandlerTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _updateCategoryHandler = new UpdateCategoryHandler(
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

            var updateCategoryResult = await _updateCategoryHandler.Handle(new UpdateCategoryCommand(Guid.NewGuid(), null, null, null, null, null));

            updateCategoryResult.IsSuccess.Should().BeFalse();
            updateCategoryResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var updateCategoryResult = await _updateCategoryHandler.Handle(new UpdateCategoryCommand(Guid.Empty, null, null, null, null, null));

            updateCategoryResult.IsSuccess.Should().BeFalse();
            updateCategoryResult.Error.Should().Be("Category ID is required");
        }

        [Fact]
        public async Task Handle_CategoryTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockCategoryRepository
                .Setup(x => x.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            var updateCategoryResult = await _updateCategoryHandler.Handle(new UpdateCategoryCommand(Guid.NewGuid(), null, null, null, null, null));

            updateCategoryResult.IsSuccess.Should().BeFalse();
            updateCategoryResult.Error.Should().Be("Category not found");
        }

        [Fact]
        public async Task Handle_SlugBaruSudahDipakai_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var category = CreateCategory();

            _mockCategoryRepository
                .Setup(x => x.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _mockCategoryRepository
                .Setup(x => x.CategoryExistsBySlugAsync("new-slug", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var updateCategoryResult = await _updateCategoryHandler.Handle(new UpdateCategoryCommand(category.Id, null, "new-slug", null, null, null));

            updateCategoryResult.IsSuccess.Should().BeFalse();
            updateCategoryResult.Error.Should().Be("Category slug already exists");
        }

        [Fact]
        public async Task Handle_ValidUpdate_ShouldReturnCategoryId()
        {
            SetupAuthenticatedUser();

            var category = CreateCategory();

            _mockCategoryRepository
                .Setup(x => x.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            var updateCategoryResult = await _updateCategoryHandler.Handle(new UpdateCategoryCommand(category.Id, "Updated Name", null, null, null, null));

            updateCategoryResult.IsSuccess.Should().BeTrue();
            updateCategoryResult.Value.Should().Be(category.Id);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
