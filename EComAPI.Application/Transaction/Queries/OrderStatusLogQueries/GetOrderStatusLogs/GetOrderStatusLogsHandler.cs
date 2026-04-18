using EComAPI.Application.Common.Authorization;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.DTOs;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Transaction.Queries.OrderStatusLogQueries.GetOrderStatusLogs
{
    public class GetOrderStatusLogsHandler
    {
        private readonly IOrderStatusLogRepository _orderStatusLogRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUser _currentUser;

        public GetOrderStatusLogsHandler(
            IOrderStatusLogRepository orderStatusLogRepository,
            IOrderRepository orderRepository,
            ICurrentUser currentUser)
        {
            _orderStatusLogRepository = orderStatusLogRepository;
            _orderRepository = orderRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<IReadOnlyList<OrderStatusLogDto>>> Handle(
            GetOrderStatusLogsQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<IReadOnlyList<OrderStatusLogDto>>.Failure("User not authenticated");

                var order = await _orderRepository.GetOrderByIdAsync(query.OrderId, cancellationToken);
                if (order == null)
                    return Result<IReadOnlyList<OrderStatusLogDto>>.Failure("Order not found");

                var canReadAllOrders = _currentUser.HasPermission(Permissions.Orders.ReadAll);

                if (!canReadAllOrders && order.UserId != _currentUser.UserId)
                    return Result<IReadOnlyList<OrderStatusLogDto>>.Failure("Order not found");

                var logs = await _orderStatusLogRepository.GetLogsByOrderIdAsync(
                    query.OrderId, cancellationToken);

                var logDtos = logs
                    .Select(statusLog => new OrderStatusLogDto(
                        statusLog.Id,
                        statusLog.OrderId,
                        statusLog.Status,
                        statusLog.Note,
                        statusLog.ChangedAt,
                        statusLog.CreatedBy))
                    .ToList();

                return Result<IReadOnlyList<OrderStatusLogDto>>.Success(logDtos);
            }
            catch (DomainException domainException)
            {
                return Result<IReadOnlyList<OrderStatusLogDto>>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<IReadOnlyList<OrderStatusLogDto>>.Failure("An error occurred while retrieving order status logs");
            }
        }
    }
}
