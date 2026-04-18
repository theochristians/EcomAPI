using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Transaction.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Transaction.Persistence.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _appDbContext;

        public OrderRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // ORDER
        public async Task<Order?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Orders
                .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
        }

        public async Task<Order?> GetOrderByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Orders
                .Include(order => order.Items)
                .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
        }

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Orders
                .Include(order => order.Items)
                .FirstOrDefaultAsync(order => order.OrderNumber == orderNumber, cancellationToken);
        }

        public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetOrdersByUserIdAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _appDbContext.Orders
                .Where(order => order.UserId == userId)
                .AsQueryable();

            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .OrderByDescending(order => order.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(order => order.Items)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (orders, totalCount);
        }

        public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetAllOrdersPaginatedAsync(
            string? status,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _appDbContext.Orders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(order => order.Status == status);

            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .OrderByDescending(order => order.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(order => order.Items)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (orders, totalCount);
        }

        public async Task AddOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Orders.AddAsync(order, cancellationToken);
        }

        public Task UpdateOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            _appDbContext.Orders.Update(order);
            return Task.CompletedTask;
        }

        public async Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Orders
                .AnyAsync(order => order.OrderNumber == orderNumber, cancellationToken);
        }

        public async Task<bool> UserHasCompletedOrderForProductAsync(
            Guid userId, Guid productId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Orders
                .Where(order => order.UserId == userId && order.Status == OrderStatus.Completed)
                .SelectMany(order => order.Items)
                .Join(
                    _appDbContext.ProductVariants,
                    item => item.ProductVariantId,
                    variant => variant.Id,
                    (item, variant) => variant.ProductId)
                .AnyAsync(pid => pid == productId, cancellationToken);
        }

        public async Task<bool> UserHasCompletedOrderForProductInOrderAsync(
            Guid userId,
            Guid orderId,
            Guid productId,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Orders
                .Where(order =>
                    order.Id == orderId &&
                    order.UserId == userId &&
                    order.Status == OrderStatus.Completed)
                .SelectMany(order => order.Items)
                .Join(
                    _appDbContext.ProductVariants,
                    item => item.ProductVariantId,
                    variant => variant.Id,
                    (item, variant) => variant.ProductId)
                .AnyAsync(pid => pid == productId, cancellationToken);
        }

        public async Task<Dictionary<Guid, Guid>> GetProductIdsByVariantIdsAsync(
            IEnumerable<Guid> variantIds, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.ProductVariants
                .Where(productVariant => variantIds.Contains(productVariant.Id))
                .ToDictionaryAsync(
                    productVariant => productVariant.Id,
                    productVariant => productVariant.ProductId,
                    cancellationToken);
        }

        // ORDER ITEM
        public async Task AddOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            await _appDbContext.OrderItems.AddAsync(orderItem, cancellationToken);
        }

        // PAYMENT
        public async Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Payments
                .FirstOrDefaultAsync(payment => payment.OrderId == orderId, cancellationToken);
        }

        public async Task<IReadOnlyList<Review>> GetReviewsByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Reviews
                .Where(review => review.OrderId == orderId)
                .Include(review => review.Images)
                .OrderByDescending(review => review.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Return>> GetReturnsByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Returns
                .Where(returnEntity => returnEntity.OrderId == orderId)
                .Include(returnEntity => returnEntity.Items)
                .Include(returnEntity => returnEntity.Images)
                .OrderByDescending(returnEntity => returnEntity.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Payments.AddAsync(payment, cancellationToken);
        }

        public Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            _appDbContext.Payments.Update(payment);
            return Task.CompletedTask;
        }
    }
}
