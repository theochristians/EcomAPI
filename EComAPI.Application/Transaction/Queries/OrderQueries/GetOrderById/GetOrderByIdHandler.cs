using EComAPI.Application.Common.Authorization;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.DTOs;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Transaction.Queries.OrderQueries.GetOrderById
{
    public class GetOrderByIdHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUser _currentUser;

        public GetOrderByIdHandler(
            IOrderRepository orderRepository,
            ICurrentUser currentUser)
        {
            _orderRepository = orderRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<OrderDto>> Handle(
            GetOrderByIdQuery getOrderByIdQuery,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<OrderDto>.Failure("User not authenticated");

                var order = await _orderRepository.GetOrderByIdWithItemsAsync(
                    getOrderByIdQuery.OrderId, cancellationToken);

                if (order == null)
                    return Result<OrderDto>.Failure("Order not found");

                var canReadAllOrders = _currentUser.HasPermission(Permissions.Orders.ReadAll);

                if (!canReadAllOrders && order.UserId != _currentUser.UserId)
                    return Result<OrderDto>.Failure("Order not found");

                var payment = await _orderRepository.GetPaymentByOrderIdAsync(
                    order.Id, cancellationToken);

                var reviews = await _orderRepository.GetReviewsByOrderIdAsync(
                    order.Id, cancellationToken);

                var returns = await _orderRepository.GetReturnsByOrderIdAsync(
                    order.Id, cancellationToken);

                var variantIds = order.Items
                    .Where(orderItem => orderItem.DeletedAt == null)
                    .Select(orderItem => orderItem.ProductVariantId)
                    .Distinct();

                var variantProductMap = await _orderRepository
                    .GetProductIdsByVariantIdsAsync(variantIds, cancellationToken);

                var orderItemDtos = order.Items
                    .Where(orderItem => orderItem.DeletedAt == null)
                    .Select(orderItem => new OrderItemDto(
                        orderItem.Id,
                        variantProductMap.GetValueOrDefault(orderItem.ProductVariantId),
                        orderItem.ProductVariantId,
                        orderItem.SnapshotProductName,
                        orderItem.SnapshotVariantName,
                        orderItem.SnapshotPrice,
                        orderItem.Quantity))
                    .ToList();

                PaymentDto? paymentDto = null;
                if (payment != null)
                {
                    paymentDto = new PaymentDto(
                        payment.Id,
                        payment.PaymentMethod,
                        payment.Amount,
                        payment.Status,
                        payment.ProofImageUrl,
                        payment.AdminNote,
                        payment.ConfirmedAt);
                }

                var reviewDtos = reviews
                    .Select(review => new ReviewDto(
                        review.Id,
                        review.UserId,
                        review.OrderId,
                        review.ProductId,
                        review.Rating,
                        review.Comment,
                        review.CreatedAt,
                        review.Images
                            .Select(reviewImage => new ReviewImageDto(
                                reviewImage.Id,
                                reviewImage.ImageUrl,
                                reviewImage.DisplayOrder))
                            .ToList()))
                    .ToList();

                var returnDtos = returns
                    .Select(returnEntity => new ReturnDto(
                        returnEntity.Id,
                        returnEntity.OrderId,
                        returnEntity.UserId,
                        returnEntity.ReturnNumber,
                        returnEntity.Reason,
                        returnEntity.Status,
                        returnEntity.RequestedAt,
                        returnEntity.ApprovedAt,
                        returnEntity.ApprovedBy,
                        returnEntity.RefundAmount,
                        returnEntity.BankName,
                        returnEntity.BankAccountNumber,
                        returnEntity.AccountHolderName,
                        returnEntity.RefundDate,
                        returnEntity.CreatedAt,
                        returnEntity.Items
                            .Select(returnItem => new ReturnItemDto(
                                returnItem.Id,
                                returnItem.OrderItemId,
                                returnItem.Quantity,
                                returnItem.Condition,
                                returnItem.AdminNote))
                            .ToList(),
                        returnEntity.Images
                            .Select(returnImage => new ReturnImageDto(
                                returnImage.Id,
                                returnImage.ImageUrl,
                                returnImage.Description,
                                returnImage.CreatedAt))
                            .ToList()))
                    .ToList();

                var orderDto = new OrderDto(
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
                    orderItemDtos,
                    paymentDto,
                    reviewDtos,
                    returnDtos);

                return Result<OrderDto>.Success(orderDto);
            }
            catch (DomainException domainException)
            {
                return Result<OrderDto>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<OrderDto>.Failure("An error occurred while retrieving the order");
            }
        }
    }
}
