using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.DTOs;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Transaction.Queries.ReturnQueries.GetReturnsByUser
{
    public class GetReturnsByUserHandler
    {
        private readonly IReturnRepository _returnRepository;
        private readonly ICurrentUser _currentUser;

        public GetReturnsByUserHandler(
            IReturnRepository returnRepository,
            ICurrentUser currentUser)
        {
            _returnRepository = returnRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<(IReadOnlyList<ReturnDto> Items, int TotalCount)>> Handle(
            GetReturnsByUserQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<(IReadOnlyList<ReturnDto>, int)>.Failure("User not authenticated");

                var (returns, totalCount) = await _returnRepository.GetReturnsByUserIdAsync(
                    _currentUser.UserId, query.Page, query.PageSize, cancellationToken);

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
                            .ToList()))
                    .ToList();

                return Result<(IReadOnlyList<ReturnDto>, int)>.Success((returnDtos, totalCount));
            }
            catch (DomainException domainException)
            {
                return Result<(IReadOnlyList<ReturnDto>, int)>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<(IReadOnlyList<ReturnDto>, int)>.Failure("An error occurred while retrieving returns");
            }
        }
    }
}
