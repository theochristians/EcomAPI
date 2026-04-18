using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Transaction.Commands.ReviewCommands.CreateReview;
using EComAPI.Application.Transaction.Commands.ReviewCommands.UpdateReview;
using EComAPI.Application.Transaction.Commands.ReviewCommands.DeleteReview;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Tests.Transaction.Commands.ReviewCommands
{
    public class ReviewHandlerTests
    {
        private readonly Mock<IReviewRepository> _mockReviewRepository;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _orderId = Guid.NewGuid();
        private readonly Guid _productId = Guid.NewGuid();
        private readonly Guid _reviewId = Guid.NewGuid();

        public ReviewHandlerTests()
        {
            _mockReviewRepository = new Mock<IReviewRepository>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        // =========================================================
        // CREATE REVIEW
        // =========================================================

        [Fact]
        public async Task CreateReview_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var handler = new CreateReviewHandler(
                _mockReviewRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new CreateReviewCommand(_orderId, _productId, 5, "Great"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task CreateReview_SudahReview_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockReviewRepository
                .Setup(x => x.UserHasReviewedOrderProductAsync(_userId, _orderId, _productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new CreateReviewHandler(
                _mockReviewRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new CreateReviewCommand(_orderId, _productId, 5, "Great"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("You have already reviewed this product");
        }

        [Fact]
        public async Task CreateReview_BelumBeli_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockReviewRepository
                .Setup(x => x.UserHasReviewedOrderProductAsync(_userId, _orderId, _productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockOrderRepository
                .Setup(x => x.UserHasCompletedOrderForProductInOrderAsync(_userId, _orderId, _productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var handler = new CreateReviewHandler(
                _mockReviewRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new CreateReviewCommand(_orderId, _productId, 5, "Great"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("You can only review products from your completed order");
        }

        [Fact]
        public async Task CreateReview_TerlaluBanyakGambar_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockReviewRepository
                .Setup(x => x.UserHasReviewedOrderProductAsync(_userId, _orderId, _productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockOrderRepository
                .Setup(x => x.UserHasCompletedOrderForProductInOrderAsync(_userId, _orderId, _productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var images = Enumerable.Range(1, 6)
                .Select(i => new CreateReviewImageDto($"https://cdn.example.com/img{i}.jpg", i))
                .ToList();

            var handler = new CreateReviewHandler(
                _mockReviewRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new CreateReviewCommand(_orderId, _productId, 5, "Great", images));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Maximum 5 images per review");
        }

        [Fact]
        public async Task CreateReview_Valid_ShouldReturnSuccess()
        {
            SetupAuthenticatedUser();

            _mockReviewRepository
                .Setup(x => x.UserHasReviewedOrderProductAsync(_userId, _orderId, _productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockOrderRepository
                .Setup(x => x.UserHasCompletedOrderForProductInOrderAsync(_userId, _orderId, _productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new CreateReviewHandler(
                _mockReviewRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new CreateReviewCommand(_orderId, _productId, 5, "Great product!"));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateReview_RatingTidakValid_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockReviewRepository
                .Setup(x => x.UserHasReviewedOrderProductAsync(_userId, _orderId, _productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockOrderRepository
                .Setup(x => x.UserHasCompletedOrderForProductInOrderAsync(_userId, _orderId, _productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new CreateReviewHandler(
                _mockReviewRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new CreateReviewCommand(_orderId, _productId, 6, "Great"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Rating must be between 1 and 5");
        }

        // =========================================================
        // UPDATE REVIEW
        // =========================================================

        [Fact]
        public async Task UpdateReview_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var handler = new UpdateReviewHandler(
                _mockReviewRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new UpdateReviewCommand(_reviewId, 4, "Updated"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task UpdateReview_ReviewTidakAda_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockReviewRepository
                .Setup(x => x.GetReviewByIdAsync(_reviewId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Review?)null);

            var handler = new UpdateReviewHandler(
                _mockReviewRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new UpdateReviewCommand(_reviewId, 4, "Updated"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Review not found");
        }

        [Fact]
        public async Task UpdateReview_BukanPemilik_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var otherUserId = Guid.NewGuid();
            var review = new Review(otherUserId, _orderId, _productId, 5, otherUserId, "Original");

            _mockReviewRepository
                .Setup(x => x.GetReviewByIdAsync(review.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(review);

            var handler = new UpdateReviewHandler(
                _mockReviewRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new UpdateReviewCommand(review.Id, 4, "Hacked"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Review not found");
        }

        [Fact]
        public async Task UpdateReview_Valid_ShouldReturnSuccess()
        {
            SetupAuthenticatedUser();

            var review = new Review(_userId, _orderId, _productId, 5, _userId, "Original");

            _mockReviewRepository
                .Setup(x => x.GetReviewByIdAsync(review.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(review);

            var handler = new UpdateReviewHandler(
                _mockReviewRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new UpdateReviewCommand(review.Id, 4, "Updated"));

            result.IsSuccess.Should().BeTrue();
        }

        // =========================================================
        // DELETE REVIEW
        // =========================================================

        [Fact]
        public async Task DeleteReview_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var handler = new DeleteReviewHandler(
                _mockReviewRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new DeleteReviewCommand(_reviewId));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task DeleteReview_ReviewTidakAda_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockReviewRepository
                .Setup(x => x.GetReviewByIdAsync(_reviewId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Review?)null);

            var handler = new DeleteReviewHandler(
                _mockReviewRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new DeleteReviewCommand(_reviewId));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Review not found");
        }

        [Fact]
        public async Task DeleteReview_BukanPemilik_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var otherUserId = Guid.NewGuid();
            var review = new Review(otherUserId, _orderId, _productId, 5, otherUserId, "Other's review");

            _mockReviewRepository
                .Setup(x => x.GetReviewByIdAsync(review.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(review);

            var handler = new DeleteReviewHandler(
                _mockReviewRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new DeleteReviewCommand(review.Id));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Review not found");
        }

        [Fact]
        public async Task DeleteReview_Valid_ShouldReturnSuccess()
        {
            SetupAuthenticatedUser();

            var review = new Review(_userId, _orderId, _productId, 5, _userId, "My review");

            _mockReviewRepository
                .Setup(x => x.GetReviewByIdAsync(review.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(review);

            var handler = new DeleteReviewHandler(
                _mockReviewRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new DeleteReviewCommand(review.Id));

            result.IsSuccess.Should().BeTrue();
        }
    }
}
