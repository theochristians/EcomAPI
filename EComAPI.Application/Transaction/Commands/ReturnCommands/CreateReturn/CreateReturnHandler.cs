using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Commands.ReturnCommands.CreateReturn
{
    public class CreateReturnHandler
    {
        private readonly IReturnRepository _returnRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateReturnHandler(
            IReturnRepository returnRepository,
            IOrderRepository orderRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _returnRepository = returnRepository;
            _orderRepository = orderRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateReturnCommand createReturnCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                // Get the order with items
                var order = await _orderRepository.GetOrderByIdWithItemsAsync(createReturnCommand.OrderId, cancellationToken);
                if (order == null)
                    return Result<Guid>.Failure("Order not found");

                // Verify ownership
                if (order.UserId != _currentUser.UserId)
                    return Result<Guid>.Failure("You can only request returns for your own orders");

                // Verify order is completed
                if (order.Status != OrderStatus.Completed)
                    return Result<Guid>.Failure("Returns can only be requested for completed orders");

                // Check if return already exists for this order
                var returnExists = await _returnRepository.ReturnExistsForOrderAsync(createReturnCommand.OrderId, cancellationToken);
                if (returnExists)
                    return Result<Guid>.Failure("A return request already exists for this order");

                // Validate items
                if (createReturnCommand.Items == null || createReturnCommand.Items.Count == 0)
                    return Result<Guid>.Failure("At least one return item is required");

                // Validate each item belongs to the order and quantity doesn't exceed ordered quantity
                foreach (var itemDto in createReturnCommand.Items)
                {
                    var orderItem = order.Items.FirstOrDefault(oi => oi.Id == itemDto.OrderItemId);
                    if (orderItem == null)
                        return Result<Guid>.Failure($"Order item {itemDto.OrderItemId} does not belong to this order");

                    if (itemDto.Quantity > orderItem.Quantity)
                        return Result<Guid>.Failure($"Return quantity ({itemDto.Quantity}) exceeds ordered quantity ({orderItem.Quantity}) for item {itemDto.OrderItemId}");
                }

                // Validate images count
                if (createReturnCommand.Images != null && createReturnCommand.Images.Count > 10)
                    return Result<Guid>.Failure("Maximum 10 images per return request");

                // Generate return number
                var returnNumber = await _returnRepository.GenerateReturnNumberAsync(cancellationToken);

                // Create return entity
                var returnEntity = new Return(
                    orderId: createReturnCommand.OrderId,
                    userId: _currentUser.UserId,
                    returnNumber: returnNumber,
                    reason: createReturnCommand.Reason,
                    createdBy: _currentUser.UserId,
                    bankName: createReturnCommand.BankName,
                    bankAccountNumber: createReturnCommand.BankAccountNumber,
                    accountHolderName: createReturnCommand.AccountHolderName);

                await _returnRepository.AddReturnAsync(returnEntity, cancellationToken);

                // Add return items
                foreach (var itemDto in createReturnCommand.Items)
                {
                    var returnItem = new ReturnItem(
                        returnId: returnEntity.Id,
                        orderItemId: itemDto.OrderItemId,
                        quantity: itemDto.Quantity,
                        createdBy: _currentUser.UserId);

                    returnEntity.AddItem(returnItem);
                }

                // Add images
                if (createReturnCommand.Images != null)
                {
                    foreach (var imageDto in createReturnCommand.Images)
                    {
                        var returnImage = new ReturnImage(
                            returnId: returnEntity.Id,
                            imageUrl: imageDto.ImageUrl,
                            description: imageDto.Description);

                        returnEntity.AddImage(returnImage);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(returnEntity.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<Guid>.Failure("An error occurred while creating the return request");
            }
        }
    }
}
