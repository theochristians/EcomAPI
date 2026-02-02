using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Domain.Auth.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; private set; }

        private Role() { }

        public Role(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Role name is required");

            Name = name.Trim();
        }
    }
}