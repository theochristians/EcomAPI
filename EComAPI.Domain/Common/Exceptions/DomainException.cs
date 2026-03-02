namespace EComAPI.Domain.Common.Exceptions
{
    public class DomainException : Exception
    {
        public string? Code { get; }
        public IReadOnlyDictionary<string, string[]>? Errors { get; }

        public DomainException()
        {
        }

        public DomainException(string message)
            : base(message)
        {
        }

        public DomainException(string message, string? code)
            : base(message)
        {
            Code = code;
        }

        public DomainException(
            string message,
            string? code,
            IReadOnlyDictionary<string, string[]>? errors)
            : base(message)
        {
            Code = code;
            Errors = errors;
        }

        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
