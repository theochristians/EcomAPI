using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Transaction.Entities
{
    /// <summary>
    /// Image attached to a return request as evidence. Not soft-deletable; immutable once created.
    /// </summary>
    public class ReturnImage
    {
        public Guid Id { get; private set; }
        public Guid ReturnId { get; private set; }
        public string ImageUrl { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private ReturnImage() { }

        public ReturnImage(
            Guid returnId,
            string imageUrl,
            string? description = null)
        {
            Guard.AgainstEmptyGuid(returnId, "ReturnId is required");

            var imageUrlValue = Guard.AgainstNullOrWhiteSpace(imageUrl, "ImageUrl is required");
            Guard.AgainstMaxLength(imageUrlValue, 500, "ImageUrl cannot exceed 500 characters");

            var descriptionValue = Guard.AgainstMaxLengthIfProvided(description, 255, "Description cannot exceed 255 characters");

            Id = Guid.NewGuid();
            ReturnId = returnId;
            ImageUrl = imageUrlValue;
            Description = descriptionValue;
            CreatedAt = JakartaTime.Now;
        }
    }
}
