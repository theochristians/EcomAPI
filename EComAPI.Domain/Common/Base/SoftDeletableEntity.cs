using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Common.Base
{
    public abstract class SoftDeletableEntity : AuditableEntity
    {
        public DateTime? DeletedAt { get; protected set; }
        public Guid? DeletedBy { get; protected set; }

        public bool IsDeleted => DeletedAt.HasValue;

        public virtual void Delete(Guid deletedBy)
        {
            if (IsDeleted)
                throw new DomainException("Entity is already deleted");

            Guard.AgainstEmptyGuid(deletedBy, "DeletedBy is required");

            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }

        public virtual void Restore()
        {
            if (!IsDeleted)
                throw new DomainException("Entity is not deleted");

            DeletedAt = null;
            DeletedBy = null;
        }
    }
}