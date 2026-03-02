using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Auth.Entities
{
    public class LoginHistory : BaseEntity
    {
        private const string SuccessStatus = "Success";
        private const string FailedStatus = "Failed";

        public Guid UserId { get; private set; }
        public User? User { get; private set; }
        public string Status { get; private set; }
        public string? FailureReason { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }
        public string? DeviceName { get; private set; }

        private LoginHistory() { }

        public LoginHistory(
            Guid userId,
            string status,
            Guid createdBy,
            string? failureReason = null,
            string? ipAddress = null,
            string? userAgent = null,
            string? deviceName = null)
        {
            Guard.AgainstEmptyGuid(userId, "UserId is required");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            var normalizedStatus = NormalizeStatus(status);
            var failureReasonValue = Guard.AgainstMaxLengthIfProvided(
                failureReason,
                500,
                "Failure reason cannot exceed 500 characters");

            if (normalizedStatus == FailedStatus && string.IsNullOrWhiteSpace(failureReasonValue))
                throw new DomainException("Failure reason is required when status is Failed");

            UserId = userId;
            Status = normalizedStatus;
            FailureReason = normalizedStatus == SuccessStatus ? null : failureReasonValue;
            IpAddress = Guard.AgainstMaxLengthIfProvided(ipAddress, 64, "IP address cannot exceed 64 characters");
            UserAgent = Guard.AgainstMaxLengthIfProvided(userAgent, 500, "User agent cannot exceed 500 characters");
            DeviceName = Guard.AgainstMaxLengthIfProvided(deviceName, 200, "Device name cannot exceed 200 characters");

            SetCreated(createdBy);
        }

        private static string NormalizeStatus(string status)
        {
            var value = Guard.AgainstNullOrWhiteSpace(status, "Status is required");

            if (value.Equals(SuccessStatus, StringComparison.OrdinalIgnoreCase))
                return SuccessStatus;

            if (value.Equals(FailedStatus, StringComparison.OrdinalIgnoreCase))
                return FailedStatus;

            throw new DomainException($"Status must be one of: {SuccessStatus}, {FailedStatus}");
        }
    }
}
