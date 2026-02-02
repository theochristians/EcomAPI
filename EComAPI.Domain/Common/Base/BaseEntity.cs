using System;

namespace EComAPI.Domain.Common.Base
{
    public abstract class BaseEntity : SoftDeletableEntity
    {
        public Guid Id { get; protected set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
        }
    }
}