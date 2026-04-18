using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Commands.ReturnCommands.ApproveReturn
{
    public class ApproveReturnHandler
    {
        private readonly IReturnRepository _returnRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockLogRepository _stockLogRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public ApproveReturnHandler(
            IReturnRepository returnRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IStockLogRepository stockLogRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _returnRepository = returnRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _stockLogRepository = stockLogRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(
            ApproveReturnCommand approveReturnCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<bool>.Failure("User not authenticated");

                var returnEntity = await _returnRepository.GetReturnWithDetailsAsync(approveReturnCommand.ReturnId, cancellationToken);
                if (returnEntity == null)
                    return Result<bool>.Failure("Return not found");

                // Approve the return (domain validates status transition)
                returnEntity.Approve(approveReturnCommand.RefundAmount, _currentUser.UserId);

                // Update item conditions if provided
                if (approveReturnCommand.ItemConditions != null)
                {
                    foreach (var itemCondition in approveReturnCommand.ItemConditions)
                    {
                        var returnItem = returnEntity.Items.FirstOrDefault(i => i.Id == itemCondition.ReturnItemId);
                        if (returnItem == null)
                            return Result<bool>.Failure($"Return item {itemCondition.ReturnItemId} not found");

                        returnItem.SetCondition(itemCondition.Condition, _currentUser.UserId);

                        if (!string.IsNullOrWhiteSpace(itemCondition.AdminNote))
                            returnItem.SetAdminNote(itemCondition.AdminNote, _currentUser.UserId);
                    }
                }

                // Restore stock for returned items
                var order = await _orderRepository.GetOrderByIdWithItemsAsync(
                    returnEntity.OrderId, cancellationToken);

                if (order?.Items != null)
                {
                    var orderItemMap = order.Items.ToDictionary(oi => oi.Id);

                    foreach (var returnItem in returnEntity.Items)
                    {
                        if (orderItemMap.TryGetValue(returnItem.OrderItemId, out var orderItem))
                        {
                            var variant = await _productRepository.GetProductVariantByIdAsync(
                                orderItem.ProductVariantId, cancellationToken);

                            if (variant != null)
                            {
                                var stockBefore = variant.Stock;
                                variant.IncreaseStock(returnItem.Quantity, _currentUser.UserId);
                                await _productRepository.UpdateProductVariantAsync(variant, cancellationToken);

                                var stockLog = new StockLog(
                                    productVariantId: variant.Id,
                                    type: "return",
                                    quantityChange: returnItem.Quantity,
                                    stockBefore: stockBefore,
                                    stockAfter: variant.Stock,
                                    createdBy: _currentUser.UserId,
                                    referenceType: "Return",
                                    referenceId: returnEntity.Id,
                                    note: $"Stock restored for approved return {returnEntity.ReturnNumber}");

                                await _stockLogRepository.AddLogAsync(stockLog, cancellationToken);
                            }
                        }
                    }
                }

                await _returnRepository.UpdateReturnAsync(returnEntity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (DomainException domainException)
            {
                return Result<bool>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<bool>.Failure("An error occurred while approving the return");
            }
        }
    }
}
