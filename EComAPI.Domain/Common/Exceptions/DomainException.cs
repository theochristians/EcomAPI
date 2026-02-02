using System;

namespace EComAPI.Domain.Common.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException()
        {
        }
        public DomainException(string message)
            : base(message)
        {
        }
        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}