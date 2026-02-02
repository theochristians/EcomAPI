using System;

namespace EComAPI.Domain.Common.Base
{
    public abstract class SoftDeletableEntity : AuditableEntity
    {
        public DateTime? DeletedAt { get; protected set; }

        public bool IsDeleted => DeletedAt.HasValue;

        public void SoftDelete(Guid? userId)
        {
            DeletedAt = DateTime.UtcNow;
            SetUpdated(userId);
        }
    }
}