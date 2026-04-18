using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Auth.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; private set; }
        public User? User { get; private set; }
        public string TokenHash { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? RevokeReason { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }
        public string? DeviceName { get; private set; }

        public bool IsExpired => SecurityTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt.HasValue;
        public bool IsActive => !IsRevoked && !IsExpired;

        private RefreshToken() { }

        public RefreshToken(
            Guid userId,
            string tokenHash,
            DateTime expiresAt,
            Guid createdBy,
            string? ipAddress = null,
            string? userAgent = null,
            string? deviceName = null)
        {
            Guard.AgainstEmptyGuid(userId, "UserId is required");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            var tokenHashValue = Guard.AgainstNullOrWhiteSpace(tokenHash, "Token hash is required");
            Guard.AgainstMaxLength(tokenHashValue, 500, "Token hash cannot exceed 500 characters");

            if (expiresAt <= SecurityTime.UtcNow)
                throw new DomainException("Expiration date must be in the future");

            UserId = userId;
            TokenHash = tokenHashValue;
            ExpiresAt = expiresAt;
            IpAddress = Guard.AgainstMaxLengthIfProvided(ipAddress, 64, "IP address cannot exceed 64 characters");
            UserAgent = Guard.AgainstMaxLengthIfProvided(userAgent, 500, "User agent cannot exceed 500 characters");
            DeviceName = Guard.AgainstMaxLengthIfProvided(deviceName, 200, "Device name cannot exceed 200 characters");

            SetCreated(createdBy);
        }

        public void Revoke(string reason, Guid updatedBy)
        {
            if (IsRevoked)
                throw new DomainException("Token is already revoked");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            var reasonValue = Guard.AgainstNullOrWhiteSpace(reason, "Revoke reason is required");
            Guard.AgainstMaxLength(reasonValue, 500, "Revoke reason cannot exceed 500 characters");

            RevokedAt = SecurityTime.UtcNow;
            RevokeReason = reasonValue;
            SetUpdated(updatedBy);
        }

        public void UpdateDeviceInfo(string? ipAddress, string? userAgent, string? deviceName, Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            IpAddress = Guard.AgainstMaxLengthIfProvided(ipAddress, 64, "IP address cannot exceed 64 characters");
            UserAgent = Guard.AgainstMaxLengthIfProvided(userAgent, 500, "User agent cannot exceed 500 characters");
            DeviceName = Guard.AgainstMaxLengthIfProvided(deviceName, 200, "Device name cannot exceed 200 characters");

            SetUpdated(updatedBy);
        }
    }
}
