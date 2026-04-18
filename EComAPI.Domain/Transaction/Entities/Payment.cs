using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Transaction.Entities
{
    public class Payment : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public string? PaymentMethod { get; private set; }
        public decimal Amount { get; private set; }
        public string Status { get; private set; } = PaymentStatus.Pending;
        public string? ProofImageUrl { get; private set; }
        public string? AdminNote { get; private set; }
        public DateTime? ConfirmedAt { get; private set; }
        public Guid? ConfirmedBy { get; private set; }

        private Payment() { }

        public Payment(
            Guid orderId,
            decimal amount,
            Guid createdBy,
            string? paymentMethod = null)
        {
            Guard.AgainstEmptyGuid(orderId, "OrderId is required");
            Guard.AgainstNegative(amount, "Amount cannot be negative");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            OrderId = orderId;
            Amount = amount;
            PaymentMethod = paymentMethod;
            Status = PaymentStatus.Pending;

            SetCreated(createdBy);
        }

        // ==========================================
        // STATUS TRANSITIONS
        // ==========================================

        public void SetPaymentMethod(string paymentMethod, Guid updatedBy)
        {
            var paymentMethodValue = Guard.AgainstNullOrWhiteSpace(paymentMethod, "PaymentMethod is required");
            Guard.AgainstMaxLength(paymentMethodValue, 50, "PaymentMethod cannot exceed 50 characters");
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            PaymentMethod = paymentMethodValue;
            SetUpdated(updatedBy);
        }

        public void SetProofImage(string proofImageUrl, Guid updatedBy)
        {
            var proofValue = Guard.AgainstNullOrWhiteSpace(proofImageUrl, "ProofImageUrl is required");
            Guard.AgainstMaxLength(proofValue, 500, "ProofImageUrl cannot exceed 500 characters");
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            ProofImageUrl = proofValue;
            SetUpdated(updatedBy);
        }

        public void Confirm(Guid confirmedBy, string? adminNote = null)
        {
            Guard.AgainstEmptyGuid(confirmedBy, "ConfirmedBy is required");

            Status = PaymentStatus.Confirmed;
            ConfirmedAt = JakartaTime.Now;
            ConfirmedBy = confirmedBy;

            if (!string.IsNullOrWhiteSpace(adminNote))
                AdminNote = adminNote;

            SetUpdated(confirmedBy);
        }

        public void Reject(Guid updatedBy, string? adminNote = null)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Status = PaymentStatus.Rejected;

            if (!string.IsNullOrWhiteSpace(adminNote))
                AdminNote = adminNote;

            SetUpdated(updatedBy);
        }
    }
}
