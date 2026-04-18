using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Transaction.Entities
{
    public class ReturnItem : BaseEntity
    {
        public Guid ReturnId { get; private set; }
        public Guid OrderItemId { get; private set; }
        public int Quantity { get; private set; }
        public string? Condition { get; private set; }
        public string? AdminNote { get; private set; }

        private ReturnItem() { }

        public ReturnItem(
            Guid returnId,
            Guid orderItemId,
            int quantity,
            Guid createdBy,
            string? condition = null)
        {
            Guard.AgainstEmptyGuid(returnId, "ReturnId is required");
            Guard.AgainstEmptyGuid(orderItemId, "OrderItemId is required");

            if (quantity <= 0)
                throw new DomainException("Quantity must be greater than zero");

            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            ReturnId = returnId;
            OrderItemId = orderItemId;
            Quantity = quantity;
            Condition = condition?.Trim();

            SetCreated(createdBy);
        }

        // ==========================================
        // BUSINESS LOGIC
        // ==========================================

        public void SetCondition(string condition, Guid updatedBy)
        {
            var conditionValue = Guard.AgainstNullOrWhiteSpace(condition, "Condition is required");
            Guard.AgainstMaxLength(conditionValue, 50, "Condition cannot exceed 50 characters");
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Condition = conditionValue;
            SetUpdated(updatedBy);
        }

        public void SetAdminNote(string adminNote, Guid updatedBy)
        {
            var noteValue = Guard.AgainstMaxLengthIfProvided(adminNote, 500, "AdminNote cannot exceed 500 characters");
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            AdminNote = noteValue;
            SetUpdated(updatedBy);
        }
    }
}
