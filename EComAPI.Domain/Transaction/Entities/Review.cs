using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Transaction.Entities
{
    public class Review : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Rating { get; private set; }
        public string? Comment { get; private set; }

        private readonly List<ReviewImage> _images = new();
        public IReadOnlyCollection<ReviewImage> Images => _images;

        private Review() { }

        public Review(
            Guid userId,
            Guid orderId,
            Guid productId,
            int rating,
            Guid createdBy,
            string? comment = null)
        {
            Guard.AgainstEmptyGuid(userId, "UserId is required");
            Guard.AgainstEmptyGuid(orderId, "OrderId is required");
            Guard.AgainstEmptyGuid(productId, "ProductId is required");

            if (rating < 1 || rating > 5)
                throw new DomainException("Rating must be between 1 and 5");

            var commentValue = Guard.AgainstMaxLengthIfProvided(comment, 2000, "Comment cannot exceed 2000 characters");

            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            UserId = userId;
            OrderId = orderId;
            ProductId = productId;
            Rating = rating;
            Comment = commentValue;

            SetCreated(createdBy);
        }

        // ==========================================
        // BUSINESS LOGIC
        // ==========================================

        public void Update(int rating, string? comment, Guid updatedBy)
        {
            if (rating < 1 || rating > 5)
                throw new DomainException("Rating must be between 1 and 5");

            var commentValue = Guard.AgainstMaxLengthIfProvided(comment, 2000, "Comment cannot exceed 2000 characters");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Rating = rating;
            Comment = commentValue;
            SetUpdated(updatedBy);
        }

        public void AddImage(ReviewImage reviewImage)
        {
            Guard.AgainstNull(reviewImage, "ReviewImage is required");

            if (_images.Count >= 5)
                throw new DomainException("Maximum 5 images per review");

            _images.Add(reviewImage);
        }

        public void RemoveImage(Guid imageId)
        {
            var image = _images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
                throw new DomainException("Image not found");

            _images.Remove(image);
        }
    }
}
