using System;
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

        public Guid RoleId { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsEmailVerified { get; private set; }

        private User() { }

        public User(
            string fullName,
            EmailAddress email,
            PasswordHash password,
            Guid roleId)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("Full name is required");

            if (roleId == Guid.Empty)
                throw new DomainException("RoleId is required");

            FullName = fullName.Trim();
            Email = email;
            Password = password;
            RoleId = roleId;

            IsActive = true;
            IsEmailVerified = false;
        }

        public void VerifyEmail()
        {
            IsEmailVerified = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}