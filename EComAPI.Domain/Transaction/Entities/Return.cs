using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Transaction.Entities
{
    public class Return : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public Guid UserId { get; private set; }
        public string ReturnNumber { get; private set; } = string.Empty;
        public string Reason { get; private set; } = string.Empty;
        public string Status { get; private set; } = ReturnStatus.Requested;
        public DateTime RequestedAt { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public Guid? ApprovedBy { get; private set; }
        public decimal RefundAmount { get; private set; }
        public string? BankName { get; private set; }
        public string? BankAccountNumber { get; private set; }
        public string? AccountHolderName { get; private set; }
        public DateTime? RefundDate { get; private set; }

        private readonly List<ReturnItem> _items = new();
        public IReadOnlyCollection<ReturnItem> Items => _items;

        private readonly List<ReturnImage> _images = new();
        public IReadOnlyCollection<ReturnImage> Images => _images;

        private Return() { }

        public Return(
            Guid orderId,
            Guid userId,
            string returnNumber,
            string reason,
            Guid createdBy,
            string? bankName = null,
            string? bankAccountNumber = null,
            string? accountHolderName = null)
        {
            Guard.AgainstEmptyGuid(orderId, "OrderId is required");
            Guard.AgainstEmptyGuid(userId, "UserId is required");

            var returnNumberValue = Guard.AgainstNullOrWhiteSpace(returnNumber, "ReturnNumber is required");
            Guard.AgainstMaxLength(returnNumberValue, 50, "ReturnNumber cannot exceed 50 characters");

            var reasonValue = Guard.AgainstNullOrWhiteSpace(reason, "Reason is required");

            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            OrderId = orderId;
            UserId = userId;
            ReturnNumber = returnNumberValue;
            Reason = reasonValue;
            Status = ReturnStatus.Requested;
            RequestedAt = JakartaTime.Now;
            RefundAmount = 0;
            BankName = bankName?.Trim();
            BankAccountNumber = bankAccountNumber?.Trim();
            AccountHolderName = accountHolderName?.Trim();

            SetCreated(createdBy);
        }

        // ==========================================
        // STATUS TRANSITIONS
        // ==========================================

        public void Approve(decimal refundAmount, Guid approvedBy)
        {
            if (Status != ReturnStatus.Requested)
                throw new DomainException("Only requested returns can be approved");

            Guard.AgainstNegative(refundAmount, "RefundAmount cannot be negative");
            Guard.AgainstEmptyGuid(approvedBy, "ApprovedBy is required");

            Status = ReturnStatus.Approved;
            RefundAmount = refundAmount;
            ApprovedAt = JakartaTime.Now;
            ApprovedBy = approvedBy;
            SetUpdated(approvedBy);
        }

        public void Reject(Guid rejectedBy)
        {
            if (Status != ReturnStatus.Requested)
                throw new DomainException("Only requested returns can be rejected");

            Guard.AgainstEmptyGuid(rejectedBy, "RejectedBy is required");

            Status = ReturnStatus.Rejected;
            SetUpdated(rejectedBy);
        }

        public void MarkAsRefunded(Guid updatedBy)
        {
            if (Status != ReturnStatus.Approved)
                throw new DomainException("Only approved returns can be marked as refunded");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Status = ReturnStatus.Refunded;
            RefundDate = JakartaTime.Now;
            SetUpdated(updatedBy);
        }

        public void AddItem(ReturnItem returnItem)
        {
            Guard.AgainstNull(returnItem, "ReturnItem is required");
            _items.Add(returnItem);
        }

        public void AddImage(ReturnImage returnImage)
        {
            Guard.AgainstNull(returnImage, "ReturnImage is required");
            _images.Add(returnImage);
        }
    }
}
