namespace EComAPI.Domain.Transaction.Entities
{
    public static class OrderStatus
    {
        public const string PendingPayment = "pending_payment";
        public const string Paid           = "paid";
        public const string Processing     = "processing";
        public const string Shipped        = "shipped";
        public const string Completed      = "completed";
        public const string Cancelled      = "cancelled";
    }
}
