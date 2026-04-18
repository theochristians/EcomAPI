using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Shopping.Entities
{
    public class Cart : BaseEntity
    {
        public Guid UserId { get; private set; }

        private readonly List<CartItem> _items = new();
        public IReadOnlyCollection<CartItem> Items => _items;

        private Cart() { }

        public Cart(Guid userId, Guid createdBy)
        {
            Guard.AgainstEmptyGuid(userId, "UserId is required");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            UserId = userId;
            SetCreated(createdBy);
        }

        public void AddItem(CartItem item)
        {
            Guard.AgainstNull(item, "CartItem is required");

            var existing = _items.FirstOrDefault(i => i.ProductVariantId == item.ProductVariantId && i.DeletedAt == null);
            if (existing != null)
                existing.IncreaseQuantity(item.Quantity);
            else
                _items.Add(item);
        }

        public void Clear(Guid updatedBy)
        {
            foreach (var item in _items.Where(i => i.DeletedAt == null))
                item.Delete(updatedBy);
        }
    }
}
