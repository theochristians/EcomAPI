using EComAPI.Domain.Auth.Enums;
using EComAPI.Domain.Auth.ValueObjects;
using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;

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
            EmailAddress email,
            PasswordHash password,
            Guid roleId,
            Guid createdBy,
            string? phone = null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("Full name is required");

            if (fullName.Length < 3)
                throw new DomainException("Full name must be at least 3 characters");

            if (fullName.Length > 255)
                throw new DomainException("Full name cannot exceed 255 characters");

            if (email == null)
                throw new DomainException("Email is required");

            if (password == null)
                throw new DomainException("Password is required");

            if (roleId == Guid.Empty)
                throw new DomainException("RoleId is required");

            if (createdBy == Guid.Empty)
                throw new DomainException("CreatedBy is required");

            if (!string.IsNullOrWhiteSpace(phone))
            {
                if (phone.Length < 10 || phone.Length > 20)
                    throw new DomainException("Phone must be between 10-20 characters");
            }

            FullName = fullName.Trim();
            Email = email;
            Password = password;
            RoleId = roleId;
            Phone = phone?.Trim();
            IsActive = true;
            IsEmailVerified = false;
            Avatar = null;
            DateOfBirth = null;
            Gender = null;
            LastLoginAt = null;

            SetCreated(createdBy);
        }

        public static User CreateForSeed(
            Guid id,
            string fullName,
            EmailAddress email,
            PasswordHash password,
            Guid roleId,
            Guid createdBy,
            string? phone = null)
        {
            if (id == Guid.Empty)
                throw new DomainException("Id is required for seed data");

            if (string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("Full name is required");

            if (fullName.Length < 3)
                throw new DomainException("Full name must be at least 3 characters");

            if (fullName.Length > 255)
                throw new DomainException("Full name cannot exceed 255 characters");

            if (email == null)
                throw new DomainException("Email is required");

            if (password == null)
                throw new DomainException("Password is required");

            if (roleId == Guid.Empty)
                throw new DomainException("RoleId is required");

            if (createdBy == Guid.Empty)
                throw new DomainException("CreatedBy is required");

            if (!string.IsNullOrWhiteSpace(phone))
            {
                if (phone.Length < 10 || phone.Length > 20)
                    throw new DomainException("Phone must be between 10-20 characters");
            }

            var user = new User
            {
                Id = id, 
                FullName = fullName.Trim(),
                Email = email,
                Password = password,
                RoleId = roleId,
                Phone = phone?.Trim(),
                IsActive = true,
                IsEmailVerified = false,
                Avatar = null,
                DateOfBirth = null,
                Gender = null,
                LastLoginAt = null
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
            if (string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("Full name is required");
            if (fullName.Length < 3)
                throw new DomainException("Full name must be at least 3 characters");
            if (fullName.Length > 255)
                throw new DomainException("Full name cannot exceed 255 characters");
            if (!string.IsNullOrWhiteSpace(phone))
            {
                if (phone.Length < 10 || phone.Length > 20)
                    throw new DomainException("Phone must be between 10-20 characters");
            }
            if (!string.IsNullOrWhiteSpace(avatar))
            {
                if (avatar.Length > 500)
                    throw new DomainException("Avatar URL cannot exceed 500 characters");
            }
            if (dateOfBirth.HasValue)
            {
                if (dateOfBirth.Value > JakartaTime.Now)
                    throw new DomainException("Date of birth cannot be in the future");

                var age = JakartaTime.Now.Year - dateOfBirth.Value.Year;
                if (age < 13)
                    throw new DomainException("User must be at least 13 years old");
            }
            if (updatedBy == Guid.Empty)
                throw new DomainException("UpdatedBy is required");

            FullName = fullName.Trim();
            Phone = phone?.Trim();
            Avatar = avatar?.Trim();
            DateOfBirth = dateOfBirth;
            Gender = gender;

            SetUpdated(updatedBy);
        }

        public void UpdateEmail(EmailAddress newEmail, Guid updatedBy)
        {
            if (newEmail == null)
                throw new DomainException("Email is required");

            if (updatedBy == Guid.Empty)
                throw new DomainException("UpdatedBy is required");

            Email = newEmail;
            IsEmailVerified = false;
            SetUpdated(updatedBy);
        }

        public void UpdatePassword(PasswordHash newPassword, Guid updatedBy)
        {
            if (newPassword == null)
                throw new DomainException("Password is required");

            if (updatedBy == Guid.Empty)
                throw new DomainException("UpdatedBy is required");

            Password = newPassword;
            SetUpdated(updatedBy);
        }

        public void VerifyEmail(Guid updatedBy)
        {
            if (IsEmailVerified)
                throw new DomainException("Email is already verified");

            if (updatedBy == Guid.Empty)
                throw new DomainException("UpdatedBy is required");

            IsEmailVerified = true;
            SetUpdated(updatedBy);
        }

        public void RecordLogin(string? ipAddress = null, string? userAgent = null, string? deviceName = null)
        {
            LastLoginAt = JakartaTime.Now;

            var loginHistory = new LoginHistory(
                Id,
                "Success",
                Id,
                null,
                ipAddress,
                userAgent,
                deviceName);

            _loginHistories.Add(loginHistory);
        }
    }
}
