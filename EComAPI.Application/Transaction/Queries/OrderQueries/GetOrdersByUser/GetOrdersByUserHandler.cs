using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.DTOs;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Transaction.Queries.OrderQueries.GetOrdersByUser
{
    public class GetOrdersByUserHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUser _currentUser;

        public GetOrdersByUserHandler(
            IOrderRepository orderRepository,
            ICurrentUser currentUser)
        {
            _orderRepository = orderRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<(IReadOnlyList<OrderDto> Items, int TotalCount)>> Handle(
            GetOrdersByUserQuery getOrdersByUserQuery,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<(IReadOnlyList<OrderDto>, int)>.Failure("User not authenticated");

                var (orders, totalCount) = await _orderRepository.GetOrdersByUserIdAsync(
                    getOrdersByUserQuery.UserId,
                    getOrdersByUserQuery.Page,
                    getOrdersByUserQuery.PageSize,
                    cancellationToken);

                var allVariantIds = orders
                    .SelectMany(order => order.Items)
                    .Where(orderItem => orderItem.DeletedAt == null)
                    .Select(orderItem => orderItem.ProductVariantId)
                    .Distinct();

                var variantProductMap = await _orderRepository
                    .GetProductIdsByVariantIdsAsync(allVariantIds, cancellationToken);

                var orderDtos = orders
                    .Select(order => new OrderDto(
                        order.Id,
                        order.OrderNumber,
                        order.Status,
                        order.ShippingRecipientName,
                        order.ShippingPhone,
                        order.ShippingFullAddress,
                        order.ShippingCity,
                        order.ShippingPostalCode,
                        order.TotalAmount,
                        order.ShippingCost,
                        order.DiscountAmount,
                        order.FinalAmount,
                        order.Courier,
                        order.TrackingNumber,
                        order.CustomerNote,
                        order.AdminNote,
                        order.CreatedAt,
                        order.Items
                            .Where(orderItem => orderItem.DeletedAt == null)
                            .Select(orderItem => new OrderItemDto(
                                orderItem.Id,
                                variantProductMap.GetValueOrDefault(orderItem.ProductVariantId),
                                orderItem.ProductVariantId,
                                orderItem.SnapshotProductName,
                                orderItem.SnapshotVariantName,
                                orderItem.SnapshotPrice,
                                orderItem.Quantity))
                            .ToList(),
                        null,
                        new List<ReviewDto>(),
                        new List<ReturnDto>()))
                    .ToList();

                return Result<(IReadOnlyList<OrderDto>, int)>.Success((orderDtos, totalCount));
            }
            catch (DomainException domainException)
            {
                return Result<(IReadOnlyList<OrderDto>, int)>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<(IReadOnlyList<OrderDto>, int)>.Failure("An error occurred while retrieving orders");
            }
        }
    }
}
