using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Auth.Entities
{
    public class PasswordReset : BaseEntity
    {
        public Guid UserId { get; private set; }
        public User? User { get; private set; }
        public string TokenHash { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime? UsedAt { get; private set; }

        public bool IsExpired => SecurityTime.UtcNow >= ExpiresAt;
        public bool IsUsed => UsedAt.HasValue;
        public bool IsValid => !IsExpired && !IsUsed;

        private PasswordReset() { }

        public PasswordReset(
            Guid userId,
            string tokenHash,
            DateTime expiresAt,
            Guid createdBy)
        {
            Guard.AgainstEmptyGuid(userId, "UserId is required");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            var tokenHashValue = Guard.AgainstNullOrWhiteSpace(tokenHash, "TokenHash is required");
            Guard.AgainstMaxLength(tokenHashValue, 500, "TokenHash cannot exceed 500 characters");

            if (expiresAt <= SecurityTime.UtcNow)
                throw new DomainException("Expiration date must be in the future");

            UserId = userId;
            TokenHash = tokenHashValue;
            ExpiresAt = expiresAt;

            SetCreated(createdBy);
        }

        public void MarkAsUsed(Guid updatedBy)
        {
            if (IsUsed)
                throw new DomainException("Password reset token has already been used");

            if (IsExpired)
                throw new DomainException("Password reset token has expired");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            UsedAt = SecurityTime.UtcNow;
            SetUpdated(updatedBy);
        }
    }
}
