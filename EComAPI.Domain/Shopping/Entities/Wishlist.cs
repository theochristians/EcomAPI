using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Shopping.Entities
{
    public class Wishlist : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid ProductId { get; private set; }

        private Wishlist() { }

        public Wishlist(Guid userId, Guid productId, Guid createdBy)
        {
            Guard.AgainstEmptyGuid(userId, "UserId is required");
            Guard.AgainstEmptyGuid(productId, "ProductId is required");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            UserId = userId;
            ProductId = productId;
            SetCreated(createdBy);
        }
    }
}
