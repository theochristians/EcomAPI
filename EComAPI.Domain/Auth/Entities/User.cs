using EComAPI.Domain.Auth.Enums;
using EComAPI.Domain.Auth.ValueObjects;
using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Auth.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; private set; }
        public EmailAddress Email { get; private set; }
        public PasswordHash Password { get; private set; }
        public string? Phone { get; private set; }

        public string? Avatar { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public Gender? Gender { get; private set; }
        public DateTime? LastLoginAt { get; private set; }

        public Guid RoleId { get; private set; }
        public Role? Role { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsEmailVerified { get; private set; }

        private readonly List<RefreshToken> _refreshTokens = new();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

        private readonly List<TokenBlacklist> _tokenBlacklists = new();
        public IReadOnlyCollection<TokenBlacklist> TokenBlacklists => _tokenBlacklists;

        private readonly List<LoginHistory> _loginHistories = new();
        public IReadOnlyCollection<LoginHistory> LoginHistories => _loginHistories;

        private readonly List<EmailVerification> _emailVerifications = new();
        public IReadOnlyCollection<EmailVerification> EmailVerifications => _emailVerifications;

        private readonly List<PasswordReset> _passwordResets = new();
        public IReadOnlyCollection<PasswordReset> PasswordResets => _passwordResets;

        private readonly List<Address> _addresses = new();
        public IReadOnlyCollection<Address> Addresses => _addresses;

        private User() { }

        public User(
            string fullName,
            EmailAddress emailAddress,
            PasswordHash password,
            Guid roleId,
            Guid createdBy,
            string? phone = null)
        {
            ValidateCreateInputs(fullName, emailAddress, password, roleId, createdBy, phone);

            FullName = EnsureFullName(fullName);
            Email = emailAddress;
            Password = password;
            RoleId = roleId;
            Phone = EnsurePhone(phone);
            IsActive = true;
            IsEmailVerified = false;

            SetCreated(createdBy);
        }

        public static User CreateForSeed(
            Guid id,
            string fullName,
            EmailAddress emailAddress,
            PasswordHash password,
            Guid roleId,
            Guid createdBy,
            string? phone = null)
        {
            Guard.AgainstEmptyGuid(id, "Id is required");
            ValidateCreateInputs(fullName, emailAddress, password, roleId, createdBy, phone);

            var user = new User
            {
                Id = id,
                FullName = EnsureFullName(fullName),
                Email = emailAddress,
                Password = password,
                RoleId = roleId,
                Phone = EnsurePhone(phone),
                IsActive = true,
                IsEmailVerified = false
            };

            user.SetCreated(createdBy);
            return user;
        }

        public void UpdateProfile(
            string fullName,
            string? phone,
            Guid updatedBy,
            string? avatar = null,
            DateTime? dateOfBirth = null,
            Gender? gender = null)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            FullName = EnsureFullName(fullName);
            Phone = EnsurePhone(phone);
            Avatar = EnsureAvatar(avatar);
            EnsureDateOfBirth(dateOfBirth);

            DateOfBirth = dateOfBirth;
            Gender = gender;

            SetUpdated(updatedBy);
        }

        public void UpdateEmail(EmailAddress newEmail, Guid updatedBy)
        {
            Guard.AgainstNull(newEmail, "Email is required");
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Email = newEmail;
            IsEmailVerified = false;
            SetUpdated(updatedBy);
        }

        public void VerifyEmail(Guid updatedBy)
        {
            if (IsEmailVerified)
                throw new DomainException("Email is already verified");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            IsEmailVerified = true;
            SetUpdated(updatedBy);
        }

        public void RecordLogin(Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            LastLoginAt = DateTime.UtcNow;
            SetUpdated(updatedBy);
        }

        private static void ValidateCreateInputs(
            string fullName,
            EmailAddress emailAddress,
            PasswordHash password,
            Guid roleId,
            Guid createdBy,
            string? phone)
        {
            Guard.AgainstNull(emailAddress, "Email is required");
            Guard.AgainstNull(password, "Password is required");
            Guard.AgainstEmptyGuid(roleId, "RoleId is required");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            EnsureFullName(fullName);
            EnsurePhone(phone);
        }

        private static string EnsureFullName(string fullName)
        {
            var name = Guard.AgainstNullOrWhiteSpace(fullName, "Full name is required");
            Guard.AgainstMinLength(name, 3, "Full name must be at least 3 characters");
            Guard.AgainstMaxLength(name, 255, "Full name cannot exceed 255 characters");
            return name;
        }

        private static string? EnsurePhone(string? phone)
        {
            return Guard.AgainstLengthIfProvided(phone, 10, 20, "Phone must be between 10-20 characters");
        }

        private static string? EnsureAvatar(string? avatar)
        {
            return Guard.AgainstMaxLengthIfProvided(avatar, 500, "Avatar URL cannot exceed 500 characters");
        }

        private static void EnsureDateOfBirth(DateTime? dateOfBirth)
        {
            if (dateOfBirth.HasValue)
            {
                if (dateOfBirth.Value > DateTime.UtcNow)
                    throw new DomainException("Date of birth cannot be in the future");

                var age = DateTime.UtcNow.Year - dateOfBirth.Value.Year;
                if (age < 13)
                    throw new DomainException("User must be at least 13 years old");
            }
        }
    }
}
