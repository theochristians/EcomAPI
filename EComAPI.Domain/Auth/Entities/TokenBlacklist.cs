using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Auth.Entities
{
    public class TokenBlacklist : BaseEntity
    {
        public Guid UserId { get; private set; }
        public User? User { get; private set; }
        public string TokenHash { get; private set; }
        public string Reason { get; private set; }
        public DateTime ExpiresAt { get; private set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        private TokenBlacklist() { }

        public TokenBlacklist(
            Guid userId,
            string tokenHash,
            string reason,
            DateTime expiresAt,
            Guid createdBy)
        {
            Guard.AgainstEmptyGuid(userId, "UserId is required");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            var tokenHashValue = Guard.AgainstNullOrWhiteSpace(tokenHash, "Token hash is required");
            Guard.AgainstMaxLength(tokenHashValue, 500, "Token hash cannot exceed 500 characters");

            var reasonValue = Guard.AgainstNullOrWhiteSpace(reason, "Reason is required");
            Guard.AgainstMaxLength(reasonValue, 500, "Reason cannot exceed 500 characters");

            if (expiresAt <= DateTime.UtcNow)
                throw new DomainException("Expiration date must be in the future");

            UserId = userId;
            TokenHash = tokenHashValue;
            Reason = reasonValue;
            ExpiresAt = expiresAt;

            SetCreated(createdBy);
        }
    }
}
