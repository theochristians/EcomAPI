using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Transaction.Entities
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid? AddressId { get; private set; }
        public string OrderNumber { get; private set; } = string.Empty;
        public string ShippingRecipientName { get; private set; } = string.Empty;
        public string ShippingPhone { get; private set; } = string.Empty;
        public string ShippingFullAddress { get; private set; } = string.Empty;
        public string ShippingCity { get; private set; } = string.Empty;
        public string ShippingPostalCode { get; private set; } = string.Empty;
        public decimal TotalAmount { get; private set; }
        public decimal ShippingCost { get; private set; }
        public Guid? CouponId { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal FinalAmount { get; private set; }
        public string Status { get; private set; } = OrderStatus.PendingPayment;
        public string? Courier { get; private set; }
        public string? TrackingNumber { get; private set; }
        public string? CustomerNote { get; private set; }
        public string? AdminNote { get; private set; }

        private readonly List<OrderItem> _items = new();
        public IReadOnlyCollection<OrderItem> Items => _items;

        private Order() { }

        public Order(
            Guid userId,
            string orderNumber,
            string shippingRecipientName,
            string shippingPhone,
            string shippingFullAddress,
            string shippingCity,
            string shippingPostalCode,
            decimal totalAmount,
            decimal shippingCost,
            decimal finalAmount,
            Guid createdBy,
            Guid? addressId = null,
            Guid? couponId = null,
            decimal discountAmount = 0,
            string? customerNote = null)
        {
            Guard.AgainstEmptyGuid(userId, "UserId is required");

            var orderNumberValue = Guard.AgainstNullOrWhiteSpace(orderNumber, "OrderNumber is required");
            Guard.AgainstMaxLength(orderNumberValue, 50, "OrderNumber cannot exceed 50 characters");

            var recipientValue = Guard.AgainstNullOrWhiteSpace(shippingRecipientName, "ShippingRecipientName is required");
            Guard.AgainstMaxLength(recipientValue, 255, "ShippingRecipientName cannot exceed 255 characters");

            var phoneValue = Guard.AgainstNullOrWhiteSpace(shippingPhone, "ShippingPhone is required");
            Guard.AgainstMaxLength(phoneValue, 20, "ShippingPhone cannot exceed 20 characters");

            var fullAddressValue = Guard.AgainstNullOrWhiteSpace(shippingFullAddress, "ShippingFullAddress is required");
            var cityValue = Guard.AgainstNullOrWhiteSpace(shippingCity, "ShippingCity is required");
            Guard.AgainstMaxLength(cityValue, 100, "ShippingCity cannot exceed 100 characters");

            var postalCodeValue = Guard.AgainstNullOrWhiteSpace(shippingPostalCode, "ShippingPostalCode is required");
            Guard.AgainstMaxLength(postalCodeValue, 10, "ShippingPostalCode cannot exceed 10 characters");

            Guard.AgainstNegative(totalAmount, "TotalAmount cannot be negative");
            Guard.AgainstNegative(shippingCost, "ShippingCost cannot be negative");
            Guard.AgainstNegative(discountAmount, "DiscountAmount cannot be negative");
            Guard.AgainstNegative(finalAmount, "FinalAmount cannot be negative");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            UserId = userId;
            AddressId = addressId;
            OrderNumber = orderNumberValue;
            ShippingRecipientName = recipientValue;
            ShippingPhone = phoneValue;
            ShippingFullAddress = fullAddressValue;
            ShippingCity = cityValue;
            ShippingPostalCode = postalCodeValue;
            TotalAmount = totalAmount;
            ShippingCost = shippingCost;
            CouponId = couponId;
            DiscountAmount = discountAmount;
            FinalAmount = finalAmount;
            Status = OrderStatus.PendingPayment;
            CustomerNote = customerNote;

            SetCreated(createdBy);
        }

        // ==========================================
        // STATUS TRANSITIONS
        // ==========================================

        public void MarkAsPaid(Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");
            Status = OrderStatus.Paid;
            SetUpdated(updatedBy);
        }

        public void MarkAsProcessing(Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");
            Status = OrderStatus.Processing;
            SetUpdated(updatedBy);
        }

        public void MarkAsShipped(string courier, string trackingNumber, Guid updatedBy)
        {
            var courierValue = Guard.AgainstNullOrWhiteSpace(courier, "Courier is required");
            var trackingValue = Guard.AgainstNullOrWhiteSpace(trackingNumber, "TrackingNumber is required");
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Courier = courierValue;
            TrackingNumber = trackingValue;
            Status = OrderStatus.Shipped;
            SetUpdated(updatedBy);
        }

        public void MarkAsCompleted(Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");
            Status = OrderStatus.Completed;
            SetUpdated(updatedBy);
        }

        public void Cancel(Guid updatedBy, string? adminNote = null)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");
            Status = OrderStatus.Cancelled;
            if (!string.IsNullOrWhiteSpace(adminNote))
                AdminNote = adminNote;
            SetUpdated(updatedBy);
        }

        public void SetAdminNote(string adminNote, Guid updatedBy)
        {
            AdminNote = adminNote;
            SetUpdated(updatedBy);
        }

        public void AddItem(OrderItem item)
        {
            Guard.AgainstNull(item, "OrderItem is required");
            _items.Add(item);
        }
    }
}
