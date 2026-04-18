using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Commands.PaymentCommands.ConfirmPayment
{
    public class ConfirmPaymentHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderStatusLogRepository _orderStatusLogRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public ConfirmPaymentHandler(
            IOrderRepository orderRepository,
            IOrderStatusLogRepository orderStatusLogRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _orderStatusLogRepository = orderStatusLogRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            ConfirmPaymentCommand confirmPaymentCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                var order = await _orderRepository.GetOrderByIdAsync(
                    confirmPaymentCommand.OrderId, cancellationToken);

                if (order == null)
                    return Result<Guid>.Failure("Order not found");

                var payment = await _orderRepository.GetPaymentByOrderIdAsync(
                    confirmPaymentCommand.OrderId, cancellationToken);

                if (payment == null)
                    return Result<Guid>.Failure("Payment record not found");

                if (string.IsNullOrWhiteSpace(payment.ProofImageUrl))
                    return Result<Guid>.Failure("Payment proof has not been submitted yet");

                // Confirm payment
                payment.Confirm(_currentUser.UserId, confirmPaymentCommand.AdminNote);
                await _orderRepository.UpdatePaymentAsync(payment, cancellationToken);

                // Update order status to Paid
                order.MarkAsPaid(_currentUser.UserId);
                await _orderRepository.UpdateOrderAsync(order, cancellationToken);

                // Log status change
                var statusLog = new OrderStatusLog(
                    orderId: order.Id,
                    status: OrderStatus.Paid,
                    createdBy: _currentUser.UserId,
                    note: confirmPaymentCommand.AdminNote);

                await _orderStatusLogRepository.AddLogAsync(statusLog, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(payment.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<Guid>.Failure("An error occurred while confirming the payment");
            }
        }
    }
}
