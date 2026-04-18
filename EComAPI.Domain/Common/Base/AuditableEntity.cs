using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Common.Base
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; protected set; }
        public Guid CreatedBy { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public Guid? UpdatedBy { get; protected set; }

        protected AuditableEntity()
        {
            CreatedAt = JakartaTime.Now;
        }

        protected void SetCreated(Guid userId)
        {
            Guard.AgainstEmptyGuid(userId, "CreatedBy is required");

            CreatedAt = JakartaTime.Now;
            CreatedBy = userId;
        }

        protected void SetUpdated(Guid userId)
        {
            Guard.AgainstEmptyGuid(userId, "UpdatedBy is required");

            UpdatedAt = JakartaTime.Now;
            UpdatedBy = userId;
        }
    }
}