using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Domain.Auth.Exceptions
{
    public sealed class InvalidEmailAddressException : DomainException
    {
        public InvalidEmailAddressException(string email)
            : base($"The email address '{email}' is not valid.")
        {
        }
    }
}
