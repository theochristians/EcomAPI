using EComAPI.Application.Common.Authorization;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.DTOs;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Transaction.Queries.ReturnQueries.GetReturnById
{
    public class GetReturnByIdHandler
    {
        private readonly IReturnRepository _returnRepository;
        private readonly ICurrentUser _currentUser;

        public GetReturnByIdHandler(
            IReturnRepository returnRepository,
            ICurrentUser currentUser)
        {
            _returnRepository = returnRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<ReturnDto>> Handle(
            GetReturnByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<ReturnDto>.Failure("User not authenticated");

                var returnEntity = await _returnRepository.GetReturnWithDetailsAsync(query.ReturnId, cancellationToken);
                if (returnEntity == null)
                    return Result<ReturnDto>.Failure("Return not found");

                var canReadAllOrders = _currentUser.HasPermission(Permissions.Orders.ReadAll);

                if (!canReadAllOrders && returnEntity.UserId != _currentUser.UserId)
                    return Result<ReturnDto>.Failure("Return not found");

        var returnDto = new ReturnDto(
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
                .Where(returnItem => returnItem.DeletedAt == null)
                .Select(returnItem => new ReturnItemDto(
                    returnItem.Id,
                    returnItem.OrderItemId,
                    returnItem.Quantity,
                    returnItem.Condition,
                    returnItem.AdminNote))
                .ToList(),
            returnEntity.Images.Select(returnImage => new ReturnImageDto(
                returnImage.Id,
                returnImage.ImageUrl,
                returnImage.Description,
                returnImage.CreatedAt))
                .ToList());

        return Result<ReturnDto>.Success(returnDto);
            }
            catch (DomainException domainException)
            {
                return Result<ReturnDto>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<ReturnDto>.Failure("An error occurred while retrieving the return");
            }
        }
    }
}
