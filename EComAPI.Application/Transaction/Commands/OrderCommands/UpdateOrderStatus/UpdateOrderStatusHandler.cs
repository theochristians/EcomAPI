using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Commands.OrderCommands.UpdateOrderStatus
{
    public class UpdateOrderStatusHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderStatusLogRepository _orderStatusLogRepository;
        private readonly IStockLogRepository _stockLogRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderStatusHandler(
            IOrderRepository orderRepository,
            IOrderStatusLogRepository orderStatusLogRepository,
            IStockLogRepository stockLogRepository,
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _orderStatusLogRepository = orderStatusLogRepository;
            _stockLogRepository = stockLogRepository;
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            UpdateOrderStatusCommand updateOrderStatusCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                var order = await _orderRepository.GetOrderByIdAsync(
                    updateOrderStatusCommand.OrderId, cancellationToken);

                if (order == null)
                    return Result<Guid>.Failure("Order not found");

                switch (updateOrderStatusCommand.NewStatus)
                {
                    case OrderStatus.Processing:
                        order.MarkAsProcessing(_currentUser.UserId);
                        break;

                    case OrderStatus.Shipped:
                        if (string.IsNullOrWhiteSpace(updateOrderStatusCommand.Courier))
                            return Result<Guid>.Failure("Courier is required when shipping");
                        if (string.IsNullOrWhiteSpace(updateOrderStatusCommand.TrackingNumber))
                            return Result<Guid>.Failure("TrackingNumber is required when shipping");

                        order.MarkAsShipped(
                            updateOrderStatusCommand.Courier,
                            updateOrderStatusCommand.TrackingNumber,
                            _currentUser.UserId);
                        break;

                    case OrderStatus.Completed:
                        order.MarkAsCompleted(_currentUser.UserId);
                        break;

                    case OrderStatus.Cancelled:
                        order.Cancel(_currentUser.UserId, updateOrderStatusCommand.AdminNote);

                        // Restore stock for each order item
                        var orderWithItems = await _orderRepository.GetOrderByIdWithItemsAsync(
                            order.Id, cancellationToken);

                        if (orderWithItems?.Items != null)
                        {
                            foreach (var item in orderWithItems.Items)
                            {
                                var variant = await _productRepository.GetProductVariantByIdAsync(
                                    item.ProductVariantId, cancellationToken);

                                if (variant != null)
                                {
                                    var stockBefore = variant.Stock;
                                    variant.IncreaseStock(item.Quantity, _currentUser.UserId);
                                    await _productRepository.UpdateProductVariantAsync(variant, cancellationToken);

                                    var stockLog = new StockLog(
                                        productVariantId: variant.Id,
                                        type: "cancel",
                                        quantityChange: item.Quantity,
                                        stockBefore: stockBefore,
                                        stockAfter: variant.Stock,
                                        createdBy: _currentUser.UserId,
                                        referenceType: "Order",
                                        referenceId: order.Id,
                                        note: $"Stock restored for cancelled order {order.OrderNumber}");

                                    await _stockLogRepository.AddLogAsync(stockLog, cancellationToken);
                                }
                            }
                        }
                        break;

                    default:
                        return Result<Guid>.Failure($"Cannot manually set status to '{updateOrderStatusCommand.NewStatus}'");
                }

                await _orderRepository.UpdateOrderAsync(order, cancellationToken);

                // Log status change
                var statusLog = new OrderStatusLog(
                    orderId: order.Id,
                    status: updateOrderStatusCommand.NewStatus,
                    createdBy: _currentUser.UserId,
                    note: updateOrderStatusCommand.AdminNote);

                await _orderStatusLogRepository.AddLogAsync(statusLog, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(order.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<Guid>.Failure("An error occurred while updating the order status");
            }
        }
    }
}
