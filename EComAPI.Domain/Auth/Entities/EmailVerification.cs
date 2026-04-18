using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Auth.Entities
{
    public class EmailVerification : BaseEntity
    {
        public Guid UserId { get; private set; }
        public User? User { get; private set; }
        public string Code { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime? VerifiedAt { get; private set; }
        public int Attempts { get; private set; }

        private const int MaxAttempts = 5;

        public bool IsExpired => SecurityTime.UtcNow >= ExpiresAt;
        public bool IsVerified => VerifiedAt.HasValue;
        public bool IsMaxAttemptsReached => Attempts >= MaxAttempts;
        public bool IsValid => !IsExpired && !IsVerified && !IsMaxAttemptsReached;

        private EmailVerification() { }

        public EmailVerification(
        Guid userId,
        string code,
        DateTime expiresAt,
        Guid createdBy)
        {
            Guard.AgainstEmptyGuid(userId, "UserId cannot be empty");
            var normalizedCode = Guard.AgainstNullOrWhiteSpace(code, "Verification code cannot be empty");
            Guard.AgainstMinLength(normalizedCode, 6, "Verification code must be 6 characters");
            Guard.AgainstMaxLength(normalizedCode, 6, "Verification code must be 6 characters");

            if (expiresAt <= SecurityTime.UtcNow)
                throw new DomainException("Expiration date must be in the future");

            Guard.AgainstEmptyGuid(createdBy, "CreatedBy cannot be empty");

            UserId = userId;
            Code = normalizedCode;
            ExpiresAt = expiresAt;
            Attempts = 0;
            SetCreated(createdBy);
        }

        public void IncrementAttempt(Guid updatedBy)
        {
            if (IsVerified)
                throw new DomainException("Email is already verified");
            if (IsExpired)
                throw new DomainException("Verification code has expired");
            if (IsMaxAttemptsReached)
                throw new DomainException("Maximum verification attempts reached");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Attempts++;
            SetUpdated(updatedBy);
        }

        public void MarkAsVerified(Guid updatedBy)
        {
            if (IsVerified)
                throw new DomainException("Email is already verified");
            if (IsExpired)
                throw new DomainException("Verification code has expired");
            if (IsMaxAttemptsReached)
                throw new DomainException("Maximum verification attempts reached");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            VerifiedAt = SecurityTime.UtcNow;
            SetUpdated(updatedBy);
        }
    }
}