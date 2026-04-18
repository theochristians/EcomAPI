using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Transaction.Commands.PaymentCommands.SubmitPaymentProof
{
    public class SubmitPaymentProofHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public SubmitPaymentProofHandler(
            IOrderRepository orderRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            SubmitPaymentProofCommand submitPaymentProofCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                var order = await _orderRepository.GetOrderByIdAsync(
                    submitPaymentProofCommand.OrderId, cancellationToken);

                if (order == null)
                    return Result<Guid>.Failure("Order not found");

                if (order.UserId != _currentUser.UserId)
                    return Result<Guid>.Failure("You are not authorized to update this order's payment");

                var payment = await _orderRepository.GetPaymentByOrderIdAsync(
                    submitPaymentProofCommand.OrderId, cancellationToken);

                if (payment == null)
                    return Result<Guid>.Failure("Payment record not found");

                if (!string.IsNullOrWhiteSpace(submitPaymentProofCommand.PaymentMethod))
                    payment.SetPaymentMethod(submitPaymentProofCommand.PaymentMethod, _currentUser.UserId);

                payment.SetProofImage(submitPaymentProofCommand.ProofImageUrl, _currentUser.UserId);

                await _orderRepository.UpdatePaymentAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(payment.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<Guid>.Failure("An error occurred while submitting payment proof");
            }
        }
    }
}
