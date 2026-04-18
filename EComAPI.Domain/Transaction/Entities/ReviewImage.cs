using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Transaction.Entities
{
    /// <summary>
    /// Image attached to a review. Not soft-deletable; immutable once created.
    /// </summary>
    public class ReviewImage
    {
        public Guid Id { get; private set; }
        public Guid ReviewId { get; private set; }
        public string ImageUrl { get; private set; } = string.Empty;
        public int DisplayOrder { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private ReviewImage() { }

        public ReviewImage(
            Guid reviewId,
            string imageUrl,
            int displayOrder = 0)
        {
            Guard.AgainstEmptyGuid(reviewId, "ReviewId is required");

            var imageUrlValue = Guard.AgainstNullOrWhiteSpace(imageUrl, "ImageUrl is required");
            Guard.AgainstMaxLength(imageUrlValue, 500, "ImageUrl cannot exceed 500 characters");

            Guard.AgainstNegative(displayOrder, "DisplayOrder cannot be negative");

            Id = Guid.NewGuid();
            ReviewId = reviewId;
            ImageUrl = imageUrlValue;
            DisplayOrder = displayOrder;
            CreatedAt = JakartaTime.Now;
        }
    }
}
