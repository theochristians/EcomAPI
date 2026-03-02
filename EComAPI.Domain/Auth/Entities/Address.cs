using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Auth.Entities
{
    public class Address : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Label { get; private set; }
        public string RecipientName { get; private set; }
        public string RecipientPhone { get; private set; }
        public string FullAddress { get; private set; }
        public string City { get; private set; }
        public string Province { get; private set; }
        public string PostalCode { get; private set; }
        public bool IsDefault { get; private set; }

        public User? User { get; private set; }

        private Address() { }

        public Address(
            Guid userId,
            string label,
            string recipientName,
            string recipientPhone,
            string fullAddress,
            string city,
            string province,
            string postalCode,
            bool isDefault,
            Guid createdBy)
        {
            Guard.AgainstEmptyGuid(userId, "UserId cannot be empty");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy cannot be empty");

            UserId = userId;
            Label = EnsureLabel(label);
            RecipientName = EnsureRecipientName(recipientName);
            RecipientPhone = EnsureRecipientPhone(recipientPhone);
            FullAddress = EnsureFullAddress(fullAddress);
            City = EnsureCity(city);
            Province = EnsureProvince(province);
            PostalCode = EnsurePostalCode(postalCode);
            IsDefault = isDefault;

            SetCreated(createdBy);
        }

        public void Update(
            string label,
            string recipientName,
            string recipientPhone,
            string fullAddress,
            string city,
            string province,
            string postalCode,
            Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy cannot be empty");

            Label = EnsureLabel(label);
            RecipientName = EnsureRecipientName(recipientName);
            RecipientPhone = EnsureRecipientPhone(recipientPhone);
            FullAddress = EnsureFullAddress(fullAddress);
            City = EnsureCity(city);
            Province = EnsureProvince(province);
            PostalCode = EnsurePostalCode(postalCode);

            SetUpdated(updatedBy);
        }

        public void SetAsDefault(Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");
            if (IsDefault) return;

            IsDefault = true;
            SetUpdated(updatedBy);
        }

        public void UnsetDefault(Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");
            if (!IsDefault) return;

            IsDefault = false;
            SetUpdated(updatedBy);
        }

        private static string EnsureLabel(string label)
        {
            var value = Guard.AgainstNullOrWhiteSpace(label, "Label cannot be empty");
            Guard.AgainstMaxLength(value, 50, "Label cannot exceed 50 characters");
            return value;
        }

        private static string EnsureRecipientName(string recipientName)
        {
            var value = Guard.AgainstNullOrWhiteSpace(recipientName, "Recipient name cannot be empty");
            Guard.AgainstMaxLength(value, 100, "Recipient name cannot exceed 100 characters");
            return value;
        }

        private static string EnsureRecipientPhone(string recipientPhone)
        {
            var value = Guard.AgainstNullOrWhiteSpace(recipientPhone, "Recipient phone cannot be empty");
            Guard.AgainstMaxLength(value, 20, "Recipient phone cannot exceed 20 characters");
            return value;
        }

        private static string EnsureFullAddress(string fullAddress)
        {
            var value = Guard.AgainstNullOrWhiteSpace(fullAddress, "Full address cannot be empty");
            Guard.AgainstMaxLength(value, 500, "Full address cannot exceed 500 characters");
            return value;
        }

        private static string EnsureCity(string city)
        {
            var value = Guard.AgainstNullOrWhiteSpace(city, "City cannot be empty");
            Guard.AgainstMaxLength(value, 100, "City cannot exceed 100 characters");
            return value;
        }

        private static string EnsureProvince(string province)
        {
            var value = Guard.AgainstNullOrWhiteSpace(province, "Province cannot be empty");
            Guard.AgainstMaxLength(value, 100, "Province cannot exceed 100 characters");
            return value;
        }

        private static string EnsurePostalCode(string postalCode)
        {
            var value = Guard.AgainstNullOrWhiteSpace(postalCode, "Postal code cannot be empty");
            Guard.AgainstMaxLength(value, 10, "Postal code cannot exceed 10 characters");
            return value;
        }
    }
}
