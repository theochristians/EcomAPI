namespace EComAPI.Domain.Transaction.Entities
{

    public class CouponUsage
    {
        public Guid Id { get; private set; }
        public Guid CouponId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid OrderId { get; private set; }
        public DateTime UsedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private CouponUsage() { }

        public CouponUsage(Guid couponId, Guid userId, Guid orderId)
        {
            if (couponId == Guid.Empty) throw new ArgumentException("CouponId is required", nameof(couponId));
            if (userId == Guid.Empty)   throw new ArgumentException("UserId is required",   nameof(userId));
            if (orderId == Guid.Empty)  throw new ArgumentException("OrderId is required",  nameof(orderId));

            Id = Guid.NewGuid();
            CouponId = couponId;
            UserId = userId;
            OrderId = orderId;
            UsedAt = JakartaTime.Now;
            CreatedAt = JakartaTime.Now;
        }
    }
}
