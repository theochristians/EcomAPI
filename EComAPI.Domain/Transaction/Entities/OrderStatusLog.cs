using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Transaction.Entities
{
    public class OrderStatusLog
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public string? Note { get; private set; }
        public DateTime ChangedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public Guid? CreatedBy { get; private set; }

        private OrderStatusLog() { }

        public OrderStatusLog(
            Guid orderId,
            string status,
            Guid? createdBy = null,
            string? note = null)
        {
            Guard.AgainstEmptyGuid(orderId, "OrderId is required");

            var statusValue = Guard.AgainstNullOrWhiteSpace(status, "Status is required");
            Guard.AgainstMaxLength(statusValue, 50, "Status cannot exceed 50 characters");

            var noteValue = Guard.AgainstMaxLengthIfProvided(note, 500, "Note cannot exceed 500 characters");

            Id = Guid.NewGuid();
            OrderId = orderId;
            Status = statusValue;
            Note = noteValue;
            ChangedAt = JakartaTime.Now;
            CreatedAt = JakartaTime.Now;
            CreatedBy = createdBy;
        }
    }
}
