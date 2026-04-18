using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Interfaces
{
    public interface IOrderRepository
    {
        // ORDER
        Task<Order?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Order?> GetOrderByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Order?> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Order> Items, int TotalCount)> GetOrdersByUserIdAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Order> Items, int TotalCount)> GetAllOrdersPaginatedAsync(
            string? status,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
        Task AddOrderAsync(Order order, CancellationToken cancellationToken = default);
        Task UpdateOrderAsync(Order order, CancellationToken cancellationToken = default);
        Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default);
        Task<bool> UserHasCompletedOrderForProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
        Task<bool> UserHasCompletedOrderForProductInOrderAsync(
            Guid userId,
            Guid orderId,
            Guid productId,
            CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, Guid>> GetProductIdsByVariantIdsAsync(IEnumerable<Guid> variantIds, CancellationToken cancellationToken = default);

        // ORDER ITEM
        Task AddOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken = default);

        // PAYMENT
        Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Review>> GetReviewsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Return>> GetReturnsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default);
        Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default);
    }
}
