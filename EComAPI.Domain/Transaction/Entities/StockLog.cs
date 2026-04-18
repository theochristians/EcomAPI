using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Transaction.Entities
{
    /// <summary>
    /// Audit trail for stock changes. Immutable log table — not soft-deletable.
    /// </summary>
    public class StockLog
    {
        public Guid Id { get; private set; }
        public Guid ProductVariantId { get; private set; }
        public string Type { get; private set; } = string.Empty;
        public int QuantityChange { get; private set; }
        public int StockBefore { get; private set; }
        public int StockAfter { get; private set; }
        public string? ReferenceType { get; private set; }
        public Guid? ReferenceId { get; private set; }
        public string? Note { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public Guid? CreatedBy { get; private set; }

        private StockLog() { }

        public StockLog(
            Guid productVariantId,
            string type,
            int quantityChange,
            int stockBefore,
            int stockAfter,
            Guid? createdBy = null,
            string? referenceType = null,
            Guid? referenceId = null,
            string? note = null)
        {
            Guard.AgainstEmptyGuid(productVariantId, "ProductVariantId is required");

            var typeValue = Guard.AgainstNullOrWhiteSpace(type, "Type is required");
            Guard.AgainstMaxLength(typeValue, 20, "Type cannot exceed 20 characters");

            Guard.AgainstNegative(stockBefore, "StockBefore cannot be negative");
            Guard.AgainstNegative(stockAfter, "StockAfter cannot be negative");

            var noteValue = Guard.AgainstMaxLengthIfProvided(note, 500, "Note cannot exceed 500 characters");

            Id = Guid.NewGuid();
            ProductVariantId = productVariantId;
            Type = typeValue;
            QuantityChange = quantityChange;
            StockBefore = stockBefore;
            StockAfter = stockAfter;
            ReferenceType = referenceType?.Trim();
            ReferenceId = referenceId;
            Note = noteValue;
            CreatedAt = JakartaTime.Now;
            CreatedBy = createdBy;
        }
    }
}
